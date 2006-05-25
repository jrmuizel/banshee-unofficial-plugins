using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Web;
using System.Threading;
using System.Collections;

using Gtk;

using Banshee.Base;

namespace Banshee.Plugins.Recommendation
{
	public class RecommendationPane : Frame
	{
		private Box main_box, similar_box, tracks_box, albums_box;
		private Box tracks_items_box, albums_items_box;
		private Table similar_items_table;
		private Label tracks_header, albums_header;
		private ArrayList artists_widgets_list = new ArrayList ();
		
		private const string AUDIOSCROBBLER_SIMILAR_URL = "http://ws.audioscrobbler.com/1.0/artist/{0}/similar.xml";
		private const string AUDIOSCROBBLER_TOP_TRACKS_URL = "http://ws.audioscrobbler.com/1.0/artist/{0}/toptracks.xml";
		private const string AUDIOSCROBBLER_TOP_ALBUMS_URL = "http://ws.audioscrobbler.com/1.0/artist/{0}/topalbums.xml";

		private const uint DEFAULT_SIMILAR_TABLE_ROWS = 2;
		private const uint DEFAULT_SIMILAR_TABLE_COLS = 5;

		private const int NUM_MAX_ARTISTS = 16;
		private const int NUM_TRACKS = 5;
		private const int NUM_ALBUMS = 5;

		private static string CACHE_PATH = System.IO.Path.Combine (Paths.UserPluginDirectory, "recommendation");
		private static TimeSpan CACHE_TIME = TimeSpan.FromHours (2);

		private string current_artist;
		public string CurrentArtist {
			get { return current_artist; }
		}

		public RecommendationPane () {
			Visible = false;

			EventBox event_box = new EventBox ();
			event_box.ModifyBg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));
			
			main_box = new HBox ();
			main_box.BorderWidth = 5;

			similar_box = new VBox (false, 3);
			tracks_box = new VBox (false, 3);
			albums_box = new VBox (false, 3);

			Label similar_header = new Label ();
			similar_header.Xalign = 0;
			similar_header.Ellipsize = Pango.EllipsizeMode.End;
			similar_header.Markup = String.Format ("<b>{0}</b>", Catalog.GetString ("Recommended Artists"));
			similar_box.PackStart (similar_header, false, false, 0);

			tracks_header = new Label ();
			tracks_header.Xalign = 0;
			tracks_header.WidthChars = 25;
			tracks_header.Ellipsize = Pango.EllipsizeMode.End;
			tracks_box.PackStart (tracks_header, false, false, 0);

			albums_header = new Label ();
			albums_header.Xalign = 0;
			albums_header.WidthChars = 25;
			albums_header.Ellipsize = Pango.EllipsizeMode.End;
			albums_box.PackStart (albums_header, false, false, 0);

			similar_items_table = new Table (DEFAULT_SIMILAR_TABLE_ROWS, DEFAULT_SIMILAR_TABLE_COLS, false);
			similar_items_table.SizeAllocated += OnSizeAllocated;
			similar_box.PackEnd (similar_items_table, true, true, 0);

			tracks_items_box = new VBox (false, 0);
			tracks_box.PackEnd (tracks_items_box, true, true, 0);

			albums_items_box = new VBox (false, 0);
			albums_box.PackEnd (albums_items_box, true, true, 0);

			main_box.PackStart (similar_box, true, true, 5);
			main_box.PackStart (new VSeparator (), false, false, 0);
			main_box.PackStart (tracks_box, false, false, 5);
			main_box.PackStart (new VSeparator (), false, false, 0);
			main_box.PackStart (albums_box, false, false, 5);

			event_box.Add (main_box);
			Add (event_box);

			if (!Directory.Exists (CACHE_PATH))
				Directory.CreateDirectory (CACHE_PATH);
		}
		
		private void OnSizeAllocated (object o, SizeAllocatedArgs args)
		{
			// FIXME: Do we really need to do resizing this way? It blows.
			Gdk.Rectangle rect = args.Allocation;

			uint requested_columns = (uint) rect.Width/150;
			if (requested_columns == 0) requested_columns = 1;

			if (similar_items_table.NColumns != requested_columns) {
				// Need to clear the table before we resize it, apparently.
				foreach (Widget child in similar_items_table.Children)
					similar_items_table.Remove (child);
			
				similar_items_table.Resize (DEFAULT_SIMILAR_TABLE_ROWS, requested_columns);
				RenderSimilarArtists ();
			}
		}

		// --------------------------------------------------------------- //

		public void HideRecommendations ()
		{
			Visible = false;
		}

		public void ShowRecommendations (string artist)
		{
			if (current_artist == artist) {
				Visible = true;
				return;
			}

			// FIXME: Error handling	
			ThreadAssist.Spawn(delegate {

		                // Last.fm requires double-encoding of '/' characters, see
                		// http://bugzilla.gnome.org/show_bug.cgi?id=340511
		                string encoded_artist = artist.Replace ("/", "%2F");
                		encoded_artist = System.Web.HttpUtility.UrlEncode (encoded_artist);

				// Fetch data for "similar" artists.
				XmlDocument artists_xml_data = new XmlDocument ();
				artists_xml_data.LoadXml (RequestContent (String.Format (AUDIOSCROBBLER_SIMILAR_URL, encoded_artist)));
				XmlNodeList artists_xml_list = artists_xml_data.SelectNodes ("/similarartists/artist");

				// Cache artists images (here in the spawned thread)
				for (int i = 0; i < artists_xml_list.Count && i < NUM_MAX_ARTISTS; i++) {
					string url = artists_xml_list [i].SelectSingleNode ("image_small").InnerText;					
					DownloadContent (url, GetCachedPathFromUrl (url), true);
				}
					
				// Fetch data for top tracks
				XmlDocument tracks_xml_data = new XmlDocument ();
				tracks_xml_data.LoadXml (RequestContent (String.Format (AUDIOSCROBBLER_TOP_TRACKS_URL, encoded_artist)));
				XmlNodeList tracks_xml_list = tracks_xml_data.SelectNodes ("/mostknowntracks/track");					
				
				// Try to match top tracks with the users's library
				for (int i = 0; i < tracks_xml_list.Count && i < NUM_TRACKS; i++) {
					string track_name = tracks_xml_list [i].SelectSingleNode ("name").InnerText;
				        int track_id = GetTrackId (artist, track_name);
					
					if (track_id == -1)
						continue;
					
					XmlNode track_id_node = tracks_xml_list [i].OwnerDocument.CreateNode (XmlNodeType.Element, "track_id", null);
					track_id_node.InnerText = track_id.ToString ();

					tracks_xml_list [i].AppendChild (track_id_node);
				}
				
				// Fetch data for top albums
				XmlDocument albums_xml_data = new XmlDocument ();
				albums_xml_data.LoadXml (RequestContent (String.Format (AUDIOSCROBBLER_TOP_ALBUMS_URL, encoded_artist)));
				XmlNodeList albums_xml_list = albums_xml_data.SelectNodes ("/topalbums/album");
				
				if (artists_xml_list.Count < 1 && tracks_xml_list.Count < 1 && albums_xml_list.Count < 1)
					return;
				
				if (artist != PlayerEngineCore.CurrentTrack.Artist)
					return;

				ThreadAssist.ProxyToMain (delegate {
					// Wipe the old recommendations here, we keep them around in case 
					// where the the artist is the same as the last song.
					foreach (Widget child in similar_items_table.Children)
						similar_items_table.Remove (child);
					foreach (Widget child in tracks_items_box.Children)
						tracks_items_box.Remove (child);
					foreach (Widget child in albums_items_box.Children)
						albums_items_box.Remove (child);

					// Display recommendations and artist information
					current_artist = artist;
					tracks_header.Markup = String.Format ("<b>{0} {1}</b>", Catalog.GetString ("Top Tracks by"), artist);
					albums_header.Markup = String.Format ("<b>{0} {1}</b>", Catalog.GetString ("Top Albums by"), artist);
					
					artists_widgets_list.Clear ();
					if (artists_xml_list != null) {	
						for (int i = 0; i < artists_xml_list.Count && i < NUM_MAX_ARTISTS; i++)
							artists_widgets_list.Add (RenderSimilarArtist (artists_xml_list [i]));

						RenderSimilarArtists ();
					}

					if (tracks_xml_list != null) {
						for (int i = 0; i < tracks_xml_list.Count && i < NUM_TRACKS; i++)
							tracks_items_box.PackStart (RenderTrack (tracks_xml_list [i], i + 1), false, true, 0);
					}
					
					if (albums_xml_list != null) {
						for (int i = 0; i < albums_xml_list.Count && i < NUM_ALBUMS; i++)
							albums_items_box.PackStart (RenderAlbum (albums_xml_list [i], i + 1), false, true, 0);
					}
					
					Visible = true;
					ShowAll ();
				});
		        });
		}
		
		// --------------------------------------------------------------- //

		private void RenderSimilarArtists ()
		{
			for (int i = 0; i < artists_widgets_list.Count && i < (similar_items_table.NColumns * similar_items_table.NRows); i++) {
				similar_items_table.Attach ((Widget) artists_widgets_list [i],
							    (uint) i / similar_items_table.NRows, 
							    (uint) (i / similar_items_table.NRows) + 1,
							    (uint) i % similar_items_table.NRows,
							    (uint) (i % similar_items_table.NRows) + 1);
			}

			similar_items_table.ShowAll ();
		}
				
		private Widget RenderSimilarArtist (XmlNode node)
		{
			Button artist_button = new Button ();
			artist_button.Relief = ReliefStyle.None;

			HBox box = new HBox ();
			Viewport vp = new Viewport ();
			vp.Add (RenderImage (node.SelectSingleNode ("image_small").InnerText));
			box.PackStart (vp, false, false, 0);

			Label label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.Xalign = 0;

			label.Markup = String.Format ("{0}\n<small><span foreground=\"grey\">{1}% {2}</span></small>", 
						      GLib.Markup.EscapeText (node.SelectSingleNode ("name").InnerText).Trim (),
						      node.SelectSingleNode ("match").InnerText,
						      Catalog.GetString ("similarity"));
			box.PackEnd (label, true, true, 3);
			
			artist_button.Add (box);

			artist_button.Clicked += delegate(object o, EventArgs args) {
				Gnome.Url.Show (node.SelectSingleNode ("url").InnerText);
			};

			return artist_button;
		}

		private Widget RenderTrack (XmlNode node, int rank)
		{
			Button track_button = new Button ();
			track_button.Relief = ReliefStyle.None;

			HBox box = new HBox ();

			Label label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.Xalign = 0;
			label.Markup = String.Format ("{0}. {1}", rank, GLib.Markup.EscapeText (node.SelectSingleNode ("name").InnerText).Trim ());

			if (node.SelectSingleNode ("track_id") != null) {
				box.PackEnd (new Image (Gdk.Pixbuf.LoadFromResource("play.png")), false, false, 0);
				track_button.Clicked += delegate(object o, EventArgs args) {
					PlayerEngineCore.OpenPlay (Globals.Library.GetTrack (Convert.ToInt32 (node.SelectSingleNode ("track_id").InnerText)));
				};
			} else {
				track_button.Clicked += delegate(object o, EventArgs args) {
					Gnome.Url.Show (node.SelectSingleNode ("url").InnerText);
				};
			}

			box.PackStart (label, true, true, 0);

			track_button.Add (box);

			return track_button;
		}

		// FIXME: Image?
		private Widget RenderAlbum (XmlNode node, int rank)
		{
			Button album_button = new Button ();
			album_button.Relief = ReliefStyle.None;

			Label label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.Xalign = 0;
			label.Markup = String.Format ("{0}. {1}", rank, GLib.Markup.EscapeText (node.SelectSingleNode ("name").InnerText).Trim ());
			album_button.Add (label);

			album_button.Clicked += delegate(object o, EventArgs args) {
				Gnome.Url.Show (node.SelectSingleNode ("url").InnerText);
			};

			return album_button;
		}

		// --------------------------------------------------------------- //

		private Image RenderImage (string url)
		{
			string path = GetCachedPathFromUrl (url);
			DownloadContent (url, path, true);
			return new Image (path);
		}

		private string RequestContent (string url)
		{
			string path = GetCachedPathFromUrl (url);
			DownloadContent (url, path, false);

			StreamReader reader = new StreamReader (path);
			string content = reader.ReadToEnd();
			
			return content;
		}

		private void DownloadContent (string url, string path, bool static_content)
		{
			if (File.Exists (path)) {
				DateTime last_updated_time = File.GetLastWriteTime (path);
				if (static_content || DateTime.Now - last_updated_time < CACHE_TIME)
					return;
			}
			
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
			request.KeepAlive = false;
			
			HttpWebResponse response = (HttpWebResponse) request.GetResponse();
			
			Stream stream = response.GetResponseStream ();

			FileStream file_stream = File.OpenWrite (path);
                        BufferedStream buffered_stream = new BufferedStream (file_stream);

                        byte [] buffer = new byte [8192];

                        int read;
                        do {
                                read = stream.Read (buffer, 0, buffer.Length);
                                if (read > 0)
                                        buffered_stream.Write (buffer, 0, read);
                        } while (read > 0);

                        buffered_stream.Close ();
			response.Close();			
		}

		private int GetTrackId (string artist, string title)
		{
			string query = String.Format("SELECT TrackId FROM Tracks WHERE Artist LIKE '{0}' AND Title LIKE '{1}' LIMIT 1",
						     Sql.Escape.EscapeQuotes(artist),
						     Sql.Escape.EscapeQuotes(title));

			object result = Globals.Library.Db.QuerySingle (query);
			if (result == null)
				return -1;
			
			return (int) result;
		}

		private string GetCachedPathFromUrl (string url)
		{			
			return System.IO.Path.Combine (CACHE_PATH, Math.Abs (url.GetHashCode ()).ToString ());
		}
	}
}

