
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaGenreHeader : PageGtkHeader
	{
		
		public WikipediaGenreHeader() :base()
		{
			this.content += "<h1 id=\"title\"><em>Genre Information</em></h1>";
		}
	}
	
}
