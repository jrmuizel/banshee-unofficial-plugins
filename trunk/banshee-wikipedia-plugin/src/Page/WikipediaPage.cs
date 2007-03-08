
using System;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	public abstract class Page {
		protected string content;
		protected string url;
		public abstract string Url {
			get;
			set;
		}
		public abstract string Content {
			get;
		}
	}
	public abstract class WikipediaPage : Page
	{
		public WikipediaPage()
		{
		}
	}
	
}
