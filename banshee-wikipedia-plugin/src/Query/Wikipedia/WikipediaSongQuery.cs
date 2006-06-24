
using System;
using Banshee.Base;

namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaSongQuery : WikiQuery
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
		public WikipediaSongQuery()
		{
		}
		public WikipediaSongQuery(TrackInfo track)
		{
			this.track = track;
		}
		protected override string BuildQueryURL() {
			return string.Format(this.query_url,System.Web.HttpUtility.UrlEncode(track.Title));
		}
		protected override void Init() {
			this.query_url = "http://www.google.com/search?hl=en"+
			                     + "&btnG=Google+Search"
			                     + "&as_epq={0}&as_oq=music+song" // maybe add genre here?
			                     + "&as_sitesearch=en.wikipedia.org"
			                     + "&btnI=asdf";
		}
	}
	
}
