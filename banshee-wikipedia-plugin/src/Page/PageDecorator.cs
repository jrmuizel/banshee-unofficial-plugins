
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class PageDecorator : WikipediaPage
	{
		protected Page page;
		public Page Component {
			set {
				this.page = value;
				//Console.WriteLine("Set_Page {0}",this.page.GetType());
			}
		}
		
		
		public PageDecorator()
		{
		}
	}
	
}
