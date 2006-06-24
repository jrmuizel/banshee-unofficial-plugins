
using System;
using System.IO;
using System.Text;
using Mono.Unix;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class WikipediaParser : Parser
	{
		
		//private MemoryStream body;
		
		public WikipediaParser(Stream s) :base(s)
		{
		}		
	}
	
}
