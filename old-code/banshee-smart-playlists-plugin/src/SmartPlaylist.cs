using System;
using System.Data;
using Mono.Unix;
 
using Banshee.Base;
using Banshee.Sources;

using Sql;
 
namespace Banshee.Plugins.SmartPlaylists
{
    /*public class SmartPlaylistSource : PlaylistSource
    {
        public override bool IsUserEditable
    }*/

    public class SmartPlaylist
    {
        public string Name {
            get {
                return Source.Name;
            }
            set {
                Source.Rename (value);
            }
        }

        public string Condition;
        public string OrderBy;
        public string LimitNumber;

        public PlaylistSource Source;

        public SmartPlaylist(PlaylistSource source, string condition, string order_by, string limit_number)
        {
            Source = source;
            Condition = condition;
            OrderBy = order_by;
            LimitNumber = limit_number;

            RefreshMembers();
        }

        public SmartPlaylist(string name, string condition, string order_by, string limit_number)
        {
            Source = new PlaylistSource ();

            Name = name;
            Condition = condition;
            OrderBy = order_by;
            LimitNumber = limit_number;

            Globals.Library.Db.Execute(String.Format(
                @"INSERT INTO SmartPlaylists (PlaylistID, Condition, OrderBy, LimitNumber)
                VALUES ({0}, '{1}', '{2}', '{3}')",
                Source.Id,
                Sql.Statement.EscapeQuotes(Condition),
                Sql.Statement.EscapeQuotes(OrderBy),
                LimitNumber
            ));
        }

        public void RefreshMembers()
        {
            if (Condition == null)
                return;

            //Console.WriteLine ("Refreshing smart playlist {0} with condition {1}", Source.Name, Condition);

            // Delete existing tracks
            Globals.Library.Db.Execute(String.Format(
                "DELETE FROM PlaylistEntries WHERE PlaylistId = {0}", Source.Id
            ));

            // Add matching tracks
            Globals.Library.Db.Execute(String.Format(
                @"INSERT INTO PlaylistEntries 
                    SELECT NULL as EntryId, {0} as PlaylistId, TrackId FROM Tracks {1} {2}",
                    Source.Id, PrependCondition("WHERE"), OrderAndLimit
            ));

            Source.ClearTracks();

            // Load the new tracks in
            IDataReader reader = Globals.Library.Db.Query(String.Format(
                @"SELECT TrackID 
                    FROM PlaylistEntries
                    WHERE PlaylistID = '{0}'",
                    Source.Id
            ));
            
            while(reader.Read())
                Source.AddTrack (Globals.Library.Tracks[Convert.ToInt32(reader[0])] as TrackInfo);
            
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
                    if (Source.ContainsTrack (track)) {
                        // If it didn't match and is in the playlist, remove it
                        Source.RemoveTrack(track);
                    }
                } else if(! Source.ContainsTrack (track)) {
                    // If it matched and isn't already in the playlist
                    Source.AddTrack (track);
                }
            } else {
                // If this SmartPlaylist has an OrderAndLimit clause things are more complicated as there are a limited
                // number of tracks -- so if we remove a track, we probably need to add a different one and vice-versa.
                //Console.WriteLine ("Checking track {0} ({1}) against condition & order/limit {2} {3}", track.Uri.LocalPath, track.TrackId, Condition, OrderAndLimit);

                // See if there is a track that was in the SmartPlaylist that now shouldn't be because
                // this track we are checking displaced it.
                IDataReader reader = Globals.Library.Db.Query(String.Format(
                    "SELECT TrackId FROM PlaylistEntries WHERE PlaylistID = {0} " +
                    "AND TrackId NOT IN (SELECT TrackID FROM Tracks {1} {2})",
                    Source.Id, PrependCondition("WHERE"), OrderAndLimit
                ));

                while (reader.Read())
                    Source.RemoveTrack (Globals.Library.Tracks[Convert.ToInt32(reader[0])] as TrackInfo);

                reader.Dispose();

                // Remove those tracks from the database
                Globals.Library.Db.Execute(String.Format(
                    "DELETE FROM PlaylistEntries WHERE PlaylistID = {0} " +
                    "AND TrackId NOT IN (SELECT TrackID FROM Tracks {1} {2})",
                    Source.Id, PrependCondition("WHERE"), OrderAndLimit
                ));

                // If we are already a member of this smart playlist
                if (Source.ContainsTrack (track))
                    return;

                // We have removed tracks no longer in this smart playlist, now need to add
                // tracks that replace those that were removed (if any)
                IDataReader new_tracks = Globals.Library.Db.Query(String.Format(
                    @"SELECT TrackId FROM Tracks 
                        WHERE TrackID NOT IN (SELECT TrackID FROM PlaylistEntries WHERE PlaylistID = {0})
                        AND TrackID IN (SELECT TrackID FROM Tracks {1} {2})",
                    Source.Id, PrependCondition("WHERE"), OrderAndLimit
                ));

                bool have_new_tracks = false;
                while (new_tracks.Read()) {
                    Source.AddTrack (Globals.Library.Tracks[Convert.ToInt32(new_tracks[0])] as TrackInfo);
                    have_new_tracks = true;
                }

                new_tracks.Dispose();

                if (have_new_tracks) {
                    Globals.Library.Db.Execute(String.Format(
                        @"INSERT INTO PlaylistEntries 
                            SELECT NULL as EntryId, {0} as PlaylistId, TrackId FROM Tracks 
                            WHERE TrackID NOT IN (SELECT TrackID FROM PlaylistEntries WHERE PlaylistID = {0})
                            AND TrackID IN (SELECT TrackID FROM Tracks {1} {2})",
                        Source.Id, PrependCondition("WHERE"), OrderAndLimit
                    ));
                }
            }
        }

        public void Commit ()
        {
            Statement query = new Update("SmartPlaylists",
                "Condition", Condition,
                "OrderBy", OrderBy,
                "LimitNumber", LimitNumber +
                new Where(new Compare("PlaylistID", Op.EqualTo, Source.Id))
            );

            Globals.Library.Db.Execute(query);
        }

        private string PrependCondition (string with)
        {
            return (Condition == null) ? " " : with + " " + Condition;
        }

        private string OrderAndLimit {
            get {
                if (OrderBy == null || OrderBy == "")
                    return null;

                return String.Format ("ORDER BY {0} LIMIT {1}", OrderBy, LimitNumber);
            }
        }
    }
}
