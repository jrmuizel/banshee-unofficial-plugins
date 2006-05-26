
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaWaitBody : WikipediaBody
	{
		
		public WikipediaWaitBody(string message)
		{
			this.content += "<h1>"+message+"</h1>";
		}
	}
	
}
