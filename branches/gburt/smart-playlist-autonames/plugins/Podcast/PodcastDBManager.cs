/***************************************************************************
 *  PodcastDBManager.cs
 *
 *  Written by Mike Urbanski <michael.c.urbanski@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW:
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Data;
using System.Text;
using System.Collections;

using Banshee.Base;

namespace Banshee.Plugins.Podcast
{
    internal static class PodcastDBManager
    {
        public static void InitPodcastDatabase ()
        {
            if(!Globals.Library.Db.TableExists("PodcastFeeds"))
            {

                Globals.Library.Db.Query (@"
                                          CREATE TABLE PodcastFeeds (
                                          PodcastFeedID INTEGER PRIMARY KEY,
                                          Title TEXT NOT NULL,
                                          FeedUrl TEXT NOT NULL,
                                          Link TEXT DEFAULT '',
                                          Description TEXT DEFAULT '',
                                          Image TEXT DEFAULT '',
                                          LastUpdated TIMESTAMP,
                                          Subscribed INTERGER DEFAULT 1,
                                          SyncPreference INTEGER DEFAULT 1
                                          )"
                                         );
            }

            if(!Globals.Library.Db.TableExists("Podcasts"))
            {
                // TODO Add downloaded timestamp
                Globals.Library.Db.Query (@"
                                          CREATE TABLE Podcasts (
                                          PodcastID INTEGER PRIMARY KEY,
                                          PodcastFeedID INTEGER NOT NULL,

                                          Title TEXT DEFAULT '',
                                          Link TEXT DEFAULT '',
                                          PubDate DATE NOT NULL,
                                          Description TEXT DEFAULT '',
                                          Author TEXT DEFAULT '',
                                          LocalPath TEXT DEFAULT '',
                                          Url TEXT NOT NULL,
                                          MimeType TEXT NOT NULL,
                                          Length INTEGER NOT NULL,
                                          Downloaded INTEGER DEFAULT 0,
                                          Active INTEGER DEFAULT 1
                                          )"
                                         );
            }
        }

        public static PodcastFeedInfo[] LoadPodcastFeeds ()
        {
            ArrayList podcastFeeds = new ArrayList ();

            IDataReader feed_reader = Globals.Library.Db.Query (@"
                                      SELECT * FROM PodcastFeeds ORDER BY Title
                                      ");

            while(feed_reader.Read())
            {

                PodcastFeedInfo feed = new PodcastFeedInfo (
                                           feed_reader.GetInt32 (0), feed_reader.GetString (1),
                                           feed_reader.GetString (2), feed_reader.GetString (3),
                                           feed_reader.GetString (4), feed_reader.GetString (5),
                                           feed_reader.GetDateTime(6), feed_reader.GetBoolean(7),
                                           (SyncPreference)feed_reader.GetInt32(8)
                                       );

                podcastFeeds.Add (feed);
                feed.Add (LoadPodcasts (feed));
            }

            feed_reader.Close ();

            return podcastFeeds.ToArray (typeof (PodcastFeedInfo)) as PodcastFeedInfo[];
        }

        private static PodcastInfo[] LoadPodcasts (PodcastFeedInfo feed)
        {
            ArrayList podcasts = new ArrayList ();

            IDataReader podcast_reader = Globals.Library.Db.Query (
                                             String.Format (
                                                 @"SELECT * FROM Podcasts
                                                 WHERE PodcastFeedID = {0}",
                                                 feed.ID
                                             )
                                         );

            while (podcast_reader.Read())
            {
                podcasts.Add(
                    new PodcastInfo (
                        feed, podcast_reader.GetInt32 (0), podcast_reader.GetString (2),
                        podcast_reader.GetString (3), podcast_reader.GetDateTime (4),
                        podcast_reader.GetString (5), podcast_reader.GetString (6),
                        podcast_reader.GetString (7), podcast_reader.GetString (8),
                        podcast_reader.GetString (9), podcast_reader.GetInt64 (10),
                        podcast_reader.GetBoolean (11), podcast_reader.GetBoolean (12)
                    )
                );
            }

            podcast_reader.Close ();

            return podcasts.ToArray (typeof (PodcastInfo)) as PodcastInfo[];
        }

        public static int LocalPodcastCount ()
        {
            string query =
                @"SELECT COUNT (*)
                FROM Podcasts
                WHERE Downloaded > 0 AND Active > 0
                LIMIT 1";

            try
            {
                return Convert.ToInt32(Globals.Library.Db.QuerySingle(query));
            }
            catch(Exception)
            {
                return -1;
            }
        }

        public static int Commit (PodcastFeedInfo feed)
        {
            int ret = 0;

            if (feed.ID != 0)
            {
                ret = Globals.Library.Db.Execute( String.Format(
                                                      @"UPDATE PodcastFeeds
                                                      SET Title='{0}', FeedUrl='{1}', Link='{2}',
                                                      Description='{3}', Image='{4}', LastUpdated='{5}',
                                                      Subscribed={6}, SyncPreference={7} WHERE PodcastFeedID={8}",

                                                      Sql.Statement.EscapeQuotes(feed.Title),
                                                      feed.Url.ToString (), feed.Link,
                                                      Sql.Statement.EscapeQuotes(feed.Description),
                                                      Sql.Statement.EscapeQuotes(feed.Image), feed.LastUpdated.ToString (),
                                                      Convert.ToInt32(feed.IsSubscribed), (int)feed.SyncPreference,
                                                      feed.ID
                                                  ));
            }
            else
            {
                ret = Globals.Library.Db.Execute( String.Format(
                                                      @"INSERT INTO PodcastFeeds
                                                      VALUES (NULL, '{0}', '{1}', '{2}',
                                                      '{3}', '{4}', '{5}', {6}, {7})",
                                                      Sql.Statement.EscapeQuotes(feed.Title),
                                                      feed.Url.ToString (), feed.Link,
                                                      Sql.Statement.EscapeQuotes(feed.Description),
                                                      Sql.Statement.EscapeQuotes(feed.Image), feed.LastUpdated.ToString (),
                                                      Convert.ToInt32(feed.IsSubscribed), (int)feed.SyncPreference
                                                  ));
            }

            return ret;
        }

        public static int Commit (PodcastInfo pi)
        {
            int ret = 0;

            if (pi.ID != 0)
            {

                ret = Globals.Library.Db.Execute(String.Format(
                                                     @"UPDATE Podcasts
                                                     SET PodcastFeedID={0}, Title='{1}', Link='{2}', PubDate='{3}',
                                                     Description='{4}', Author='{5}', LocalPath='{6}', Url='{7}',
                                                     MimeType='{8}', Length={9}, Downloaded={10}, Active={11}
                                                     WHERE PodcastID={12}",
                                                     pi.Feed.ID, Sql.Statement.EscapeQuotes(pi.Title),
                                                     pi.Link, pi.PubDate.ToString (),
                                                     Sql.Statement.EscapeQuotes(pi.Description),
                                                     Sql.Statement.EscapeQuotes(pi.Author),
                                                     Sql.Statement.EscapeQuotes(pi.LocalPath),
                                                     pi.Url.ToString (), Sql.Statement.EscapeQuotes(pi.MimeType), pi.Length,
                                                     Convert.ToInt32(pi.IsDownloaded), Convert.ToInt32(pi.IsActive),pi.ID
                                                 ));
            }
            else
            {
                ret = Globals.Library.Db.Execute(String.Format(
                                                     @"INSERT INTO Podcasts
                                                     VALUES (NULL, {0}, '{1}', '{2}',
                                                     '{3}', '{4}', '{5}', '{6}', '{7}',
                                                     '{8}', {9}, {10}, {11})",
                                                     pi.Feed.ID, Sql.Statement.EscapeQuotes(pi.Title),
                                                     pi.Link, pi.PubDate.ToString (),
                                                     Sql.Statement.EscapeQuotes(pi.Description),
                                                     Sql.Statement.EscapeQuotes(pi.Author),
                                                     Sql.Statement.EscapeQuotes(pi.LocalPath),
                                                     pi.Url.ToString (),
                                                     Sql.Statement.EscapeQuotes(pi.MimeType), pi.Length,
                                                     Convert.ToInt32(pi.IsDownloaded), Convert.ToInt32(pi.IsActive)
                                                 ));
            }

            return ret;
        }

        public static void Delete (PodcastFeedInfo pfi)
        {
            if (pfi == null)
            {
                throw new ArgumentNullException ("pfi");
            }

            Globals.Library.Db.Execute(String.Format(
                                           @"DELETE FROM PodcastFeeds
                                           WHERE PodcastFeedID = '{0}'",
                                           pfi.ID
                                       ));

            Globals.Library.Db.Execute(String.Format(
                                           @"DELETE FROM Podcasts
                                           WHERE PodcastFeedID = '{0}'",
                                           pfi.ID
                                       ));
        }

        private static readonly string base_podcast_remove_query =
            @"DELETE FROM Podcasts WHERE";

        public static void Delete (PodcastInfo pi)
        {
            if (pi == null)
            {
                throw new ArgumentNullException ("pi");
            }

            string query = String.Format(
                               @"{0} PodcastID = '{1}'",
                               base_podcast_remove_query,
                               pi.ID
                           );

            Globals.Library.Db.Execute(query);
        }

        public static void Delete (ICollection podcasts)
        {
            QueryOnID (base_podcast_remove_query, podcasts);
        }

        private static readonly string base_podcast_deactivate_query =
            @"Update Podcasts SET Active=0 WHERE";

        public static void Deactivate (PodcastInfo pi)
        {
            if (pi == null)
            {
                throw new ArgumentNullException ("pi");
            }

            string query = String.Format(
                               @"{0} PodcastID = '{1}'",
                               base_podcast_deactivate_query,
                               pi.ID
                           );

            Globals.Library.Db.Execute(query);
        }

        public static void Deactivate (ICollection podcasts)
        {
            QueryOnID (base_podcast_deactivate_query, podcasts);
        }

        private static void QueryOnID (string base_query, ICollection podcasts)
        {
            if (podcasts == null)
            {
                throw new ArgumentNullException ("podcasts");
            }

            if (podcasts.Count == 0)
            {
                return;
            }

            StringBuilder query_builder = new StringBuilder (base_query);

            bool first = true;

            foreach (PodcastInfo pi in podcasts)
            {
                if (first)
                {
                    query_builder.AppendFormat (@" PodcastID={0}", pi.ID);
                    first = false;
                }
                else
                {
                    query_builder.AppendFormat (@" OR PodcastID={0}", pi.ID);
                }
            }

            Globals.Library.Db.Execute(query_builder.ToString ());
        }
    }
}
