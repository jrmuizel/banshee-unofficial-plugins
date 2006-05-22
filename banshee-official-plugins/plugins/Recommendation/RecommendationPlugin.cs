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
					"Fredrik Hedberg",
					"Lukas Lipka"
				};
			}
		}
		
		// --------------------------------------------------------------- //
		
		protected override void PluginInitialize()
		{
			InstallInterfaceActions ();
			
			PlayerEngineCore.EventChanged += OnPlayerEngineEventChanged;
			SourceManager.ActiveSourceChanged += OnActiveSourceChanged;
			
			if (recommendation_pane != null && ValidTrack)
				ShowRecommendations (PlayerEngineCore.CurrentTrack.Artist);
		}
		
		protected override void PluginDispose()
		{
			Globals.ActionManager.UI.RemoveUi(ui_manager_id);
			Globals.ActionManager.UI.RemoveActionGroup(actions);
			
			PlayerEngineCore.EventChanged -= OnPlayerEngineEventChanged;
			SourceManager.ActiveSourceChanged -= OnActiveSourceChanged;
			
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

		// --------------------------------------------------------------- //
		
		private bool enabled = true;
		public bool Enabled {
			get { return enabled; }
			set { 
				if (enabled && !value && PaneVisible)
					HideRecommendations ();
				else if (!enabled && value && ValidTrack)
					ShowRecommendations (PlayerEngineCore.CurrentTrack.Artist);

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

		private RecommendationPane recommendation_pane;

		private void OnPlayerEngineEventChanged (object o, PlayerEngineEventArgs args)
		{
			if (!Enabled)
				return; 

			switch (args.Event) {
			case PlayerEngineEvent.StartOfStream:
				if (ValidTrack)
					ShowRecommendations (PlayerEngineCore.CurrentTrack.Artist);
				break;
				
			case PlayerEngineEvent.EndOfStream:
				if (PaneVisible)
					HideRecommendations ();
				break;
			}
		}

		private Source displayed_on_source;
		
		private void OnActiveSourceChanged (SourceEventArgs args)
		{
			if (args.Source == displayed_on_source) {
				PaneVisible = true;
			} else {
				PaneVisible = false;
			}
		}

		private bool PaneVisible
		{
			get {
				if (recommendation_pane == null)
					return false;

				return recommendation_pane.Visible;
			}

			set {
				if (recommendation_pane == null)
					return;

				recommendation_pane.Visible = value;
			}
		}

		private void ShowRecommendations (string artist)
		{
			lock (this) {
				if (recommendation_pane == null) {
					recommendation_pane = new RecommendationPane ();
					InterfaceElements.MainContainer.PackEnd (recommendation_pane, false, false, 0);
				}
				
				// Don't do anything if we already are showing recommendations for the
				// requested artist.
				if (PaneVisible && recommendation_pane.CurrentArtist == artist)
					return;
				
				// If we manually switch track we don't get an EndOfStream event and 
				// must clear the recommendation pane here.
				if (PaneVisible)
					HideRecommendations ();
				
				recommendation_pane.ShowRecommendations (artist);
				displayed_on_source = SourceManager.ActiveSource;
			}
		}
		
		private void HideRecommendations ()
		{
			recommendation_pane.HideRecommendations ();
		}
	}
}
