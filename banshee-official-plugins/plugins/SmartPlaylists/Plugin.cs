using System;
using System.Data;
using System.Collections;
using Mono.Unix;
using Gtk;
 
using Banshee.Base;
using Banshee.Sources;
using Banshee.Database;
 
namespace Banshee.Plugins.SmartPlaylists
{
    public class Plugin : Banshee.Plugins.Plugin
    {
        private ArrayList playlists = new ArrayList ();

        private Menu musicMenu;
        private MenuItem addItem;

        protected override string ConfigurationName {
            get { return "SmartPlaylists"; }
        }

        public override string DisplayName {
            get { return Catalog.GetString("Smart Playlists"); }
        }
        
        public override string Description {
            get {
                return Catalog.GetString(
                    "Allows for playlists based on a set of parameters"
                );
            }
        }
        
        public override string [] Authors {
            get {
                return new string [] {
                    "Aaron Bockover",
                    "Gabriel Burt"
                };
            }
        }
 
        protected override void PluginInitialize()
        {
            // Check that our SmartPlaylists table exists in the database, otherwise make it
            if(!Globals.Library.Db.TableExists("SmartPlaylists")) {
                Globals.Library.Db.Execute(@"
                    CREATE TABLE SmartPlaylists (
                        PlaylistID  INTEGER PRIMARY KEY,
                        Name        TEXT NOT NULL,
                        Condition   TEXT,
                        OrderBy     TEXT,
                        LimitNumber TEXT)
                ");
            }

            if(!Globals.Library.Db.TableExists("SmartPlaylistEntries")) {
                Globals.Library.Db.Execute(@"
                    CREATE TABLE SmartPlaylistEntries (
                        EntryID     INTEGER PRIMARY KEY,
                        PlaylistID  INTEGER NOT NULL,
                        TrackID     INTEGER NOT NULL)
                ");
            }

            // Listen for added/removed source and added/changed songs
            SourceManager.SourceAdded += HandleSourceAdded;
            SourceManager.SourceRemoved += HandleSourceRemoved;
            Globals.Library.Reloaded += HandleLibraryReloaded;
            Globals.Library.TrackAdded += HandleTrackAdded;
            Globals.Library.TrackRemoved += HandleTrackRemoved;

            // Load existing smart playlists
            IDataReader reader = Globals.Library.Db.Query(String.Format(
                "SELECT PlaylistID, Name, Condition, OrderBy, LimitNumber FROM SmartPlaylists"
            ));

            while (reader.Read()) {
                SmartPlaylist.LoadFromReader (reader);
            }

            reader.Dispose();

            // Add a menu option to create a new smart playlist
            musicMenu = (Globals.ActionManager.GetWidget ("/MainMenu/MusicMenu") as MenuItem).Submenu as Menu;
            addItem = new MenuItem (Catalog.GetString("New Smart Playlist..."));
            addItem.Activated += delegate {
                SmartPlaylists.Editor ed = new SmartPlaylists.Editor ();
                ed.RunDialog ();
            };

            // Insert it right after the New Playlist item
            musicMenu.Insert (addItem, 2);
            addItem.Show ();

            // Add option for editing smart playlists
            /*Globals.ActionManager.PlaylistActions.Add(new ActionEntry [] {
                new ActionEntry("EditSmartPlaylistAction", null,
                    Catalog.GetString("Edit Smart Playlist"), "<shift>Delete",
                    Catalog.GetString("Edit the active smart playlist"), null)
            });

            Globals.ActionManager.PlaylistActions.GetAction ("EditSmartPlaylistAction").Visible = true;
            Globals.ActionManager.PlaylistActions.GetAction ("EditSmartPlaylistAction").Sensitive = true;*/
        }

        protected override void PluginDispose()
        {
            musicMenu.Remove(addItem);

            SourceManager.SourceAdded -= HandleSourceAdded;
            SourceManager.SourceRemoved -= HandleSourceRemoved;

            foreach (SmartPlaylist playlist in playlists)
                SourceManager.RemoveSource(playlist);

            playlists.Clear();
        }

        private void HandleLibraryReloaded (object sender, EventArgs args)
        {
            // Listen for changes to any track to keep our playlists up to date
            IDataReader reader = Globals.Library.Db.Query(String.Format(
                "SELECT TrackID FROM Tracks"
            ));

            while (reader.Read()) {
                LibraryTrackInfo track = Globals.Library.GetTrack (Convert.ToInt32(reader[0]));
                if (track != null)
                    track.Changed += HandleTrackChanged;
            }

            reader.Dispose();

            Globals.Library.Reloaded -= HandleLibraryReloaded;
        }

        private void HandleSourceAdded (SourceEventArgs args)
        {
            SmartPlaylist playlist = args.Source as SmartPlaylist;
            if (playlist == null)
                return;

            playlists.Add(playlist);
        }

        private void HandleSourceRemoved (SourceEventArgs args)
        {
            SmartPlaylist playlist = args.Source as SmartPlaylist;
            if (playlist == null)
                return;

            playlists.Remove (playlist);
        }

        private void HandleTrackAdded (object sender, LibraryTrackAddedArgs args)
        {
            args.Track.Changed += HandleTrackChanged;
            CheckTrack (args.Track);
        }

        private void HandleTrackChanged (object sender, EventArgs args)
        {
            TrackInfo track = sender as TrackInfo;

            if (track != null)
                CheckTrack (track);
        }

        private void HandleTrackRemoved (object sender, LibraryTrackRemovedArgs args)
        {
            if (args.Track != null)
                args.Track.Changed -= HandleTrackChanged;
        }

        private void CheckTrack (TrackInfo track)
        {
            foreach (SmartPlaylist playlist in playlists)
                playlist.Check (track);
        }
    }
}
