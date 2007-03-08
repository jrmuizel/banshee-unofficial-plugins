using System;
using Mono.Unix;
using Gtk;
using Glade;
 
using Banshee.Base;
using Banshee.Sources;
 
namespace Banshee.Plugins.SmartPlaylists
{
    public class SmartPlaylistEditor
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

        public SmartPlaylistEditor (SmartPlaylist playlist) : this ()
        {
            this.playlist = playlist;

            name_entry.Text = playlist.Name;
            Condition = playlist.Condition;
            OrderBy = playlist.OrderBy;
            LimitNumber = playlist.LimitNumber;
        }
	
        public SmartPlaylistEditor ()
        {
            glade = new Glade.XML(null, "smart-playlist.glade", dialog_name, "banshee");
            glade.Autoconnect(this);

            dialog = (Dialog)glade.GetWidget(dialog_name);
            Banshee.Base.IconThemeUtils.SetWindowIcon(dialog);

            // Add the QueryBuilder widget
			model = new TracksQueryModel();
			builder = new QueryBuilder(model);
			builder.Show();
			builder.Spacing = 4;

			builder_box.PackStart(builder, true, true, 0);

            name_entry.Changed += HandleNameChanged;

            Update();
        }

        public void RunDialog()
        {
			ResponseType response = (ResponseType) dialog.Run ();

            //int w = -1, h = -1;
		    //dialog.GetSize (out w, out h);
            //Console.WriteLine ("w = {0}, h = {1}", w, h);

			if (response == ResponseType.Ok) {
                string name = Name;
                string condition = Condition;
                string order_by = OrderBy;
                string limit_number = LimitNumber;

                ThreadAssist.Spawn (delegate {
                    //Console.WriteLine ("Name = {0}, Cond = {1}, OrderAndLimit = {2}", name, condition, order_by, limit_number);
                    if (playlist == null) {
                        playlist = new SmartPlaylist(name, condition, order_by, limit_number);
                        playlist.Source.Commit();
                        SourceManager.AddSource(playlist.Source);
                    } else {
                        playlist.Name = name;
                        playlist.Condition = condition;
                        playlist.OrderBy = order_by;
                        playlist.LimitNumber = limit_number;
                        playlist.Commit();
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
                    already_in_use_label.Markup = "<small>" + Mono.Unix.Catalog.GetString ("This name is already in use") + "</small>";
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
                return (builder.Limit && builder.LimitNumber != "0")
                    ? builder.LimitNumber
                    : "0";
            }
            
            set {
                builder.LimitNumber = value;
            }
        }
    }
}
