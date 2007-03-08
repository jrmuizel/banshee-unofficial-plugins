
using System;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Text;
using Mono.Unix;
namespace Banshee.Plugins.Wikipedia
{
	
	public class XPathWikipediaParser : Parser
	{
		private MemoryStream body;
		public XPathWikipediaParser(Stream s ) : base(s)
		{
		}
		
		public override Page GetPage(){
			this.Parse();
			return new PageBody(new StreamReader(this.body).ReadToEnd() );
		}
		
		protected override void Parse()
		{
			Console.WriteLine("Start Parsing");
			try {
			XmlDocument html = new XmlDocument();
			StreamReader sr = new StreamReader(req, Encoding.UTF8);
			sr.ReadLine();
			sr.ReadLine();				
			StringBuilder sb = new StringBuilder();
			sb.Insert(0,"<html>");			
			sb.Append(sr.ReadToEnd());
			sr.Close();
			html.LoadXml(sb.ToString());
			XPathNavigator nav = html.CreateNavigator();
			Console.WriteLine("Created Navigator");
			
				XPathNodeIterator iter	= nav.Select("//div[@id='bodyContent']");
				iter.MoveNext();
				XmlNode node  			= ((IHasXmlNode)iter.Current ).GetNode();
				XmlNode r = node.SelectSingleNode("//h3[@id='siteSub']");
				if ( r != null ) {
					r.ParentNode.RemoveChild(r);
					Console.WriteLine("Removed SiteSub {0}",r.InnerText);
				}
				r = node.SelectSingleNode("//div[@id='contentSub']");
				if ( r != null ) {
					node.RemoveChild(r);
					Console.WriteLine("Removed ContentSub {0}",r.InnerText);
				}
				
				r = node.SelectSingleNode("//div[@id='jump-to-nav']");
				if ( r != null ) {
					node.RemoveChild(r);
					Console.WriteLine("Removed jump-to-nav {0}",r.InnerText);
				}
				
				r = node.SelectSingleNode("//div[@class='printfooter']");
				if ( r != null ) {
					node.RemoveChild(r);
					Console.WriteLine("Removed printfooter {0}",r.InnerText);
				}
				
				if ( node != null ) {					
					this.body = new MemoryStream(Encoding.UTF8.GetBytes(node.InnerXml));
				}
			} catch ( Exception e ) {
				Console.WriteLine("Error retrieving body "+e.Message);
				Console.WriteLine(e.StackTrace);
				this.body = new MemoryStream(Encoding.UTF8.GetBytes(Catalog.GetString("An error ocurred while retrieving the artist information from wikipedia"))); 
			}
			Console.WriteLine("Parsing Done");
		}
	}
	
}
