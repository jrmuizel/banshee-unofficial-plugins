using System;

using Gtk;
using Mono.Unix;

using Banshee.Base;
using Banshee.Sources;
using Banshee.MediaEngine;

namespace Banshee.Plugins.Wikipedia
{
	public class WikipediaPlugin : Banshee.Plugins.Plugin
	{
		protected override string ConfigurationName { get { return "Wikipedia"; } }
		public override string DisplayName { get { return Catalog.GetString ("Wikipedia"); } }
		
		public override string Description {
			get {
				return Catalog.GetString("Gets artist info from wikipedia");
			}
		}
		
		public override string [] Authors {
			get {
				return new string [] {
					"Patrick van Staveren <trick@vanstaveren.us>"
				};
			}
		}
		
		// --------------------------------------------------------------- //
		
		protected override void PluginInitialize()
		{
			InstallInterfaceActions ();
			
			PlayerEngineCore.EventChanged += OnPlayerEngineEventChanged;
			
			if (wikipedia_pane != null && ValidTrack)
				ShowWikipedia (PlayerEngineCore.CurrentTrack.Artist);
		}
		
		protected override void PluginDispose()
		{
			Globals.ActionManager.UI.RemoveUi(ui_manager_id);
			Globals.ActionManager.UI.RemoveActionGroup(actions);
			
			PlayerEngineCore.EventChanged -= OnPlayerEngineEventChanged;
			
			if (PaneVisible) {
				// nope, we actually need to kill the widgets here!
				//HideWikipedia ();
				InterfaceElements.MainContainer.Remove(wikipedia_pane);
				//InterfaceElements.MainContainer.Remove(separator);
				wikipedia_pane.Destroy();
				//separator.Destroy();
			}
		}
		
		// --------------------------------------------------------------- //
		
		private ActionGroup actions;
		private uint ui_manager_id;
		
		private void InstallInterfaceActions()
		{
			actions = new ActionGroup("Wikipedia");
			
			actions.Add(new ToggleActionEntry [] {
					new ToggleActionEntry("ShowWikipediaAction", null,
							      Catalog.GetString("Show Wikipedia"), "<control>I",
							      Catalog.GetString("Show Wikipedia"), OnToggleShow, true)
				});
			
			Globals.ActionManager.UI.InsertActionGroup(actions, 0);
			ui_manager_id = Globals.ActionManager.UI.AddUiFromResource("WikipediaMenu.xml");
		}
		
		private void OnToggleShow (object o, EventArgs args) 
		{
			Enabled = (o as ToggleAction).Active;
		}

		// --------------------------------------------------------------- //
		
		private bool enabled = true;
		public bool Enabled {
			get { return enabled; }
			set { 
				if (enabled && !value && PaneVisible)
					HideWikipedia ();
				else if (!enabled && value && ValidTrack)
					ShowWikipedia (PlayerEngineCore.CurrentTrack.Artist);

				enabled = value;
			}
		}

		public bool ValidTrack {
			get {
				return (PlayerEngineCore.CurrentTrack != null &&
					PlayerEngineCore.CurrentTrack.Artist != null && 
					PlayerEngineCore.CurrentTrack.Artist != "");
			}
		}

		// --------------------------------------------------------------- //

		private WikipediaPane wikipedia_pane;
		
		private void OnPlayerEngineEventChanged (object o, PlayerEngineEventArgs args)
		{
			if (!Enabled)
				return; 

			switch (args.Event) {
			case PlayerEngineEvent.StartOfStream:
				if (ValidTrack)
					ShowWikipedia (PlayerEngineCore.CurrentTrack.Artist);
				break;
				
			case PlayerEngineEvent.EndOfStream:
				if (PaneVisible)
					HideWikipedia ();
				break;
			}
		}

		private bool PaneVisible
		{
			get {
				if (wikipedia_pane == null)
					return false;

				return wikipedia_pane.Visible;
			}
		}
		
		//private VPaned separator;

		private void ShowWikipedia (string artist)
		{
			lock (this) {
				if (wikipedia_pane == null) {
					wikipedia_pane = new WikipediaPane ();
					/*separator = new VPaned();
					//Gtk.Box list = InterfaceElements.MainContainer.Children[0]
					//InterfaceElements.MainContainer.Rem
					Gtk.VBox list = new Gtk.VBox();
					
					for(int i = 0; i < InterfaceElements.MainContainer.Children.GetLength(0); i++) {
						Console.WriteLine("Found a widget: {0}", i);
						Gtk.Widget cur = InterfaceElements.MainContainer.Children[i];
						InterfaceElements.MainContainer.Remove(InterfaceElements.MainContainer.Children[i]);
						list.PackStart(cur);
					}
						
						
						
					
					separator.Add1(list);
					separator.Add2(wikipedia_pane);
					//InterfaceElements.MainContainer.Add(separator);
					//InterfaceElements.MainContainer.PackEnd (separator, false, false, 0);*/
					InterfaceElements.MainContainer.PackEnd (wikipedia_pane, false, false, 0);
				}
				
				// Don't do anything if we already are showing wikipedia for the
				// requested artist.
				if (PaneVisible && wikipedia_pane.current_artist == artist)
					return;
				
				// If we manually switch track we don't get an EndOfStream event and 
				// must clear the wikipedia pane here.
				if (PaneVisible)
					HideWikipedia ();
				
				wikipedia_pane.ShowWikipedia (artist);
				//separator.Visible = true;
			}
		}
		
		private void HideWikipedia ()
		{
			wikipedia_pane.HideWikipedia ();
			//separator.Visible = false;
		}
	}
}