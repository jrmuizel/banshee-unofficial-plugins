
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public sealed class WaitBody : PageBody
	{
		
		public WaitBody(string message)
		{
			this.content += "<h1>"+message+"</h1>";
			this.Url = "file:///";
		}
	}
	
}
