/* -*- Mode:csharp; tab-width: 4; indent-tabs-mode: nil; c-basic-offset: 4 -*- */
/***************************************************************************
 *  XinePlayerEngine.cs
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
using System.Collections;

using Banshee.Base;
using Banshee.MediaEngine;

using Xine;

namespace Banshee.MediaEngine.Xine
{
    public class XinePlayerEngine : PlayerEngine, IEqualizer
    {
        private XineEngine _xine;
        private Stream _stream;
        private uint _timeoutId;
        
        public XinePlayerEngine ()
        {
            _xine = new XineEngine ();
            _stream = _xine.CreateStream ();
            Stream.EndOfStream += new EventHandler (OnEndOfStream);
        }

        private void StartTimer ()
        {
            if (_timeoutId > 0) {
                return;
            }

            _timeoutId = GLib.Timeout.Add (500, delegate {
                _stream.ProcessEventQueue ();
                            
                if (CurrentState == PlayerEngineState.Playing) {
                    OnEventChanged(PlayerEngineEvent.Iterate);
                }

                return true;
            });     
        }

        private void StopTimer ()
        {
            if (_timeoutId > 0) {
                GLib.Source.Remove(_timeoutId);
                _timeoutId = 0;
            }
        }
        
        private void OnEndOfStream (object sender, EventArgs args)
        {
            OnEventChanged (PlayerEngineEvent.EndOfStream);
        }
        
        protected override void OpenUri (SafeUri uri)
        {
            _stream.Open (uri.AbsoluteUri);
        }

        public override void Close ()
        {
            StopTimer ();
            _stream.Close ();
            OnStateChanged (PlayerEngineState.Idle);
        }       
        
        public override void Play ()
        {
            _stream.Play ();            
            StartTimer ();
            OnStateChanged (PlayerEngineState.Playing);
        }

        public override void Pause ()
        {
            StopTimer ();
            _stream.Pause ();
            OnStateChanged (PlayerEngineState.Paused);
        }

        public void SetEqualizerGain (uint frequency, int value)
        {
            if (value == 0) {
                value = 1;
            }
            if (value < -100 || value > 100) {
                throw new ArgumentOutOfRangeException ("value must be in range -100..100");
            }

            // Normalize the passed frequency in the context of the available frequencies
            //
            uint normalizedFrequency = 0;
            int foundIndex = -1;
            foundIndex = Array.BinarySearch (EqualizerFrequencies, frequency);
            
            if (foundIndex >= 0) {
                normalizedFrequency = EqualizerFrequencies[foundIndex];
            }
            else if (frequency < EqualizerFrequencies[0]) {
                normalizedFrequency = EqualizerFrequencies[0];
            }
            else if (frequency > EqualizerFrequencies[EqualizerFrequencies.Length - 1]) {
                normalizedFrequency = EqualizerFrequencies[EqualizerFrequencies.Length - 1];
            }
            else {                
                for (int i = 0; i + 1 < EqualizerFrequencies.Length; i++) {
                    if (frequency <= EqualizerFrequencies[i] && frequency <= EqualizerFrequencies[i+1]) {
                        normalizedFrequency = EqualizerFrequencies[i];
                    }
                }
            }

            _stream.SetEqualizerGain (normalizedFrequency, value);
        }
        
        public override ushort Volume {
            get {
                return (ushort) _stream.Volume;
            }
            set {
                _stream.Volume = (uint) value;
                OnEventChanged (PlayerEngineEvent.Volume);
            }
        }
        
        public override bool CanSeek {
            get { return _stream.CanSeek; }
        }
        
        public override uint Position {
            get {
                return _stream.Position;
            }
            set {
                _stream.Position = value;
                OnEventChanged (PlayerEngineEvent.Seek);
            }
        }
        
        public override uint Length {
            get {
                return _stream.Length;
            }
        }

        public uint[] EqualizerFrequencies
        {
            get {
                uint[] freqs = _stream.EqualizerFrequencies;
                Array.Sort (freqs);
                return freqs;
            }
        }

        // Xine Default: 100
        // Xine Range: 0 - 200
        // Plugin Default: 0
        // Plugin Range: -100 +100 
        // Plugin = Xine - 100
        //
        public int AmplifierLevel
        {
            get { return (int) _stream.AmplifierLevel - 100; }
            set { _stream.AmplifierLevel = (uint) value + 100; }
        }

        private static string [] source_capabilities = { "file", "http", "cdda" };
        public override IEnumerable SourceCapabilities {
            get { return source_capabilities; }
        }
                
        private static string [] decoder_capabilities = { "mp3", "ogg", "wma", "asf", "flac" };
        public override IEnumerable ExplicitDecoderCapabilities {
            get { return decoder_capabilities; }
        }
        
        public override string Id {
            get { return "xine-engine"; }
        }
        
        public override string Name {
            get { return "Xine"; }
        }       

        public override void Dispose ()
        {
            StopTimer();        
            
            _stream.Dispose ();
            _xine.Dispose ();
            
            base.Dispose ();
            GC.SuppressFinalize(this);
        }

        ~XinePlayerEngine ()
        {
            Dispose ();
        }
    }
}
