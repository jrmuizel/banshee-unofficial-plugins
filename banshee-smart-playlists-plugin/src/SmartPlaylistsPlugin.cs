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
    public class SmartPlaylistsPlugin : Banshee.Plugins.Plugin
    {
        protected override string ConfigurationName { get { return "SmartPlaylists"; } }
        public override string DisplayName { get { return "Smart Playlists"; } }

        private Hashtable playlists = new Hashtable ();

        private Menu musicMenu;
        private MenuItem addItem;

        private Menu sourceMenu;
        private MenuItem editItem;
        
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
                Console.WriteLine("Creating smart playlists table");
                Globals.Library.Db.Execute(@"
                    CREATE TABLE SmartPlaylists (
                        PlaylistID INTEGER PRIMARY KEY,
                        Condition TEXT,
                        OrderBy TEXT,
                        LimitNumber TEXT)
                ");
            }

            // Listen for deleted playlists and new/changed songs
            SourceManager.SourceAdded += HandleSourceAdded;
            SourceManager.SourceRemoved += HandleSourceRemoved;
            Globals.Library.Reloaded += HandleLibraryReloaded;
            Globals.Library.TrackAdded += HandleTrackAdded;
            Globals.Library.TrackRemoved += HandleTrackRemoved;


            Console.WriteLine ("Adding menu stuff");
            // Add a menu option to create a new smart playlist
            musicMenu = (Globals.ActionManager.GetWidget ("/MainMenu/MusicMenu") as MenuItem).Submenu as Menu;
            addItem = new MenuItem (Catalog.GetString("New Smart Playlist..."));
            addItem.Activated += delegate {
                SmartPlaylistEditor ed = new SmartPlaylistEditor ();
                ed.RunDialog ();
            };
            // Insert right after the New Playlist item
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

            sourceMenu = Globals.ActionManager.GetWidget ("/SourceMenu") as Menu;
            editItem = new MenuItem (Catalog.GetString("Edit Smart Playlist..."));
            editItem.Activated += delegate {
                SmartPlaylistEditor ed = new SmartPlaylistEditor (playlists[SourceManager.ActiveSource] as SmartPlaylist);
                ed.RunDialog ();
            };
            sourceMenu.Insert (editItem, 2);
            editItem.Show ();
        }

        protected override void PluginDispose()
        {
            Console.WriteLine("Disposing Smart Playlists Plugin");

            musicMenu.Remove(addItem);
            sourceMenu.Remove(editItem);
        }

        private void HandleLibraryReloaded (object sender, EventArgs args)
        {
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
            PlaylistSource playlist = args.Source as PlaylistSource;

            if (playlist == null)
                return;

            IDataReader reader = Globals.Library.Db.Query(String.Format(
                "SELECT Condition, OrderBy, LimitNumber FROM SmartPlaylists WHERE PlaylistID = {0}",
                playlist.Id
            ));

            if (!reader.Read())
                return;

            string condition = reader[0] as string;
            string order_by = reader[1] as string;
            string limit_number = reader[2] as string;

            reader.Dispose();

            Console.WriteLine ("Adding smart playlist {0}, id {1}", playlist.Name, playlist.Id);
            playlists.Add(playlist, new SmartPlaylist(playlist, condition, order_by, limit_number));
        }

        private void HandleSourceRemoved (SourceEventArgs args)
        {
            PlaylistSource source = args.Source as PlaylistSource;
            if (source == null)
                return;

            Console.WriteLine ("received notice that {0} was removed", args.Source.Name);
            playlists.Remove (source);

            // Delete it from the database
            Globals.Library.Db.Execute(String.Format(
                "DELETE FROM SmartPlaylists WHERE PlaylistId = {0}",
                source.Id
            ));
        }

        private void HandleTrackAdded (object sender, LibraryTrackAddedArgs args)
        {
            Console.WriteLine ("Handling track added {0}", args.Track.Uri.LocalPath);
            args.Track.Changed += HandleTrackChanged;
            CheckTrack (args.Track);
        }

        private void HandleTrackChanged (object sender, EventArgs args)
        {
            TrackInfo track = sender as TrackInfo;

            Console.WriteLine ("Handling track changed");
            if (track != null) {
                Console.WriteLine ("...track= {0}", track.Uri.LocalPath);
                CheckTrack (track);
            }
            else {
                Console.WriteLine ("track == null");
            }
        }

        private void HandleTrackRemoved (object sender, LibraryTrackRemovedArgs args)
        {
            if (args.Track == null)
                return;

            Console.WriteLine ("Handling track removed {0}", args.Track.Uri.LocalPath);
            args.Track.Changed -= HandleTrackChanged;
        }

        private void CheckTrack (TrackInfo track)
        {
            Console.WriteLine ("Checking track {0} agaisnt smart playlists", track.Uri.LocalPath);
            foreach (SmartPlaylist playlist in playlists.Values)
                playlist.Check (track);
        }
    }
}
