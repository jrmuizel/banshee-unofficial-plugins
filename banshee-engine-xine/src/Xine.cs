/* -*- Mode:csharp; tab-width: 4; indent-tabs-mode: nil; c-basic-offset: 4 -*- */
/***************************************************************************
 *  Xine.cs
 *
 *  Copyright (C) 2006 Ivan N. Zlatev
 *  Written by Ivan N. Zlatev <contact i-nZ.net>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using System.IO;

namespace Xine
{
    public class XineEngine : IDisposable
    {
        private static IntPtr _engine;
        
        public XineEngine ()
        {
            string configFile = ".xine" + Path.DirectorySeparatorChar + "config";
            
            _engine = xine_new ();
            if (_engine == IntPtr.Zero) {
                throw new InvalidOperationException ("Unable to initalize xine-engine");
            }

            string configPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), configFile);
            xine_config_load (_engine, configPath);
            xine_init (_engine);

        }

        public Stream CreateStream ()
        {
            return new Stream (_engine);
        }
        
        public Stream CreateStream (string mrl)
        {
            return new Stream (_engine, mrl);
        }
        
        public void Dispose ()
        {
            if (_engine != IntPtr.Zero) {
                xine_exit (_engine);
                _engine = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }

        ~XineEngine ()
        {
            Dispose ();
        }

        [DllImport ("libxine")]
        private  static extern IntPtr xine_new ();

        [DllImport ("libxine")]
        private static extern void xine_init (IntPtr engineHandle);

        [DllImport ("libxine")]
        private static extern void xine_exit (IntPtr engineHandle);
        
        [DllImport ("libxine")]
        private static extern void xine_config_load (IntPtr engineHandle, string configFile);
        
    }


    public class Stream : IDisposable
    {
        public delegate void XineEventHandler (IntPtr data, IntPtr eventArgs);

        public static event EventHandler EndOfStream;
        
        public enum State {
            Idle,
            Playing,
            Paused,
            Buffering
        };

        private IntPtr _engine;
        private State _state;
        private IntPtr _stream;
        private IntPtr _eventQueue;
        private IntPtr _audioPort;


                
        internal Stream (IntPtr engine) : this (engine, null)
        {
        }
            
        internal Stream (IntPtr engine, string mrl)
        {
            _engine = engine;
            
            _audioPort = xine_open_audio_driver (_engine, "auto", IntPtr.Zero);
            if (_audioPort == IntPtr.Zero) {
                throw new ApplicationException ("Unable to load the audio driver.");
            }
            
            _stream = xine_stream_new (_engine, _audioPort, IntPtr.Zero);
            _eventQueue = xine_event_new_queue (_stream);   

            if (mrl != null) {
                Open (mrl);
            }
            _state = State.Idle;
        }
    
        public void ProcessEventQueue()
        {
            if(_eventQueue == IntPtr.Zero) {
                return;
            }
            
            while(true) {
                IntPtr eventArgs = xine_event_get(_eventQueue);
                if (eventArgs == IntPtr.Zero) {
                    return;
                }

                int eventType = Marshal.ReadInt32(eventArgs);

                switch (eventType) {
                case XINE_EVENT_UI_PLAYBACK_FINISHED:
                    if (EndOfStream != null) {
                        EndOfStream (this, EventArgs.Empty);
                    }
                    break;
                }

                xine_event_free (eventArgs);
            }
        }
                                  
        public void Open (string mrl)
        {
            if (mrl == null || mrl == String.Empty) {
                throw new ArgumentException ("mrl");
            }
            xine_open (_stream, mrl);
            _state = State.Idle;
        }

        public void Close ()
        {
            if (_stream != IntPtr.Zero) {
                xine_close (_stream);
                _state = State.Idle;
            }
        }
        
        public void Play ()
        {
            if (_stream != IntPtr.Zero) {
                if (_state == State.Paused) {
                    xine_set_param (_stream, XINE_PARAM_SPEED, XINE_SPEED_NORMAL);
                    _state = State.Playing;
                }
                else {
                    if (xine_play (_stream, 0, 0) == 0) {
                       _state = State.Idle;
                    }
                    else {
                        _state = State.Playing;
                    }
                }
                
            }
        }

        public void Pause ()
        {
            if (_stream != IntPtr.Zero) {
                xine_set_param (_stream, XINE_PARAM_SPEED, XINE_SPEED_PAUSE);
                _state = State.Paused;
            }
        }

        public void Stop ()
        {
            if (_stream != IntPtr.Zero) {
                xine_stop (_stream);
                _state = State.Idle;
            }
        }         



        
        // value is between -100 and +100.
        // frequancy in hz 
        //
        public void SetEqualizerGain (uint frequency, int value)
        {
            if (_stream == IntPtr.Zero) {
                return;
            }        
            if (value < -100 || value > 100) {
                throw new ArgumentOutOfRangeException ("value");
            }

            switch (frequency) {
            case 30:
                xine_set_param (_stream, XINE_PARAM_EQ_30HZ, value);
                break;
            case 60:
                xine_set_param (_stream, XINE_PARAM_EQ_60HZ, value);
                break;
            case 125:
                xine_set_param (_stream, XINE_PARAM_EQ_125HZ, value);
                break;
            case 250:
                xine_set_param (_stream, XINE_PARAM_EQ_250HZ, value);
                break;
            case 500:
                xine_set_param (_stream, XINE_PARAM_EQ_500HZ, value);
                break;
            case 1000:
                xine_set_param (_stream, XINE_PARAM_EQ_1000HZ, value);
                break;
            case 2000:
                xine_set_param (_stream, XINE_PARAM_EQ_2000HZ, value);
                break;
            case 4000:
                xine_set_param (_stream, XINE_PARAM_EQ_4000HZ, value);
                break;
            case 80000:
                xine_set_param (_stream, XINE_PARAM_EQ_8000HZ, value);
                break;
            case 16000:
                xine_set_param (_stream, XINE_PARAM_EQ_16000HZ, value);
                break;
            }
        }
                
        public uint[] EqualizerFrequencies
        {
            get {
                return new uint[] { 30, 60, 125, 250, 500, 1000, 2000, 4000, 80000, 16000 };
            }
        }

        // Xine Default: 100%
        // Xine Range: 0 - 200%
        //
        public uint AmplifierLevel
        {
            get {
                uint level = 0;
                if (_stream != IntPtr.Zero) {
                     level = (uint) xine_get_param (_stream, XINE_PARAM_AUDIO_AMP_LEVEL);
                }
                return level;
            }
            set {
                if (value > 200) {
                    throw new ArgumentOutOfRangeException ("AmplifierLevel");
                }
                if (_stream != IntPtr.Zero) {
                    xine_set_param (_stream, XINE_PARAM_AUDIO_AMP_LEVEL, (int) value);
                }
            }
        }
        
        public uint Length {
            get {
                int length, positionTime, positionStream;
                length = positionTime = positionStream = 0;
                
                xine_get_pos_length (_stream, ref positionStream, ref positionTime, ref length);
                return (uint) (length / 1000);
            }
        }           

        public uint Position {
            get {
                int length, positionTime, positionStream;
                length = positionTime = positionStream = 0;
                
                xine_get_pos_length (_stream, ref positionStream, ref positionTime, ref length);                
                return (uint) positionTime / 1000;
            }
            set {
                xine_play (_stream, 0, (int) value * 1000);
            }
        }
        
        public bool CanSeek {
            get {
                if (_stream != IntPtr.Zero) {
                    if (xine_get_stream_info (_stream, XINE_STREAM_INFO_SEEKABLE) == 1) {
                        return true;
                    }
                }
                return false;
            }
        }          

        public State CurrentState
        {
            get {
                return _state;
            }               
        }

        public uint Volume {
            get {
                if (_stream != IntPtr.Zero) {
                    return (uint) xine_get_param (_stream, XINE_PARAM_AUDIO_VOLUME);
                }
                return 0;
            }
            set {
                if (value <= 100 && _stream != IntPtr.Zero) {
                    xine_set_param (_stream, XINE_PARAM_AUDIO_VOLUME, (int) value);
                }
            }
        }

        public void Dispose ()
        {
            if (_stream != IntPtr.Zero) {
                xine_close (_stream);
                xine_dispose (_stream);
                _stream = IntPtr.Zero;
                
                if (_audioPort != IntPtr.Zero) {
                    xine_close_audio_driver (_engine, _audioPort);
                    _audioPort = IntPtr.Zero;
                }
                if (_eventQueue != IntPtr.Zero) {
                    xine_event_dispose_queue (_eventQueue);
                    _eventQueue = IntPtr.Zero;
                }
            }
        }

        ~Stream ()
        {
            Dispose ();
        }



        private const int XINE_SPEED_PAUSE = 0;
        private const int XINE_SPEED_NORMAL = 4;
        private const int XINE_PARAM_AUDIO_VOLUME = 6;
        private const int XINE_STREAM_INFO_SEEKABLE = 1;
        private const int XINE_EVENT_UI_PLAYBACK_FINISHED = 1;
        private const int XINE_TRICK_MODE_SEEK_TO_TIME = 2;
        private const int XINE_PARAM_SPEED = 1;
        private const int XINE_PARAM_AUDIO_AMP_LEVEL = 9;
        private const int XINE_PARAM_EQ_30HZ = 18;
        private const int XINE_PARAM_EQ_60HZ = 19;
        private const int XINE_PARAM_EQ_125HZ = 20;
        private const int XINE_PARAM_EQ_250HZ = 21;
        private const int XINE_PARAM_EQ_500HZ = 22;
        private const int XINE_PARAM_EQ_1000HZ = 23;
        private const int XINE_PARAM_EQ_2000HZ = 24;
        private const int XINE_PARAM_EQ_4000HZ = 25;
        private const int XINE_PARAM_EQ_8000HZ = 26;
        private const int XINE_PARAM_EQ_16000HZ = 27;
        
        [DllImport ("libxine")]
        private static extern IntPtr xine_open_audio_driver (IntPtr engine, string driverId, IntPtr data);

        [DllImport ("libxine")]
        private static extern void xine_close_audio_driver (IntPtr engine, IntPtr audioPort);
        
        [DllImport ("libxine")]
        private static extern IntPtr xine_stream_new (IntPtr engine, IntPtr audioPort, IntPtr videoPort);

        [DllImport ("libxine")]
        private static extern int xine_open (IntPtr stream, string mrl);

        [DllImport ("libxine")]
        private static extern void xine_close (IntPtr stream);

        [DllImport ("libxine")]
        private static extern int xine_play (IntPtr stream, int startPosition, int startTime);

        [DllImport ("libxine")]
        private static extern void xine_set_param (IntPtr stream, int param, int value);
        
        [DllImport ("libxine")]
        private static extern int xine_get_param (IntPtr stream, int param);

        [DllImport ("libxine")]
        private static extern uint xine_get_stream_info (IntPtr stream, int info);
        
        [DllImport ("libxine")]
        private static extern int xine_get_pos_length (IntPtr stream, ref int pos_stream, ref int pos_time, ref int length);

        [DllImport ("libxine")]
        private static extern void xine_dispose (IntPtr stream);

        [DllImport ("libxine")]
        private static extern IntPtr xine_event_new_queue (IntPtr stream);

        [DllImport ("libxine")]
        private static extern void xine_event_dispose_queue (IntPtr eventQueue);

        [DllImport ("libxine")]
        private static extern IntPtr xine_event_get (IntPtr eventQueue);

        [DllImport ("libxine")]
        private static extern void xine_event_free (IntPtr eventArgs);

        [DllImport ("libxine")]
        private static extern void xine_stop (IntPtr stream);
    }
}

