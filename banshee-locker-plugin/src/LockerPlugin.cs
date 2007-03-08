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
using Mono.Unix;

using GConf;

using Banshee.Base;
using Banshee.Sources;

namespace Banshee.Plugins.Locker
{
    public class LockerPlugin : Banshee.Plugins.Plugin
    {
        private Source source;
        private GConf.Client gconf;
    
        protected override string ConfigurationName { get { return "Locker"; } }

        public override string DisplayName { get { return "Music Locker"; } }
        
        public override string Description {
            get {
                return Catalog.GetString(
                    "Enables browsing and listening to songs from your MP3tunes locker."
                );
            }
        }
        
        public override string [] Authors {
            get {
                return new string [] { 
                    "MP3tunes, LLC"
                };
            }
        }

        protected override void PluginInitialize()
        {
            RegisterConfigurationKey("Username");
            RegisterConfigurationKey("Password");

            gconf = Globals.Configuration;
            gconf.AddNotify(ConfigurationBase, GConfNotifier);

            source = new LockerSource(this);
            SourceManager.AddSource(this.source);
        }
        
        protected override void PluginDispose()
        {
            SourceManager.RemoveSource(this.source);
            gconf.RemoveNotify(ConfigurationBase, GConfNotifier);
        }
                
        public override Gtk.Widget GetConfigurationWidget()
        {
            return new LockerConfigPage(this); 
        }

        private void GConfNotifier(object sender, NotifyEventArgs args)
        {

        }

        internal void CreateAccount()
        {
            Gnome.Url.Show("https://shop.mp3tunes.com/registration.php");
        }
        
        internal string Username {
            get {
                return GetStringPref(ConfigurationKeys["Username"], String.Empty);
            }

            set {
                gconf.Set(ConfigurationKeys["Username"], value);
            }
        }

        internal string Password {
            get {
                return GetStringPref(ConfigurationKeys["Password"], String.Empty);
            }

            set {
                gconf.Set(ConfigurationKeys["Password"], value);
            }
        }

        private string GetStringPref(string key, string def)
        {
            try {
                return (string)gconf.Get(key);
            } catch {
                return def;
            }
        }
    }
}
