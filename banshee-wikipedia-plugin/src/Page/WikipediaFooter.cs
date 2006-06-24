
using System;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class WikipediaFooter  : WikipediaDecorator
	{	
		public WikipediaFooter(WikipediaHeader h)
		{
			this.page = h;			
		}
		public WikipediaFooter()
		{			
		}
		
		public override string Content {
			get {
				return page.Content+this.content;
			}
		}
	}
}
