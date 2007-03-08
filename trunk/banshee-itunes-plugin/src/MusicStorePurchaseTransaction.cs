using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Diagnostics;

using Gtk;
using Mono.Unix;

using Banshee.Base;
using Banshee.Widgets;

namespace Banshee.Plugins.MusicStore 
{
	public class MusicStorePurchaseTransaction 
	{
		private MusicStorePlugin plugin;
		private MusicStoreTrackInfo track;
		private ActiveUserEvent user_event;
		
		public MusicStorePurchaseTransaction (MusicStorePlugin plugin, MusicStoreTrackInfo track)
		{
			this.plugin = plugin;
			this.track = track;
		}
		
		public void Run()
		{
			Thread thread = new Thread(new ThreadStart(ThreadWorker));
			thread.Start();
		}
		
		// ----------------------------------------------- //
		
		// FIXME: Ok, this is a real hack, it has half-support for buying multiple songs
		// but doenst really allow it yet. Will overwrite the song with the last one downloaded.
		protected virtual void ThreadWorker()
		{
			user_event = new ActiveUserEvent (String.Format (Catalog.GetString ("Purchasing {0} - {1}"), track.Artist, track.Title));
			
			user_event.CanCancel = false;
			user_event.Icon = IconThemeUtils.LoadIcon("document-save", 22); // FIXME: Get a nice icon
			
			user_event.Header = Catalog.GetString ("Purchasing");
			user_event.Message = String.Format ("{0} - {1}", track.Artist, track.Title);

			ArrayList songs = new ArrayList ();
			try {
				songs = plugin.Store.Buy (track.Data);
			} catch (Exception ex) {
				// FIXME: Error handling correct?
				user_event.Dispose ();
				HigMessageDialog.RunHigMessageDialog (null,
								      0,
								      MessageType.Error,
								      ButtonsType.Ok,
								      "Purchase Error",
								      String.Format ("There was an error while performing the purchase: {0}", ex.Message)); 				
				return;
			}

			Hashtable song, meta;

			for (int i = 0; i < songs.Count; i++) {
				song = (Hashtable) songs[i];
				meta = song[ "metaData" ] != null ? (Hashtable)song ["metaData"] : song;
				
				user_event.Header = String.Format (Catalog.GetString ("Downloading {0} of {1}"), i+1, songs.Count);
				user_event.Message = String.Format (Catalog.GetString ("{0} - {1}"), track.Artist, track.Title);

				try {
					byte [] buffer = plugin.Store.DownloadSong (song, new FairStore.Progress (ProgressUpdate));
					
					FileStream fs = new FileStream (MusicStorePlugin.GetLibraryPathForTrack (track), FileMode.CreateNew);
	                    		BinaryWriter bw = new BinaryWriter (fs);
                    			bw.Write (buffer);
                    			bw.Close ();
                    			fs.Close ();
				} catch (Exception ex) {
					// FIXME: Error handling correct?
					user_event.Dispose ();
					HigMessageDialog.RunHigMessageDialog (null,
									      0,
									      MessageType.Error,
									      ButtonsType.Ok,
									      "Download Error",
									      String.Format ("There was an error while performing the download: {0}", ex.Message)); 
					return;
				}
			}
			user_event.Dispose ();
		}

		// ----------------------------------------------- //
		
		private void ProgressUpdate (int bytes_position, int bytes_total)
    		{
			user_event.Progress = (double)bytes_position/(double)bytes_total;
		}
	}
}
