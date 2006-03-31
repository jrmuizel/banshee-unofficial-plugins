using System;
using System.Data;
using Mono.Unix;
 
using Banshee.Base;
using Banshee.Sources;

using Sql;
 
namespace Banshee.Plugins.SmartPlaylists
{
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
        public string OrderAndLimit;

        public PlaylistSource Source;

        public SmartPlaylist(PlaylistSource source, string condition, string order_and_limit)
        {
            Source = source;
            Condition = condition;
            OrderAndLimit = order_and_limit;

            RefreshMembers();
        }

        public SmartPlaylist(string name, string condition, string order_and_limit)
        {
            Source = new PlaylistSource ();

            Name = name;
            Condition = condition;
            OrderAndLimit = order_and_limit;

            Globals.Library.Db.Execute(String.Format(
                @"INSERT INTO SmartPlaylists (PlaylistID, Condition, OrderAndLimit)
                VALUES ({0}, '{1}', '{2}')",
                Source.Id,
                Sql.Statement.EscapeQuotes(Condition),
                Sql.Statement.EscapeQuotes(OrderAndLimit)
            ));

            RefreshMembers();
        }

        public void RefreshMembers()
        {
            if (Condition == null)
                return;

            Console.WriteLine ("Refreshing smart playlist {0} with condition {1}", Source.Name, Condition);

            // Delete existing tracks
            Globals.Library.Db.Execute(String.Format(
                "DELETE FROM PlaylistEntries WHERE PlaylistId = {0}", Source.Id
            ));

            // Add matching tracks
            Globals.Library.Db.Execute(String.Format(
                @"INSERT INTO PlaylistEntries 
                    SELECT NULL as EntryId, {0} as PlaylistId, TrackId FROM Tracks {1} {2}",
                    Source.Id, Condition, OrderAndLimit
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
            Console.WriteLine ("Checking track {0} ({1}) against condition {2}", track.Uri.LocalPath, track.TrackId, Condition);
            object id = Globals.Library.Db.QuerySingle(String.Format(
                "SELECT TrackId FROM Tracks WHERE TrackId = {0} AND {1}",
                track.TrackId, Condition
            ));

            if (id == null || (int) id != track.TrackId) {
                // If it didn't match and isn't in the playlist, remove it
                if (Source.ContainsTrack (track))
                    Source.RemoveTrack(track);
                return;
            }

            // If it matched and isn't already in the playlist
            if (! Source.ContainsTrack (track))
                Source.AddTrack (track);
        }

        public void Commit ()
        {
            Statement query = new Update("SmartPlaylists",
                "Condition", Condition,
                "OrderAndLimit", OrderAndLimit +
                new Where(new Compare("PlaylistID", Op.EqualTo, Source.Id))
            );

            Globals.Library.Db.Execute(query);
        }
    }
}
