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
					"Patrick van Staveren <trick@vanstaveren.us>",
					"David Schneider <david.schneider@picle.org>"
				};
			}
		}
		
		// --------------------------------------------------------------- //
		
		public WikipediaPlugin() {
			Console.WriteLine("Initializing {0}",this.GetType());
		}
		protected override void PluginInitialize()
		{
			InstallInterfaceActions ();			
			PlayerEngineCore.EventChanged += OnPlayerEngineEventChanged;
			
			if (ValidTrack)
				ShowWikipedia (PlayerEngineCore.CurrentTrack);
		}
		
		protected override void PluginDispose()
		{
			Globals.ActionManager.UI.RemoveUi(ui_manager_id);
			Globals.ActionManager.UI.RemoveActionGroup(actions);
			
			PlayerEngineCore.EventChanged -= OnPlayerEngineEventChanged;
			
			if (PaneVisible) {
				// nope, we actually need to kill the widgets here!
				//HideWikipedia ();
				InterfaceElements.MainContainer.Remove(wiki_controller.Pane);
				//InterfaceElements.MainContainer.Remove(separator);
				wiki_controller.Destroy();
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
					ShowWikipedia (PlayerEngineCore.CurrentTrack);
				
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

		private WikipediaQueryController wiki_controller;
				
		private void OnPlayerEngineEventChanged (object o, PlayerEngineEventArgs args)
		{
			if (!Enabled)
				return; 

			switch (args.Event) {
			case PlayerEngineEvent.StartOfStream:
				if (ValidTrack)
					ShowWikipedia (PlayerEngineCore.CurrentTrack);
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
				if (wiki_controller != null)
					return wiki_controller.Pane.Visible;
				else
					return false;

				
			}
		}

		private void ShowWikipedia (TrackInfo info)
		{
			
			lock (this) {
				if ( InterfaceElements.MainContainer != null ) {
					if ( this.wiki_controller == null ) {
						this.wiki_controller = new WikipediaQueryController();
						InterfaceElements.MainContainer.PackEnd (wiki_controller.Pane, false, false, 0);
						wiki_controller.Pane.ShowAll();
					}
				}
			}
			// Don't do anything if we already are showing wikipedia for the
			// requested artist.
			if ( wiki_controller.Track != null )  
				if (PaneVisible && wiki_controller.Track == info)	return;
			
			// If we manually switch track we don't get an EndOfStream event and 
			// must clear the wikipedia pane here.
			if (PaneVisible)
				HideWikipedia ();
			wiki_controller.Pane.Visible = true;
			wiki_controller.Track = info;
			
			
		}
		
		private void HideWikipedia ()
		{
			wiki_controller.Pane.Visible = false;
		}
		
	}
}