
using System;
using Banshee.Base;

namespace Banshee.Plugins.Wikipedia
{
	public class WikipediaAlbumQuery : WikiQuery
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
		public WikipediaAlbumQuery(TrackInfo track)		
		{
			this.track = track;
			this.Init();
		}
		public WikipediaAlbumQuery()
		{
			this.Init();
		}
		protected override string BuildQueryURL() {
			return string.Format(this.query_url,System.Web.HttpUtility.UrlEncode(track.Album),System.Web.HttpUtility.UrlEncode(track.Artist));
		}
		protected override void Init() {
			this.query_url = "http://www.google.com/search?"
			+ "&q=%22{0}%22+%22{1}%22+"
			+ "site%3Aen.wikipedia.org"
			+ "&btnI=asdf";
		}
	}
	
}
