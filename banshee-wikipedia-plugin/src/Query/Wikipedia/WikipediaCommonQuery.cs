
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaCommonQuery : WikiQuery
	{
		private string query;
		public WikipediaCommonQuery(string query)
		{
			this.query = query;
			this.Init();
		}
		public WikipediaCommonQuery()
		{
			this.Init();
		}
		protected override string BuildQueryURL() {
			return string.Format(this.query_url,System.Web.HttpUtility.UrlEncode(this.query));
		}
		protected override void Init() {
			this.query_url = "http://www.google.com/search?hl=en"+
			                     + "&btnG=Google+Search"
			                     + "&as_epq={0}" // maybe add genre here?
			                     + "&as_sitesearch=en.wikipedia.org"
			                     + "&btnI=asdf";
		}
	}
	
}
