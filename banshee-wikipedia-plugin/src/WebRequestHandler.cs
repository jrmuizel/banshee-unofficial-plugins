
using System;
using System.Net;
using System.IO;

namespace Banshee.Plugins.Wikipedia
{
	
	public sealed class WebRequestHandler
	{
		private static readonly  WebRequestHandler instance = new WebRequestHandler();
		private HttpWebResponse response; 
		
		public static WebRequestHandler Instance
		{
			get {
				return instance;
			}
		}
		static WebRequestHandler()
		{
		}
		WebRequestHandler()
		{
		}
		
		public void LookUp(string url) {
			Console.WriteLine("Requesting {0}",url);
			HttpWebRequest hwr 	= this.GetRequest(url);
			this.response		= (HttpWebResponse) hwr.GetResponse();
			Console.WriteLine("Answer from {0}",this.ResponseUri.Host);
		}
		
		public Uri ResponseUri {
			get {
				if ( this.response != null )
					return this.response.ResponseUri;
				else
					return null;
			}
		}
		
		public Stream ResponseStream {
			get {
				if ( this.response != null )					
					return this.response.GetResponseStream();
				else
					return null;
			}
		}
		
		public void Close() {
			if (this.response != null) {
				this.response.Close();
				this.response = null;
			}
		}
		
		
		private HttpWebRequest GetRequest(string url) {
			HttpWebRequest request 		= (HttpWebRequest)WebRequest.Create(url);
			request.KeepAlive 		= false;
			request.AllowAutoRedirect 	= true;
			request.UserAgent 		= "Mozilla (Banshee-wikipedia plugin)";
			return request;
			
		}
	}
	
}
