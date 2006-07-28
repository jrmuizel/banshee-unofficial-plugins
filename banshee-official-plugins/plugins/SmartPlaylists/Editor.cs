using System;
using System.Collections;
using Gtk;
using Glade;
 
using Banshee.Base;
using Banshee.Sources;
using Banshee.Plugins;
using Banshee.Widgets;

namespace Banshee.Plugins.SmartPlaylists
{
    public class Editor
    {
        private const string dialog_name = "SmartPlaylistDialog";
        private Glade.XML glade;
        private Dialog dialog;

		private Banshee.QueryBuilder builder;
		private Banshee.TracksQueryModel model;

        private SmartPlaylist playlist = null;

        [Widget] private Gtk.Entry name_entry;
        [Widget] private Gtk.VBox builder_box;
        [Widget] private Gtk.Button ok_button;
        [Widget] private Gtk.Label already_in_use_label;

        public Editor (SmartPlaylist playlist) : this ()
        {
            this.playlist = playlist;

            dialog.Title = Catalog.GetString ("Edit Smart Playlist");

            name_entry.Text = playlist.Name;
            Condition = playlist.Condition;
            OrderBy = playlist.OrderBy;
            LimitNumber = playlist.LimitNumber;
            LimitCriterion = playlist.LimitCriterion;
        }
	
        public Editor ()
        {
            glade = new Glade.XML(null, "smart-playlist.glade", dialog_name, "banshee");
            glade.Autoconnect(this);

            dialog = (Dialog) glade.GetWidget(dialog_name);
            Banshee.Base.IconThemeUtils.SetWindowIcon(dialog);

            dialog.Title = Catalog.GetString ("New Smart Playlist");

            // Add the QueryBuilder widget
			model = new TracksQueryModel();
			builder = new QueryBuilder(model);
			builder.Show();
			builder.Spacing = 4;

			builder_box.PackStart(builder, true, true, 0);

            name_entry.Changed += HandleNameChanged;

            Update();
        }

        public void SetQueryFromSearch()
        {
            Banshee.Widgets.SearchEntry search_entry = InterfaceElements.SearchEntry;

            string field = search_entry.Field;
            string query = search_entry.Query;

            string condition = String.Empty;
            ArrayList condition_candidates = new ArrayList ();

            QueryFilter FilterContains = QueryFilter.Contains;
            QueryFilter FilterIs       = QueryFilter.Is;

            condition_candidates.Add (FilterContains.Operator.FormatValues (true, "Artist", query, null) );
            condition_candidates.Add (FilterContains.Operator.FormatValues (true, "Title", query, null) );
            condition_candidates.Add (FilterContains.Operator.FormatValues (true, "AlbumTitle", query, null) );
            condition_candidates.Add (FilterContains.Operator.FormatValues (true, "Genre", query, null) );

            // only search for years if the query is a number
            try {
                int.Parse(query);
                condition_candidates.Add (FilterIs.Operator.FormatValues (false, "Year", query, null) );
            }
            catch {
                //Console.WriteLine ("{0} is not a valid year", query);
                condition_candidates.Add (String.Empty);
            }

            if(field == Catalog.GetString("Artist Name")) {
                condition = " (" + condition_candidates[0].ToString() + ") ";
            } else if(field == Catalog.GetString("Song Name")) {
                condition = " (" + condition_candidates[1].ToString() + ") ";
            } else if(field == Catalog.GetString("Album Title")) {
                condition = " (" + condition_candidates[2].ToString() + ") ";
            } else if(field == Catalog.GetString("Genre")) {
                condition = " (" + condition_candidates[3].ToString() + ") ";
            } else if(field == Catalog.GetString("Year")) {
                condition = " (" + condition_candidates[4].ToString() + ") ";
            } else {
                // Searching for all possible conditions
                for(int i = 0; i < condition_candidates.Count; i++) {
                    string c = condition_candidates[i].ToString();
                    if (c.Length > 0) {
                        if (i > 0)
                            condition += "OR";
                        
                        condition += " (" + c  + ") ";
                    }
                }
            }

            Condition = condition;

            dialog.Title = Catalog.GetString ("Create Smart Playlist from Search");
            name_entry.Text = field + ": " + query;
        }

        public void RunDialog()
        {
            dialog.ShowAll();
            builder.MatchesBox.FirstRow.FieldBox.GrabFocus();

			ResponseType response = (ResponseType) dialog.Run ();

            //int w = -1, h = -1;
		    //dialog.GetSize (out w, out h);
            //Console.WriteLine ("w = {0}, h = {1}", w, h);

			if (response == ResponseType.Ok) {
                string name = Name;
                string condition = Condition;
                string order_by = OrderBy;
                string limit_number = LimitNumber;
                int limit_criterion = LimitCriterion;

                ThreadAssist.Spawn (delegate {
                    //Console.WriteLine ("Name = {0}, Cond = {1}, OrderAndLimit = {2}", name, condition, order_by, limit_number);
                    if (playlist == null) {
                        Timer t = new Timer ("Create/Add new Playlist");
                        playlist = new SmartPlaylist(name, condition, order_by, limit_number, limit_criterion);
                        LibrarySource.Instance.AddChildSource(playlist);
                        Plugin.Instance.StartTimer(playlist);
                        t.Stop();
                    } else {
                        playlist.Rename(name);
                        playlist.Condition = condition;
                        playlist.OrderBy = order_by;
                        playlist.LimitNumber = limit_number;
                        playlist.LimitCriterion = limit_criterion;
                        playlist.Commit();
                        playlist.RefreshMembers();

                        if (playlist.TimeDependent)
                            Plugin.Instance.StartTimer(playlist);
                        else
                            Plugin.Instance.StopTimer();
                    }
                });
            }

            dialog.Destroy();
        }

        private void HandleNameChanged(object sender, EventArgs args)
        {
            Update ();
        }

        private void Update()
        {
			if (name_entry.Text == "") {
				ok_button.Sensitive = false;
				already_in_use_label.Markup = "";
			} else {
                object res = Globals.Library.Db.QuerySingle(String.Format(
                    "SELECT PlaylistID FROM Playlists WHERE lower(Name) = lower('{0}')",
                    Sql.Statement.EscapeQuotes(name_entry.Text)
                ));

                if (res != null && (playlist == null || String.Compare (playlist.Name, name_entry.Text, true) != 0)) {
                    ok_button.Sensitive = false;
                    already_in_use_label.Markup = "<small>" + Catalog.GetString ("This name is already in use") + "</small>";
                } else {
                    ok_button.Sensitive = true;
                    already_in_use_label.Markup = "";
                }
            }
        }

		private string Name {
            get {
                return name_entry.Text;
            }
        }

		private string Condition {
            get {
                return builder.MatchesEnabled
                    ? builder.MatchQuery
                    : null;
            }

            set {
                builder.MatchesEnabled = (value != null);
                builder.MatchQuery = value;
            }
        }

        private string OrderBy {
            get {
                return (builder.Limit && builder.LimitNumber != "0")
                    ? builder.OrderBy
                    : null;
            }

            set {
                builder.Limit = (value != null);
                builder.OrderBy = value;
            }
        }

        private string LimitNumber {
            get {
                return (builder.Limit)
                    ? builder.LimitNumber
                    : "0";
            }
            
            set {
                if (value != null && value != "" && value != "0") {
                    builder.Limit = true;
                    builder.LimitNumber = value;
                }
            }
        }

        private int LimitCriterion {
            get {
                return builder.LimitCriterion;
            }
            
            set {
                builder.LimitCriterion = value;
            }
        }
    }
}
