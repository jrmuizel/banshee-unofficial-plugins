/*
 * Copyright (C) 2005-2006 MP3tunes, LLC
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using Gtk;
using GConf;
using Mono.Unix;

using Banshee.Base;
using Banshee.Widgets;

namespace Banshee.Plugins.Locker
{
    public class LockerConfigPage : VBox
    {
        private LockerPlugin plugin;
        private PropertyTable table;
        private Entry user_entry;
        private Entry pass_entry;
        private Image logo;

        public LockerConfigPage(LockerPlugin plugin) :  base()
        {
            this.plugin = plugin;
            BuildWidget();
        }
        
        private void BuildWidget()
        {
            Spacing = 10;
            
            Label title = new Label();
            title.Markup = String.Format("<big><b>{0}</b></big>", 
                GLib.Markup.EscapeText(Catalog.GetString("Music Locker")));
            title.Xalign = 0.0f;
            
            Label label = new Label(Catalog.GetString("Enter your MP3tunes account information"));
            label.Xalign = 0.0f;
                        
            Alignment account_alignment = new Alignment(0.0f, 0.0f, 1.0f, 1.0f);
            account_alignment.BorderWidth = 5;
            account_alignment.LeftPadding = 10;
            account_alignment.RightPadding = 10;
            account_alignment.BottomPadding = 10;
            
            table = new PropertyTable();
            table.RowSpacing = 5;
            table.ColumnSpacing = 5;
            
            user_entry = table.AddEntry(Catalog.GetString("Username"), "", false);
            pass_entry = table.AddEntry(Catalog.GetString("Password"), "", false);
            pass_entry.Visibility = false;
            user_entry.Text = plugin.Username;
            pass_entry.Text = plugin.Password;
            user_entry.Changed += OnUserPassChanged;
            pass_entry.Changed += OnUserPassChanged;
            
            account_alignment.Add(table);
            
            HBox logo_box = new HBox();
            logo_box.Spacing = 20;
            
            logo = new Image();
            logo.Pixbuf = Gdk.Pixbuf.LoadFromResource("locker-logo.png");
            logo.Xalign = 0.0f;
            
            VBox button_box = new VBox();
            Button create_account_button = new Button(Catalog.GetString("Create an account..."));
            create_account_button.Clicked += delegate(object o, EventArgs args) {
                plugin.CreateAccount();
            };
            button_box.PackStart(create_account_button, true, false, 0);

            logo_box.PackStart(logo, false, false, 0);
            logo_box.PackStart(button_box, true, true, 0);

            PackStart(title, false, false, 0);
            PackStart(label, false, false, 0);
            PackStart(account_alignment, false, false, 0);
            PackStart(logo_box, false, false, 0);
            
            ShowAll();
        }
        
        internal Image Logo {
            get {
                return logo;
            }
        }
        
        private void OnUserPassChanged(object o, EventArgs args)
        {
            plugin.Username = user_entry.Text;
            plugin.Password = pass_entry.Text;
        }
    }
}
