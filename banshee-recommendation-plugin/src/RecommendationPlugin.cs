using System;

using Gtk;
using Mono.Unix;

using Banshee.Base;
using Banshee.Sources;
using Banshee.MediaEngine;

namespace Banshee.Plugins.Recommendation
{
	public class RecommendationPlugin : Banshee.Plugins.Plugin
	{
		protected override string ConfigurationName { get { return "Recommendation"; } }
		public override string DisplayName { get { return Catalog.GetString ("Music Recommendations"); } }
		
		public override string Description {
			get {
				return Catalog.GetString("Automatically recommends music that you might like. The recommendations " + 
							 "are based on the what song you are listening to at the moment, and finds " + 
							 "artists and popular songs that others, that share your taste in music, prefers.");
			}
		}
		
		public override string [] Authors {
			get {
				return new string [] {
					"Fredrik Hedberg"
				};
			}
		}
		
		// --------------------------------------------------------------- //
		
		protected override void PluginInitialize()
		{
			InstallInterfaceActions ();
			
			PlayerEngineCore.EventChanged += OnPlayerEngineEventChanged;
			
			if (recommendation_pane != null &&
			    PlayerEngineCore.CurrentTrack != null &&
			    PlayerEngineCore.CurrentTrack.Artist != null && 
			    PlayerEngineCore.CurrentTrack.Artist != "")
				ShowRecommendations (PlayerEngineCore.CurrentTrack.Artist);
		}
		
		protected override void PluginDispose()
		{
			Globals.ActionManager.UI.RemoveUi(ui_manager_id);
			Globals.ActionManager.UI.RemoveActionGroup(actions);
			
			PlayerEngineCore.EventChanged -= OnPlayerEngineEventChanged;
			
			if (PaneVisible)
				HideRecommendations ();
		}
		
		// --------------------------------------------------------------- //
		
		private ActionGroup actions;
		private uint ui_manager_id;
		
		private void InstallInterfaceActions()
		{
			actions = new ActionGroup("Recommendation");
			
			actions.Add(new ToggleActionEntry [] {
					new ToggleActionEntry("ShowRecommendationAction", null,
							      Catalog.GetString("Show Recommendations"), "<control>R",
							      Catalog.GetString("Show Recommendations"), OnToggleShow, true)
				});
			
			Globals.ActionManager.UI.InsertActionGroup(actions, 0);
			ui_manager_id = Globals.ActionManager.UI.AddUiFromResource("RecommendationMenu.xml");
		}
		
		private void OnToggleShow (object o, EventArgs args) 
		{
			Enabled = (o as ToggleAction).Active;
		}
		
		private bool enabled = true;
		public bool Enabled {
			get { return enabled; }
			set { 
				if (enabled && !value && PaneVisible) {
					HideRecommendations ();
				} else if (!enabled && value && 
					   PlayerEngineCore.CurrentTrack != null &&
					   PlayerEngineCore.CurrentTrack.Artist != null && 
					   PlayerEngineCore.CurrentTrack.Artist != "") {
					ShowRecommendations (PlayerEngineCore.CurrentTrack.Artist);
				} 

				enabled = value;
			}
		}

		// --------------------------------------------------------------- //

		private RecommendationPane recommendation_pane;

		private void OnPlayerEngineEventChanged (object o, PlayerEngineEventArgs args)
		{
			if (!Enabled)
				return; 

			switch (args.Event) {
			case PlayerEngineEvent.StartOfStream:
				if (PlayerEngineCore.CurrentTrack != null && 
				    PlayerEngineCore.CurrentTrack.Artist != null && 
				    PlayerEngineCore.CurrentTrack.Artist != "")
					ShowRecommendations (PlayerEngineCore.CurrentTrack.Artist);
				break;
				
			case PlayerEngineEvent.EndOfStream:
				if (PaneVisible)
					HideRecommendations ();
				break;
			}
		}

		private bool PaneVisible
		{
			get {
				if (recommendation_pane == null)
					return false;

				return recommendation_pane.Visible;
			}
		}

		private void ShowRecommendations (string artist)
		{
			lock (this) {
				if (recommendation_pane == null) {
					recommendation_pane = new RecommendationPane ();
					InterfaceElements.MainContainer.PackEnd (recommendation_pane, false, false, 0);
				}
				
				// If we manually switch track we don't get an EndOfStream event and 
				// must clear the recommendation pane here.
				if (PaneVisible)
					HideRecommendations ();
				
				recommendation_pane.ShowRecommendations (artist);
			}
		}
		
		private void HideRecommendations ()
		{
			recommendation_pane.HideRecommendations ();
		}
	}
}