
using System;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	
	public abstract class WikiQuery : Query
	{
		
		public override bool Find() {
			string url = this.BuildQueryURL();
			wrh.LookUp(url);
			if ( wrh.ResponseUri == null ) Console.WriteLine("ResponseUri is null");
			if ( !wrh.ResponseUri.Host.Equals("en.wikipedia.org")) {
				wrh.Close();
				return false;
			} else {
				return true;
			}
			
		}
		public override Page GetResult() {
			Page p = null;
			Stream s;
			//try {
				s = wrh.ResponseStream;
				p = new RegexWikipediaParser(s).GetPage();
				p.Url = wrh.ResponseUri.ToString();
				s.Close();
				
			//} catch (Exception e) {
			//	Console.WriteLine(e.Message);
			//} finally {
				wrh.Close();
			//}
			
			Console.WriteLine("returning Page after parsing");
			return p;
		}
		
		
		protected WikiQuery() :base() {
			
		}
		
		
		
		
	}
	
}
