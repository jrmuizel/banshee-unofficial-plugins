using System;
using System.Collections;

using Banshee.Base;

namespace Banshee.Plugins.MusicStore 
{
	public class MusicStoreTrackInfo : TrackInfo 
	{
		private Hashtable data;
		
		public Hashtable Data {
			get { return data; }
		}
		
		public MusicStoreTrackInfo (Hashtable data) 
		{
			this.data = data;
			
			if (data.ContainsKey ("artistName"))
				Artist = (string) data ["artistName"];
			
			if (data.ContainsKey ("playlistName"))
				Album = (string) data ["playlistName"]; // FIXME: Correct?
			
			if (data.ContainsKey ("songName"))
				Title = (string) data ["songName"];
			
			if (data.ContainsKey ("genre"))
				Genre = (string) data ["genre"];
			
			if (data.ContainsKey ("year"))
				Year = (int) data ["year"];
	
			if (data.ContainsKey ("duration"))
				Duration = TimeSpan.FromSeconds ((int) data["duration"]/1000);
		}

		public string DisplayPrice {
			get { 
				if (data.ContainsKey ("priceDisplay"))
					return (string) data ["priceDisplay"];
				return "";
			}
		}
	}
}
