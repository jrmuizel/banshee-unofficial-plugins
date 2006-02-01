using System;
using System.Collections;

using Gtk;
using Mono.Unix;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Widgets;

namespace Banshee.Plugins.MusicStore
{
	public class MusicStoreSource : Banshee.Sources.Source
	{
		private MusicStorePlugin plugin;
		private Widget widget;
		
		public MusicStoreSource (MusicStorePlugin plugin) : base("Music Store", 150)
		{
			this.plugin = plugin;
		}
		
		public override Widget ViewWidget {
			get {
				if (widget == null)
					widget = BuildWidget ();	
				return widget;
			}
		}
		
		public override bool HandlesSearch {
			get {
				return true;
			}
		}
		
		public override void Activate () 
		{
			InterfaceElements.SearchEntry.EnterPress += OnSearchEntryActivated;
		}
		
		public override void Deactivate () 
		{
			InterfaceElements.SearchEntry.EnterPress -= OnSearchEntryActivated;
		}
		
		// ---------------------------------------------- //
		
		private Widget BuildWidget ()
		{
			ScrolledWindow sw = new ScrolledWindow ();
			sw.ShadowType = ShadowType.In;
			sw.Child = plugin.View;
			
			return sw;
		}
		
		private void OnSearchEntryActivated (object o, EventArgs args)
		{
			Console.WriteLine ("Search: {0}", InterfaceElements.SearchEntry.Query);
			plugin.Search (InterfaceElements.SearchEntry.Query);
		}
	}
}

