
using System;
using Banshee.Base;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	public sealed class LyricsQuery : Query
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
		
		public LyricsQuery()
		{
			this.Init();
		}
		public LyricsQuery(TrackInfo track)
		{
			this.track = track;
			this.Init();
		}
		
		protected override void Init()
		{
			this.query_url = "http://www.autolyrics.com/tema1en.php?artist={0}&songname={1}";
		}
		
		protected override string BuildQueryURL()
		{
			string artist = Track.Artist.Trim().Replace(" ","+");
			string song   = Track.Title.Trim().Replace(" ","+");
			return string.Format(this.query_url,artist,song);
		}
		
		public  override bool Find() {
			string url = this.BuildQueryURL();
			wrh.LookUp(url);
			if ( wrh.ResponseUri == null ) {
				wrh.Close();
				return false;
			} else {
				return true;
			}
		}
		
		public override Page GetResult() {
			Page p = null;
			Stream s;
			s = wrh.ResponseStream;
			p = new RegexLyricsParser(s).GetPage();
			p.Url = wrh.ResponseUri.ToString();
			s.Close();
			wrh.Close();
			return p;
		}
		
	}
	
}
