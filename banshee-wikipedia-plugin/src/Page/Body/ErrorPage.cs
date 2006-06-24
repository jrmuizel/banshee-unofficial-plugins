
using System;
using System.IO;
using System.Text;
using Mono.Unix;
namespace Banshee.Plugins.Wikipedia
{
	
	public class ErrorPage  : PageBody
	{
		//private Stream stream;
		public ErrorPage()		
		{
			this.content = "<h1 id=\"error\">"+Catalog.GetString("An error Occured")+"</h1>";
			this.Url = "file:///";
		}
		public ErrorPage(string title,string message) {
			this.content = "<h1 id=\"error\">"+title+"</h1>"
			+"<br /><br/>"+
			"<div class=\"e_msg\">"+message+"</div>";
			this.Url = "file:///";
		}
		
		
		public override string Content {
			get {
				return this.content;
			}
		}
		
				
	}
	
}