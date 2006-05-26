
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaAlbumHeader : WikipediaGtkHeader
	{
		
		public WikipediaAlbumHeader(WikipediaPage p ) :base(p)
		{
		}
		public WikipediaAlbumHeader()		
		{
			this.content += "<h1 id=\"title\"><em>Album Information</em></h1>";
		}
	}
	
}
