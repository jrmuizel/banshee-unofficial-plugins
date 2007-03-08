
using System;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class PageFooter  : PageDecorator
	{	
		public PageFooter(PageHeader h)
		{
			this.page = h;			
		}
		public PageFooter()
		{			
		}
		
		public override string Content {
			get {
				return page.Content+this.content;
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
