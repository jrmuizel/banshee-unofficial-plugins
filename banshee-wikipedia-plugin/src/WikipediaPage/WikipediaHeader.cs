
using System;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaHeader  : WikipediaDecorator
	{
		public WikipediaHeader(WikipediaPage p)		
		{
			this.page = p;
		}
		public WikipediaHeader() :base()		
		{
		}
		
		public override string Content {
			get {
				/*//Console.WriteLine("Me {0}",this.content);
				if ( this.page != null ) 
					//Console.WriteLine("Page {0}",this.page.Content);
				else
					//Console.WriteLine("Page is null");
				*/
				return this.content+this.page.Content;
			}
		}
	}
	
}
