
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaSongHeader  : WikipediaGtkHeader
	{
		
		public WikipediaSongHeader(WikipediaPage p) : base(p)
		{
		}
		public WikipediaSongHeader() : base()
		{
			this.content += "<h1 id=\"title\"><em>Song Information</em></h1>";
		}
	}
	
}
