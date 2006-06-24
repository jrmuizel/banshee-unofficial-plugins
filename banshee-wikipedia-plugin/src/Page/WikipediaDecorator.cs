
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class WikipediaDecorator : WikipediaPage
	{
		protected WikipediaPage page;
		public WikipediaPage Component {
			set {
				this.page = value;
				//Console.WriteLine("Set_Page {0}",this.page.GetType());
			}
		}
		
		
		public WikipediaDecorator()
		{
		}
	}
	
}
