
using System;
using System.IO;
using System.Text;

namespace Banshee.Plugins.Wikipedia
{
	
	public class CommonWikipediaFooter : WikipediaFooter
	{
		
		/*public CommonWikipediaFooter(WikipediaHeader h) : base(h)
		{
			this.content =  new MemoryStream(Encoding.UTF8.GetBytes("</body></html>"));
		}*/
		public CommonWikipediaFooter()
		{
			this.content = "</body></html>";
		}
	}
	
}
