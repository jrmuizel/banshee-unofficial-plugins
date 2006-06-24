
using System;
using System.IO;
using System.Text;

namespace Banshee.Plugins.Wikipedia
{
	
	public sealed class CommonPageFooter : PageFooter
	{
		
		/*public CommonPageFooter(PageHeader h) : base(h)
		{
			this.content =  new MemoryStream(Encoding.UTF8.GetBytes("</body></html>"));
		}*/
		public CommonPageFooter()
		{
			this.content = "</body></html>";
		}
	}
	
}
