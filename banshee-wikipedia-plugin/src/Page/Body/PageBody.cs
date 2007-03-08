
using System;
using System.IO;
namespace Banshee.Plugins.Wikipedia
{
	
	public class PageBody  : Page
	{
		public PageBody(string s)		
		{
			this.content = s;
			//Console.WriteLine("Body: {0}",this.content);
		}
		public PageBody()		
		{
			
		}
		
		public override string Content {
			get {
				//return "Help";
				return this.content;
			}
		}
		public override string Url {
			get {
				return this.url;
			}
			set {
				this.url = value;
			}
		}
		
				
	}
	
}
