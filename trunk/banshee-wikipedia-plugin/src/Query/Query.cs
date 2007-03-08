
using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class Query
	{
		protected string query_url;
		protected WebRequestHandler wrh;
		protected abstract void Init();
		protected abstract string BuildQueryURL();
		public  abstract bool Find();
		public abstract Page GetResult();
		protected Query() {
			this.wrh = WebRequestHandler.Instance;
		}
		
	}
	
}
