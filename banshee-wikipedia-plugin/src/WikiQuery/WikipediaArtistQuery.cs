
using System;
using Banshee.Base;

namespace Banshee.Plugins.Wikipedia
{
	
	public sealed class WikipediaArtistQuery : WikiQuery
	{
		private TrackInfo track;
		public TrackInfo Track {
			get {
				return this.track;
			}
			set {
				this.track = value;
			}
		}
		public WikipediaArtistQuery()
		{
			this.Init();
		}
		public WikipediaArtistQuery(TrackInfo track)
		{
			this.track = track;
			this.Init(); 
		}
		protected override string BuildQueryURL() {
			return string.Format(this.query_url,System.Web.HttpUtility.UrlEncode(track.Artist),System.Web.HttpUtility.UrlEncode(track.Genre));
		}
		protected override void Init() {
			this.query_url = "http://www.google.com/search?hl=en"+
			                     + "&btnG=Google+Search"
			                     + "&as_epq={0}&as_oq=music+band+group+artist+musician+singer+{1}" // maybe add genre here?
			                     + "&as_sitesearch=en.wikipedia.org"
			                     + "&btnI=asdf";
		}
		
		
	}
	
}
