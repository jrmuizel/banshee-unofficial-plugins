
using System;
using System.IO;
namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaBody  : WikipediaPage
	{
		public WikipediaBody(string s)		
		{
			this.content = s;
			//Console.WriteLine("Body: {0}",this.content);
		}
		public WikipediaBody()		
		{
			
		}
		
		public override string Content {
			get {
				//return "Help";
				return this.content;
			}
		}
		
				
	}
	
}
