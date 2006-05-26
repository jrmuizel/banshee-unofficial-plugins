
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace Banshee.Plugins.Wikipedia
{
	
	public class RegexWikipediaParser : WikipediaParser
	{
		private string body;
		public RegexWikipediaParser(Stream s) : base(s)
		{
		}
		public override WikipediaPage GetPage() {
			this.Parse();
			return new WikipediaBody(body);
		}
		
		protected override void Parse()
		{
			this.body = new StreamReader(req,Encoding.UTF8).ReadToEnd();
			body = Regex.Replace(body,"^(.|[\n])*(?=<h1)","");
			body = Regex.Replace(body,"<div class=\"printfooter\">(.|[\n])*$","");
			 
		}
	}
	
}
