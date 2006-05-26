
using System;
using System.IO;
using System.Text;
using Mono.Unix;
namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaErrorPage  : WikipediaBody
	{
		//private Stream stream;
		public WikipediaErrorPage()		
		{
			this.content = "<h1 id=\"error\">"+Catalog.GetString("An error Occured")+"</h1>";
		}
		public WikipediaErrorPage(string title,string message) {
			this.content = "<h1 id=\"error\">"+title+"</h1>"
			+"<br /><br/>"+
			"<div class=\"e_msg\">"+message+"</div>";
		}
		
		
		public override string Content {
			get {
				return this.content;
			}
		}
		
				
	}
	
}