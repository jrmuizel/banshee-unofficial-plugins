
using System;
using System.IO;
using System.Text;
using Mono.Unix;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class WikipediaParser
	{
		protected Stream req;
		//private MemoryStream body;
		
		public WikipediaParser(Stream s)
		{
			this.req = s;
		}
		
		public abstract WikipediaPage GetPage();
		protected abstract void Parse();		
	}
	
}
