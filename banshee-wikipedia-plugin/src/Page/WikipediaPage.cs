
using System;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class WikipediaPage
	{
		protected string content;
		public WikipediaPage()
		{
		}
		/*public WikipediaPage Decorate() {
			return new WikipediaHeader(new WikipediaFooter(this));
		}*/
		
		public abstract string Content {
			get;
		}
		
	}
	
}
