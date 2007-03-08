
using System;
using System.IO;
namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class Parser
	{
		protected Stream req;
		public Parser(Stream s) 
		{
			this.req = s;
		}
		public abstract Page GetPage();
		protected abstract void Parse();
	}
	
}
