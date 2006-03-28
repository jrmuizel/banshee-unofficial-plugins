using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Web;

using Gtk;
using Mono.Unix;

using Banshee.Base;

namespace Banshee.Plugins.Recommendation
{
	public class RecommendationPane : Frame
	{
		private Box main_box, similar_box, tracks_box, albums_box;
		private Box tracks_items_box, albums_items_box;
		private Table similar_items_table;

		private string current_artist;

		private const string AUDIOSCROBBLER_SIMILAR_URL = "http://ws.audioscrobbler.com/1.0/artist/{0}/similar.xml";
		private const string AUDIOSCROBBLER_TOP_TRACKS_URL = "http://ws.audioscrobbler.com/1.0/artist/{0}/toptracks.xml";
		private const string AUDIOSCROBBLER_TOP_ALBUMS_URL = "http://ws.audioscrobbler.com/1.0/artist/{0}/topalbums.xml";

		private string LOCAL_IMAGE_CACHE = System.IO.Path.Combine (Paths.UserPluginDirectory, "recommendation-images");

		// FIXME: Dynamically extend the table depending on the width of the window
		private const int SIMILAR_TABLE_COLS = 5;
		private const int SIMILAR_TABLE_ROWS = 2;

		public RecommendationPane () {
			Visible = false;

			EventBox event_box = new EventBox ();
			event_box.ModifyBg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));
			
			main_box = new HBox ();
			main_box.BorderWidth = 5;

			similar_box = new VBox ();
			tracks_box = new VBox ();
			albums_box = new VBox ();

			Label similar_header = new Label ();
			similar_header.Xalign = 0;
			similar_header.Markup = String.Format ("<b>{0}</b>", Catalog.GetString ("Similar Artists"));
			similar_box.PackStart (similar_header);

			Label tracks_header = new Label ();
			tracks_header.Xalign = 0;
			tracks_header.Markup = String.Format ("<b>{0}</b>", Catalog.GetString ("Top Tracks by Artist"));
			tracks_box.PackStart (tracks_header);

			Label albums_header = new Label ();
			albums_header.Xalign = 0;
			albums_header.Markup = String.Format ("<b>{0}</b>", Catalog.GetString ("Top Albums by Artist"));
			albums_box.PackStart (albums_header);

			similar_items_table = new Table (3, 3, false);
			similar_box.PackEnd (similar_items_table, true, true, 0);

			tracks_items_box = new VBox ();
			tracks_box.PackEnd (tracks_items_box, true, true, 0);

			albums_items_box = new VBox ();
			albums_box.PackEnd (albums_items_box, true, true, 0);

			main_box.PackStart (similar_box, true, true, 5);
			main_box.PackStart (tracks_box, false, false, 5);
			main_box.PackStart (albums_box, false, false, 5);

			event_box.Add (main_box);
			Add (event_box);
		}

		// --------------------------------------------------------------- //

		public void HideRecommendations ()
		{
			Visible = false;
		}

		public void ShowRecommendations (string artist)
		{
			//FIXME: We might wanna do some async foo here.
			ThreadAssist.Spawn(delegate {
				lock (this) {
					if (current_artist == artist) {
						ThreadAssist.ProxyToMain (delegate {
								Visible = true;
								ShowAll ();
							});
						return;
					}

					current_artist = artist;
					
					ThreadAssist.ProxyToMain (delegate {
						// Wipe the old recommendations here, we keep them around in case 
						// where the the artist is the same as the last song.
						foreach (Widget child in similar_items_table.Children)
							similar_items_table.Remove (child);
						foreach (Widget child in tracks_items_box.Children)
							tracks_items_box.Remove (child);
						foreach (Widget child in albums_items_box.Children)
							albums_items_box.Remove (child);
						});

					// Fetch data for "similar" artists.
					XmlDocument similar_data = new XmlDocument ();
					
					try {
						similar_data.LoadXml (RequestContent (String.Format (AUDIOSCROBBLER_SIMILAR_URL, artist)));
					} catch (Exception ex) {
						Console.WriteLine ("snopp");
						return;
					}
					
					XmlNodeList similar_artists = similar_data.SelectNodes ("/similarartists/artist");
					for (int i = 0; i < similar_artists.Count && i < (SIMILAR_TABLE_COLS * SIMILAR_TABLE_ROWS); i++) {
						Widget artist_widget = RenderSimilarArtist (similar_artists[i]);
						
						similar_items_table.Attach (artist_widget, 
									    (uint) i % SIMILAR_TABLE_COLS, (uint) (i % SIMILAR_TABLE_COLS) + 1, 
									    (uint) i / SIMILAR_TABLE_COLS, (uint) (i / SIMILAR_TABLE_COLS) + 1);
					}
					
					
					// Fetch data for top tracks
					XmlDocument tracks_data = new XmlDocument ();
					try {
						tracks_data.LoadXml (RequestContent (String.Format (AUDIOSCROBBLER_TOP_TRACKS_URL, artist)));
					} catch (Exception ex) {
						Console.WriteLine ("snopp");
						return;
					}
					
					XmlNodeList tracks = tracks_data.SelectNodes ("/mostknowntracks/track");					
					for (int i = 0; i < tracks.Count && i < 5; i++) {
						Widget track_widget = RenderTrack (tracks [i]);
						tracks_items_box.Add (track_widget);
					}
					
					// Fetch data for top albums
					XmlDocument albums_data = new XmlDocument ();
					try {
						albums_data.LoadXml (RequestContent (String.Format (AUDIOSCROBBLER_TOP_ALBUMS_URL, artist)));
					} catch (Exception ex) {
						Console.WriteLine ("snopp");
						return;
					}
					
					XmlNodeList albums = albums_data.SelectNodes ("/topalbums/album");
					for (int i = 0; i < albums.Count && i < 5; i++) {
						Widget album_widget = RenderAlbum (albums [i]);
						albums_items_box.Add (album_widget);
					}
					
					// Display the recommendation pane only if we have something to recommend.
					if ((similar_items_table.Children.Length + 
					     tracks_items_box.Children.Length + 
					     albums_items_box.Children.Length) > 0) {
						
						ThreadAssist.ProxyToMain (delegate {
								Visible = true;
								ShowAll ();
							});
					}
				}
			});
		}
		
		// --------------------------------------------------------------- //
		
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

			// FIXME: Escape stuff
			label.Markup = String.Format ("{0}\n<small><span foreground=\"grey\">{1}% {2}</span></small>", 
						      node.SelectSingleNode ("name").InnerText,
						      node.SelectSingleNode ("match").InnerText,
						      Catalog.GetString ("similarity"));
			box.PackEnd (label, true, true, 3);
			
			artist_button.Add (box);

			artist_button.Clicked += delegate(object o, EventArgs args) {
				Gnome.Url.Show (node.SelectSingleNode ("url").InnerText);
			};

			return artist_button;
		}

		private Widget RenderTrack (XmlNode node)
		{
			Button track_button = new Button ();
			track_button.Relief = ReliefStyle.None;

			Label label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.Xalign = 0;
			label.Markup = node.SelectSingleNode ("name").InnerText;
			track_button.Add (label);

			track_button.Clicked += delegate(object o, EventArgs args) {
				Gnome.Url.Show (node.SelectSingleNode ("url").InnerText);
			};

			return track_button;
		}

		// FIXME: Image?
		private Widget RenderAlbum (XmlNode node)
		{
			Button album_button = new Button ();
			album_button.Relief = ReliefStyle.None;

			Label label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.Xalign = 0;
			label.Markup = node.SelectSingleNode ("name").InnerText;
			album_button.Add (label);

			album_button.Clicked += delegate(object o, EventArgs args) {
				Gnome.Url.Show (node.SelectSingleNode ("url").InnerText);
			};

			return album_button;
		}

		// --------------------------------------------------------------- //

		private Image RenderImage (string url)
		{
			// FIXME: Hash and path
			string path = System.IO.Path.Combine (LOCAL_IMAGE_CACHE, Math.Abs (url.GetHashCode ()).ToString ());
			
			if (! File.Exists (path))
				RequestContent (url, path);

			return new Image (path);
		}

		private string RequestContent (string url)
		{
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
			request.KeepAlive = false;
			
			HttpWebResponse response = (HttpWebResponse) request.GetResponse();
			
			Stream stream = response.GetResponseStream ();
			StreamReader reader = new StreamReader (stream);
			string content = reader.ReadToEnd();
			
			response.Close();
			
			return content;
		}

		private void RequestContent (string url, string path)
		{
			if (!Directory.Exists (LOCAL_IMAGE_CACHE))
				Directory.CreateDirectory (LOCAL_IMAGE_CACHE);
				
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
	}
}