using System;
using System.Data;
using System.Collections;
using Gtk;
 
using Banshee.Base;
using Banshee.Sources;
using Banshee.Database;
using Banshee.Plugins;

namespace Banshee.Plugins.SmartPlaylists
{
    public class Plugin : Banshee.Plugins.Plugin
    {
        private ArrayList playlists = new ArrayList ();

        private Menu musicMenu;
        private MenuItem addItem;

        private static Plugin instance = null;

        private uint timeout_id = 0;

        public static Plugin Instance {
            get { return instance; }
        }

        protected override string ConfigurationName {
            get { return "SmartPlaylists"; }
        }

        public override string DisplayName {
            get { return Catalog.GetString("Smart Playlists"); }
        }
        
        public override string Description {
            get {
                return Catalog.GetString(
                    "Create playlists that automatically add and remove songs based on customizable queries."
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

        public Plugin ()
        {
            instance = this;
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
                        LimitNumber TEXT,
                        LimitCriterion INTEGER)
                ");
            } else {
                // Database Schema Updates
                try {
                    Globals.Library.Db.QuerySingle("SELECT LimitCriterion FROM SmartPlaylists LIMIT 1");
                } catch(ApplicationException) {
                    LogCore.Instance.PushDebug("Adding new database column", "LimitCriterion INTEGER");
                    Globals.Library.Db.Execute("ALTER TABLE SmartPlaylists ADD LimitCriterion INTEGER");
                    Globals.Library.Db.Execute("UPDATE SmartPlaylists SET LimitCriterion = 0");
                }
            }

            if(!Globals.Library.Db.TableExists("SmartPlaylistEntries")) {
                Globals.Library.Db.Execute(@"
                    CREATE TABLE SmartPlaylistEntries (
                        EntryID     INTEGER PRIMARY KEY,
                        PlaylistID  INTEGER NOT NULL,
                        TrackID     INTEGER NOT NULL)
                ");
            }

            // Listen for added/removed sources and added/changed songs
            SourceManager.SourceAdded += HandleSourceAdded;
            SourceManager.SourceRemoved += HandleSourceRemoved;
            Globals.Library.Reloaded += HandleLibraryReloaded;
            Globals.Library.TrackAdded += HandleTrackAdded;
            Globals.Library.TrackRemoved += HandleTrackRemoved;

            // Load existing smart playlists
            IDataReader reader = Globals.Library.Db.Query(String.Format(
                "SELECT PlaylistID, Name, Condition, OrderBy, LimitNumber, LimitCriterion FROM SmartPlaylists"
            ));

            while (reader.Read()) {
                SmartPlaylist.LoadFromReader (reader);
            }

            reader.Dispose();

            // Add a menu option to create a new smart playlist
            if(!Globals.UIManager.IsInitialized) {
                Globals.UIManager.Initialized += OnUIManagerInitialized;
            } else {
                OnUIManagerInitialized (null, null);
            }
        }

        private void OnUIManagerInitialized(object o, EventArgs args)
        {
            musicMenu = (Globals.ActionManager.GetWidget ("/MainMenu/MusicMenu") as MenuItem).Submenu as Menu;
            addItem = new MenuItem (Catalog.GetString("New Smart Playlist..."));
            addItem.Activated += delegate {
                SmartPlaylists.Editor ed = new SmartPlaylists.Editor ();
                ed.RunDialog ();
            };

            // Insert it right after the New Playlist item
            musicMenu.Insert (addItem, 2);
            addItem.Show ();
        }

        protected override void PluginDispose()
        {
            if (timeout_id != 0)
                GLib.Source.Remove (timeout_id);

            if (musicMenu != null)
                musicMenu.Remove(addItem);

            SourceManager.SourceAdded -= HandleSourceAdded;
            SourceManager.SourceRemoved -= HandleSourceRemoved;

            foreach (SmartPlaylist playlist in playlists)
                LibrarySource.Instance.RemoveChildSource(playlist);

            playlists.Clear();

            instance = null;
        }

        /*public override Widget GetConfigurationWidget ()
        {
            return new Label ("Smart Playlist Configuration..");
        }*/

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

            StartTimer (playlist);

            playlists.Add(playlist);
        }

        private void HandleSourceRemoved (SourceEventArgs args)
        {
            SmartPlaylist playlist = args.Source as SmartPlaylist;
            if (playlist == null)
                return;

            playlists.Remove (playlist);

            StopTimer();
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

        public void StartTimer (SmartPlaylist playlist)
        {
            // Check if the playlist is time-dependent, and if it is,
            // start the auto-refresh timer if needed.
            if (timeout_id == 0 && playlist.TimeDependent) {
                LogCore.Instance.PushInformation (
                        "Starting timer",
                        "Time-dependent smart playlist added, so starting auto-refresh timer.",
                        false
                );
                timeout_id = GLib.Timeout.Add(1000*60, OnTimerBeep);
            }
        }

        public void StopTimer ()
        {
            // If the timer is going and there are no more time-dependent playlists,
            // stop the timer.
            if (timeout_id != 0) {
                foreach (SmartPlaylist p in playlists) {
                    if (p.TimeDependent) {
                        return;
                    }
                }

                // No more time-dependent playlists, so remove the timer
                LogCore.Instance.PushInformation (
                        "Stopping timer",
                        "There are no time-dependent smart playlists, so stopping auto-refresh timer.",
                        false
                );

                GLib.Source.Remove (timeout_id);
                timeout_id = 0;
            }
        }


        private bool OnTimerBeep ()
        {
            foreach (SmartPlaylist p in playlists) {
                if (p.TimeDependent) {
                    p.RefreshMembers();
                }
            }

            // Keep the timer going
            return true;
        }

        private void CheckTrack (TrackInfo track)
        {
            foreach (SmartPlaylist playlist in playlists)
                playlist.Check (track);
        }
    }
}
