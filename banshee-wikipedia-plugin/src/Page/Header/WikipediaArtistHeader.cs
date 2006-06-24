
using System;
using System.IO;
using System.Text;

namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaArtistHeader : PageGtkHeader
	{
		public WikipediaArtistHeader(WikipediaPage p) : base (p)		
		{
			
		}
		public WikipediaArtistHeader() :base()
		{			
			this.content += "<h1 id=\"title\"><em>Artist Information</em></h1>";
		}
	}
	
}
