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

			if (response == ResponseType.Ok) {
                ThreadAssist.Spawn (delegate {
                    if (playlist == null) {
                        playlist = new SmartPlaylist(Name, Condition, OrderAndLimit);
                        playlist.Source.Commit();
                        SourceManager.AddSource(playlist.Source);
                    } else {
                        playlist.Name = Name;
                        playlist.Condition = Condition;
                        playlist.OrderAndLimit = OrderAndLimit;
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
        }

        private string OrderAndLimit {
            get {
                return (builder.Limit && builder.LimitNumber > 0)
                    ? "ORDER BY " + builder.OrderBy + " LIMIT " + builder.LimitNumber
                    : null;
            }
        }
    }
}
