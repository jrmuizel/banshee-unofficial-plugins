/***************************************************************************
 *  RadioSource.cs
 *
 *  Copyright (C) 2006 Novell, Inc.
 *  Written by Aaron Bockover <aaron@abock.org>
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
using System.IO;
using System.Collections;
using Mono.Unix;
using Gtk;

using Banshee.Base;
using Banshee.Sources;
 
namespace Banshee.Plugins.Radio
{   
    public class RadioTrackInfo : TrackInfo
    {
        public RadioTrackInfo(Station station, string uri) : base()
        {
            Title = station.Title;
#if BANSHEE_0_10
            Uri = new Uri(uri);
#else
            Uri = new SafeUri(uri);
#endif
        }
    }

    public class RadioSource : Source
    {
        private string stations_file;
        private Stations stations;
        private StationStore store;
        private StationView view;
        private RadioPlugin plugin;
        
        private HBox box;
        
        public RadioSource(RadioPlugin plugin) : base(Catalog.GetString("Internet Radio"), 150)
        {
            this.plugin = plugin;
            
            stations_file = Path.Combine(Paths.UserPluginDirectory, "radio-stations.xml");
        
            if(!File.Exists(stations_file)) {
                string default_xml = Resource.GetFileContents("stations.xml");
                TextWriter writer = new StreamWriter(stations_file);
                writer.Write(default_xml);
                writer.Close();
            }
            
            stations = Stations.Load(stations_file);
            stations.Updated += delegate {
                view.QueueDraw();
            };
            
            store = new StationStore(stations);

            BuildInterface();
        }
        
        private void BuildInterface()
        {
            box = new HBox();
            
            ScrolledWindow view_scroll = new ScrolledWindow();
            view_scroll.HscrollbarPolicy = PolicyType.Never;
            view_scroll.VscrollbarPolicy = PolicyType.Automatic;
            view_scroll.ShadowType = ShadowType.In;
            
            view = new StationView(store);
            view.Popup += OnViewPopup;
            view.RowActivated += OnViewRowActivated;
            view.HeadersVisible = true;
            view.AppendColumn(Catalog.GetString("Station"), new CellRendererText(), "text", 0);
            view.AppendColumn(Catalog.GetString("Description"), new CellRendererText(), "text", 1);
            view.AppendColumn(Catalog.GetString("Stream"), new CellRendererText(), "text", 2);
            view_scroll.Add(view);
            
            view.ExpandAll();
            
            box.PackStart(view_scroll, true, true, 0);
            box.ShowAll();
        }
        
        private void OnViewPopup(object o, StationViewPopupArgs args)
        {
            Station station = view.NodeSelection.SelectedNode as Station;
            plugin.StationActions.Sensitive = !(station == null || station is StationGroup);
                
            Menu menu = Globals.ActionManager.GetWidget("/RadioMenu") as Menu;
            menu.ShowAll();
            menu.Popup(null, null, null, 0, args.Time);
        }
        
        private void OnViewRowActivated(object o, RowActivatedArgs args)
        {
            Station station = store.GetNode(args.Path) as Station;
            if(station == null || station is StationGroup) {
                return;
            }
            
            Link link = station.SelectedStream;
            if(link == null) {
                return;
            }
            
            string uri = link.Href;
                        
            try {
                if(!Gnome.Vfs.Vfs.Initialized) {
                    Gnome.Vfs.Vfs.Initialize();
                }
            

                ArrayList uris = new ArrayList();
                TotemPlParser.Parser parser = new TotemPlParser.Parser();
        	        parser.Entry += delegate(object o, TotemPlParser.EntryArgs args) {
        	           uris.Add(args.Uri);
        	        };
    	        
        	        TotemPlParser.Result result = parser.Parse(uri, false);
        	        if(result == TotemPlParser.Result.Success && uris.Count > 0) {	   
                    uri = uris[0] as string;
                }
            } catch(Exception e) {
                Console.WriteLine("Could not parse URI with totem-plparser: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            
            RadioTrackInfo track = new RadioTrackInfo(station, uri);
            PlayerEngineCore.OpenPlay(track);
        }
        
        private void OnStationSaved(object o, StationSavedArgs args)
        {
            store.AddStation(args.Station);
        }
        
        private void StationDialog(Station station)
        {
            StationDialog dialog = new StationDialog(stations, station);
            dialog.Saved += OnStationSaved;
            dialog.Run();
        }
        
        internal void NewStation()
        {
            StationDialog(null);
        }
                
        internal void EditStation()
        {
            Station station = view.NodeSelection.SelectedNode as Station;
            if(station == null || station is StationGroup) {
                return;
            }
            
            StationDialog(station);
        }
        
        internal void RemoveStation()
        {
            Station station = view.NodeSelection.SelectedNode as Station;
            if(station == null || station is StationGroup) {
                return;
            }
            
            store.RemoveStation(station);
        }
        
        public override Gtk.Widget ViewWidget {
            get { return box; }
        }
        
        public override int Count {
            get { return stations.Count; }
        }
        
        public override bool SearchEnabled {
            get { return false; }
        }
        
        private static Gdk.Pixbuf icon = IconThemeUtils.LoadIcon(22, "network-wireless", "source-library");
        public override Gdk.Pixbuf Icon {
            get { return icon; } 
        }
    }
}
