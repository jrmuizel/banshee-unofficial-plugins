
using System;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	
	public abstract class WikiQuery
	{
		protected string query_url;
		protected WebRequestHandler wrh;
		protected abstract void Init();
		protected abstract string BuildQueryURL();
		public bool Find() {
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
		public WikipediaPage GetResult() {
			WikipediaPage p = null;
			Stream s;
			//try {
				s = wrh.ResponseStream;
				p = new RegexWikipediaParser(s).GetPage();
				s.Close();
				
			//} catch (Exception e) {
			//	Console.WriteLine(e.Message);
			//} finally {
				wrh.Close();
			//}
			
			Console.WriteLine("returning Page after parsing");
			return p;
		}
		
		
		protected WikiQuery() {
			this.wrh = WebRequestHandler.Instance;
		}
		
		
		
		
	}
	
}
