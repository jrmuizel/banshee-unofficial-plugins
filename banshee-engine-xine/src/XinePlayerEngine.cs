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

using Xine;
using Banshee.Base;
using Banshee.MediaEngine;

namespace Banshee.MediaEngine.Xine
{
	public class XinePlayerEngine : PlayerEngine
	{
		private XineEngine _xine;
		private Stream _stream;
		private uint timeout_id;
		
		public XinePlayerEngine ()
		{
			_xine = new XineEngine ();
			_stream = _xine.CreateStream ();
			Stream.EndOfStream += new EventHandler (OnEndOfStream);
		}

		private void StartTimer()
		{
			if(timeout_id > 0) {
				return;
			}

			timeout_id = GLib.Timeout.Add(500, delegate {
				_stream.ProcessEventQueue();
							
				if(CurrentState == PlayerEngineState.Playing) {
					OnEventChanged(PlayerEngineEvent.Iterate);
				}

				return true;
			});		
		}

		private void StopTimer()
		{
			if(timeout_id > 0) {
				GLib.Source.Remove(timeout_id);
				timeout_id = 0;
			}
		}
		
		private void OnEndOfStream (object sender, EventArgs args)
		{
			OnEventChanged(PlayerEngineEvent.EndOfStream);
		}
		
		protected override void OpenUri (SafeUri uri)
		{
			_stream.Open(uri.AbsoluteUri);
		}

        public override void Close()
        {
			StopTimer();
			_stream.Close ();
			OnStateChanged (PlayerEngineState.Idle);
        }       
		
		public override void Play()
        {
			_stream.Play ();			
			StartTimer();
			OnStateChanged (PlayerEngineState.Playing);
		}

        public override void Pause()
        {
			StopTimer();
			_stream.Pause ();
			OnStateChanged (PlayerEngineState.Paused);
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
