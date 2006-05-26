
using System;
using Banshee.Base;
namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaGenreQuery : WikiQuery
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
		
		public WikipediaGenreQuery() {
			this.Init();
		}
		public WikipediaGenreQuery(TrackInfo track)
		{
			this.track = track;
			this.Init(); 
		}
		protected override string BuildQueryURL() {
			return string.Format(this.query_url,System.Web.HttpUtility.UrlEncode(track.Genre));
		}
		protected override void Init() {
			this.query_url = "http://www.google.com/search?"
			+ "&q=%22{0}%22+music+"
			+ "site%3Aen.wikipedia.org"
			+ "&btnI=asdf";
		}
		
	}
	
}
