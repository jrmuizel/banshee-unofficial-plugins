
using System;

namespace  Banshee.Plugins.Wikipedia
{
	
	public class LyricsHeader : PageGtkHeader
	{
		
		public LyricsHeader(Page p) : base(p)
		{
			this.content += "<h1 id=\"title\"><em>Song Lyrics</em></h1>";
		}
		public LyricsHeader() : base()
		{
			this.content += "<h1 id=\"title\"><em>Song Lyrics</em></h1>";
		}
	}
	
}
