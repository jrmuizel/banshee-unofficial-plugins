using System;
using System.Collections;
using System.Collections.Specialized;

using Gtk;
using Mono.Unix;

using Banshee.Base;
using Banshee.Widgets;

namespace Banshee.Plugins.MusicStore
{
	public class MusicStoreConfigPage : VBox
	{
		private MusicStorePlugin plugin;
		private PropertyTable table;
		private Entry user_entry;
		private Entry pass_entry;
		private ComboBox country_combo;
		
		public MusicStoreConfigPage(MusicStorePlugin plugin) :  base()
		{
			this.plugin = plugin;
			BuildWidget();
		}
		
		private void BuildWidget()
		{
			Spacing = 10;
			
			Label title = new Label();
			title.Markup = String.Format("<big><b>{0}</b></big>", 
						     GLib.Markup.EscapeText(Catalog.GetString("iTunes Music Store")));
			title.Xalign = 0.0f;
			
			Label label = new Label(plugin.Description);
			label.Wrap = true;
			
			Alignment alignment = new Alignment(0.0f, 0.0f, 1.0f, 1.0f);
			alignment.LeftPadding = 10;
			alignment.RightPadding = 10;
			
			Frame frame = new Frame();
			Alignment frame_alignment = new Alignment(0.0f, 0.0f, 1.0f, 1.0f);
			frame_alignment.BorderWidth = 5;
			frame_alignment.LeftPadding = 10;
			frame_alignment.RightPadding = 10;
			frame_alignment.BottomPadding = 10;
			
			frame.LabelWidget = new Label (Catalog.GetString ("Account Information"));
			frame.ShadowType = ShadowType.EtchedIn;
			
			table = new PropertyTable();
			table.RowSpacing = 5;
			
			user_entry = table.AddEntry(Catalog.GetString("Username"), "", false);
			pass_entry = table.AddEntry(Catalog.GetString("Password"), "", false);
			pass_entry.Visibility = false;
			
			user_entry.Text = plugin.Username;
			pass_entry.Text = plugin.Password;
			user_entry.Changed += OnUserPassChanged;
			pass_entry.Changed += OnUserPassChanged;
			
			country_combo = ComboBox.NewText ();
			country_combo.Changed += OnCountryChanged; 

			TreeIter loaded_iter = TreeIter.Zero;
			foreach (string country in FairStore.Countries.AllKeys) {
				TreeIter iter = ((ListStore)country_combo.Model).AppendValues (country);
				if (country == plugin.Country)
					loaded_iter = iter;
			}
			country_combo.SetActiveIter (loaded_iter);

			table.AddWidget (Catalog.GetString ("Country"), country_combo, false);
			
			frame_alignment.Add(table);
			frame.Add(frame_alignment);
			
			Label copyright = new Label (Catalog.GetString ("Apple and iTunes Music Store are registered trademarks of Apple Computer, Inc."));
			
			PackStart(title, false, false, 0);
			PackStart(label, false, false, 0);
			PackStart(alignment, false, false, 0);
			PackStart(frame, true, false, 0);
			PackStart(copyright, false, false, 0);
			
			ShowAll();
		}
		
		private void OnUserPassChanged(object o, EventArgs args)
		{
			plugin.Username = user_entry.Text;
			plugin.Password = pass_entry.Text;
		}

		private void OnCountryChanged (object o, EventArgs args)
		{
			TreeIter iter;
			if (country_combo.GetActiveIter (out iter))
				plugin.Country = (string) country_combo.Model.GetValue (iter, 0);
		}
	}
}
