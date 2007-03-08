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
using System.Collections;
using Mono.Unix;

using Banshee.Base;
using Banshee.Sources;

using MP3tunes;

namespace Banshee.Plugins.Locker
{
    public class LockerSource : Source
    {
        private MP3tunes.Locker locker;
        private ArrayList tracks;
        private LockerPlugin plugin;
        private bool is_activating;

        public LockerSource(LockerPlugin plugin) : base(plugin.DisplayName, 300)
        {
            this.plugin = plugin;
            tracks = new ArrayList();
            is_activating = false;
        }
        
        public override void Activate()
        {
            if(locker == null && !is_activating)
            {
                is_activating = true;
                ThreadAssist.Spawn(delegate {
                    try
                    {
                        locker = new MP3tunes.Locker( String.Empty );
                        locker.Login( this.plugin.Username, this.plugin.Password );
                        ArrayList trs = locker.GetTracks();
                        IEnumerator en = trs.GetEnumerator();
                        while( en.MoveNext() )
                        {
                            LockerTrackInfo lti = new LockerTrackInfo( (MP3tunes.Track)en.Current );
                            tracks.Add( lti );
                        }
                        OnUpdated();
                    }
                    catch( Exception e )
                    {
						LogCore.Instance.PushError(Catalog.GetString("Could not load Music Locker"), e.Message);
                    }
                    is_activating = false;
                });
            }
        }

        protected override void OnDispose()
        {
        }
        
        public override IEnumerable Tracks {
            get {
                return this.tracks;
            }
        }
        
        public override int Count {
            get {
                return this.tracks.Count;
            }
        }

        public override Gdk.Pixbuf Icon {
            get {
                return IconThemeUtils.LoadIcon(22, "network-server", Gtk.Stock.Network);
            }
        }
    }
}
