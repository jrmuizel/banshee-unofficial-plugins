using System;
using System.Data;
using System.Collections;
using Mono.Unix;
 
using Banshee.Base;
using Banshee.Sources;

using Sql;
 
namespace Banshee.Plugins.SmartPlaylists
{
    public class SmartPlaylist : Banshee.Sources.Source
    {
        private ArrayList tracks = new ArrayList();

        public string Condition;
        public string OrderBy;
        public string LimitNumber;

        private string OrderAndLimit {
            get {
                if (OrderBy == null || OrderBy == "")
                    return null;

                return String.Format ("ORDER BY {0} LIMIT {1}", OrderBy, LimitNumber);
            }
        }

        private int id;
        public int Id {
            get { return id; }
            set { id = value; }
        }

        public override int Count {
            get {
                return tracks.Count;
            }
        }
        
        public override IEnumerable Tracks {
            get {
                return tracks;
            }
        }

        public override Gdk.Pixbuf Icon {
            get {
                return Gdk.Pixbuf.LoadFromResource("source-smart-playlist.png");
            }
        }

        public override object TracksMutex {
            get { return tracks.SyncRoot; }
        }

        // For existing smart playlists that we're loading from the database
        public SmartPlaylist(int id, string name, string condition, string order_by, string limit_number) : base(name, 100)
        {
            Id = id;
            Name = name;
            Condition = condition;
            OrderBy = order_by;
            LimitNumber = limit_number;

            Globals.Library.TrackRemoved += OnLibraryTrackRemoved;

            if (Globals.Library.IsLoaded)
                OnLibraryReloaded(Globals.Library, new EventArgs());
            else
                Globals.Library.Reloaded += OnLibraryReloaded;
        }

        // For new smart playlists
        public SmartPlaylist(string name, string condition, string order_by, string limit_number) : base(name, 100)
        {
            Name = name;
            Condition = condition;
            OrderBy = order_by;
            LimitNumber = limit_number;

            Statement query = new Insert("SmartPlaylists", true,
                "Name", Name,
                "Condition", Condition,
                "OrderBy", OrderBy,
                "LimitNumber", LimitNumber
            );

            Id = Globals.Library.Db.Execute(query);

            Globals.Library.TrackRemoved += OnLibraryTrackRemoved;

            if (Globals.Library.IsLoaded)
                OnLibraryReloaded(Globals.Library, new EventArgs());
            else
                Globals.Library.Reloaded += OnLibraryReloaded;
        }

        public void RefreshMembers()
        {
            //Console.WriteLine ("Refreshing smart playlist {0} with condition {1}", Source.Name, Condition);

            // Delete existing tracks
            Globals.Library.Db.Execute(String.Format(
                "DELETE FROM SmartPlaylistEntries WHERE PlaylistId = {0}", Id
            ));

            foreach (TrackInfo track in tracks)
                OnTrackRemoved (track);

            tracks.Clear();

            // Add matching tracks
            Globals.Library.Db.Execute(String.Format(
                @"INSERT INTO SmartPlaylistEntries 
                    SELECT NULL as EntryId, {0} as PlaylistId, TrackId FROM Tracks {1} {2}",
                    Id, PrependCondition("WHERE"), OrderAndLimit
            ));

            // Load the new tracks in
            IDataReader reader = Globals.Library.Db.Query(String.Format(
                @"SELECT TrackID 
                    FROM SmartPlaylistEntries
                    WHERE PlaylistID = '{0}'",
                    Id
            ));
            
            while(reader.Read())
                AddTrack (Globals.Library.Tracks[Convert.ToInt32(reader[0])] as TrackInfo);

            reader.Dispose();
        }

        public void Check (TrackInfo track)
        {
            if (OrderAndLimit == null) {
                // If this SmartPlaylist doesn't have an OrderAndLimit clause, then it's quite simple
                // to check this track - if it matches the Condition we make sure it's in, and vice-versa
                //Console.WriteLine ("Limitless condition");

                object id = Globals.Library.Db.QuerySingle(String.Format(
                    "SELECT TrackId FROM Tracks WHERE TrackId = {0} {1}",
                    track.TrackId, PrependCondition("AND")
                ));

                if (id == null || (int) id != track.TrackId) {
                    if (tracks.Contains (track)) {
                        // If it didn't match and is in the playlist, remove it
                        RemoveTrack (track);
                    }
                } else if(! tracks.Contains (track)) {
                    // If it matched and isn't already in the playlist
                    AddTrack (track);
                }
            } else {
                // If this SmartPlaylist has an OrderAndLimit clause things are more complicated as there are a limited
                // number of tracks -- so if we remove a track, we probably need to add a different one and vice-versa.
                //Console.WriteLine ("Checking track {0} ({1}) against condition & order/limit {2} {3}", track.Uri.LocalPath, track.TrackId, Condition, OrderAndLimit);

                // See if there is a track that was in the SmartPlaylist that now shouldn't be because
                // this track we are checking displaced it.
                IDataReader reader = Globals.Library.Db.Query(String.Format(
                    "SELECT TrackId FROM SmartPlaylistEntries WHERE PlaylistID = {0} " +
                    "AND TrackId NOT IN (SELECT TrackID FROM Tracks {1} {2})",
                    Id, PrependCondition("WHERE"), OrderAndLimit
                ));

                while (reader.Read())
                    RemoveTrack  (Globals.Library.Tracks[Convert.ToInt32(reader[0])] as TrackInfo);

                reader.Dispose();

                // Remove those tracks from the database
                Globals.Library.Db.Execute(String.Format(
                    "DELETE FROM SmartPlaylistEntries WHERE PlaylistID = {0} " +
                    "AND TrackId NOT IN (SELECT TrackID FROM Tracks {1} {2})",
                    Id, PrependCondition("WHERE"), OrderAndLimit
                ));

                // If we are already a member of this smart playlist
                if (tracks.Contains (track))
                    return;

                // We have removed tracks no longer in this smart playlist, now need to add
                // tracks that replace those that were removed (if any)
                IDataReader new_tracks = Globals.Library.Db.Query(String.Format(
                    @"SELECT TrackId FROM Tracks 
                        WHERE TrackID NOT IN (SELECT TrackID FROM SmartPlaylistEntries WHERE PlaylistID = {0})
                        AND TrackID IN (SELECT TrackID FROM Tracks {1} {2})",
                    Id, PrependCondition("WHERE"), OrderAndLimit
                ));

                bool have_new_tracks = false;
                while (new_tracks.Read()) {
                    AddTrack (Globals.Library.Tracks[Convert.ToInt32(new_tracks[0])] as TrackInfo);
                    have_new_tracks = true;
                }

                new_tracks.Dispose();

                if (have_new_tracks) {
                    Globals.Library.Db.Execute(String.Format(
                        @"INSERT INTO SmartPlaylistEntries 
                            SELECT NULL as EntryId, {0} as PlaylistId, TrackId FROM Tracks 
                            WHERE TrackID NOT IN (SELECT TrackID FROM SmartPlaylistEntries WHERE PlaylistID = {0})
                            AND TrackID IN (SELECT TrackID FROM Tracks {1} {2})",
                        Id, PrependCondition("WHERE"), OrderAndLimit
                    ));
                }
            }
        }

        public override void Commit ()
        {
            Statement query = new Update("SmartPlaylists",
                "Name", Name,
                "Condition", Condition,
                "OrderBy", OrderBy,
                "LimitNumber", LimitNumber) + 
                new Where(new Compare("PlaylistID", Op.EqualTo, Id));

            Globals.Library.Db.Execute(query);

            // Make sure the tracks are up to date
            Globals.Library.Db.Execute(String.Format(
                @"DELETE FROM SmartPlaylistEntries
                    WHERE PlaylistID = '{0}'",
                    id
            ));

            
            lock(TracksMutex) {
                foreach(TrackInfo track in Tracks) {
                    if(track == null || track.TrackId <= 0)
                        continue;
                        
                    Globals.Library.Db.Execute(String.Format(
                        @"INSERT INTO SmartPlaylistEntries 
                            VALUES (NULL, '{0}', '{1}')",
                            id, track.TrackId
                    ));
                }
            }
        }

        private string PrependCondition (string with)
        {
            return (Condition == null) ? " " : with + " " + Condition;
        }

        public override void ShowPropertiesDialog()
        {
            SmartPlaylists.Editor ed = new SmartPlaylists.Editor (this);
            ed.RunDialog ();
        }

        public override void Reorder(TrackInfo track, int position)
        {
            RemoveTrack(track);
            lock(TracksMutex) {
                tracks.Insert(position, track);
            }
        }

        private void OnLibraryReloaded (object o, EventArgs args)
        {
            RefreshMembers();
        }

        private void OnLibraryTrackRemoved(object o, LibraryTrackRemovedArgs args)
        {
            if(args.Track != null) {
                if(tracks.Contains(args.Track)) {
                    RemoveTrack(args.Track);
                    
                    if(Count == 0) {
                        Delete();
                    } else {
                        Commit();
                    }
                }
                
                return;
            } else if(args.Tracks == null) {
                return;
            }
            
            int removed_count = 0;
            
            foreach(TrackInfo track in args.Tracks) {
                if(tracks.Contains(track)) {
                    RemoveTrack (track);
                    removed_count++;
                }
            }
            
            if(removed_count > 0) {
                if(Count == 0) {
                    Delete();
                } else {
                    Commit();
                }
            }
        }

        public override void AddTrack(TrackInfo track)
        {
            //Console.WriteLine ("Adding track ... == null ? {0}", track == null);
            if(track is LibraryTrackInfo) {
                //Console.WriteLine ("its a LibraryTrackInfo! track = {0}", track);
                lock(TracksMutex) {
                    tracks.Add(track);
                }

                OnTrackAdded (track);
            }
        }
        
        public override void RemoveTrack(TrackInfo track)
        {
            lock(TracksMutex) {
                tracks.Remove (track);
            }

            OnTrackRemoved (track);
        }

        protected override bool UpdateName(string oldName, string newName)
        {
            if (oldName == newName)
                return false;

            Name = newName;
            Commit();
            return true;
        }

        public override void Delete()
        {
            Globals.Library.Db.Execute(String.Format(
                @"DELETE FROM SmartPlaylistEntries
                    WHERE PlaylistID = '{0}'",
                    id
            ));
            
            Globals.Library.Db.Execute(String.Format(
                @"DELETE FROM SmartPlaylists
                    WHERE PlaylistID = '{0}'",
                    id
            ));
            
            SourceManager.RemoveSource(this);
        }

        public static void LoadFromReader (IDataReader reader)
        {
            int id = (int) reader[0];
            string name = reader[1] as string;
            string condition = reader[2] as string;
            string order_by = reader[3] as string;
            string limit_number = reader[4] as string;

            SmartPlaylist playlist = new SmartPlaylist (id, name, condition, order_by, limit_number);
            SourceManager.AddSource(playlist);
        }
    }
}
