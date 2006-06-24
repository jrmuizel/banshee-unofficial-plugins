
using System;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class PageHeader  : PageDecorator
	{
		public PageHeader(Page p)		
		{
			this.page = p;
		}
		public PageHeader() :base()		
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
		public override string Url {
			get {
				return this.page.Url;
			}
			set {
				this.page.Url = value;
			}
		}
	}
	
}
