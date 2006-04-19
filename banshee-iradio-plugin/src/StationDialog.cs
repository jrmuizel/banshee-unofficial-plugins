/***************************************************************************
 *  StationDialog.cs
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
using System.Collections;
using Mono.Unix;

using Gtk;
using Glade;

using Banshee.Base;

namespace Banshee.Plugins.Radio
{
    public class LinkStore : NodeStore
    {
        public LinkStore(Station station) : base(typeof(Link)) 
        {
            if(station.Links == null) {
                return;
            }
            
            foreach(Link link in station.Links) {
                AddNode(link);
            }
        }
    }
    
    public class StationSavedArgs : EventArgs
    {
        public Station Station;
    }
    
    public delegate void StationSavedHandler(object o, StationSavedArgs args);
    
    public class StationDialog
    {
        private Glade.XML glade;
        private Dialog dialog;
        
        private Stations stations;
        private Station station;
        private LinkStore link_store;
        private NodeView view;
        private bool is_new = false;
        
        private Hashtable renderer_map = new Hashtable();
        private int renderers = 0;
        
        [Widget] private Entry title_entry;
        [Widget] private Entry description_entry;
        
        private ComboBoxEntry group_combo;
        
        public event StationSavedHandler Saved;
        
        public StationDialog(Stations stations, Station station)
        {       
            glade = new Glade.XML(null, "radio.glade", "StationDialog", "banshee");
            glade.Autoconnect(this);
            dialog = (Dialog)glade.GetWidget("StationDialog");
            Banshee.Base.IconThemeUtils.SetWindowIcon(dialog);
            
            if(station == null) {
                is_new = true;
                station = new Station();
            }
            
            this.stations = stations;
            this.station = station;
            
            link_store = new LinkStore(station);
            view = new NodeView(link_store);
            view.HeadersVisible = true;
            
            CellRendererToggle active_renderer = new CellRendererToggle();
            active_renderer.Activatable = true;
            active_renderer.Radio = true;
            active_renderer.Toggled += OnLinkToggled;

            view.AppendColumn(Catalog.GetString("Active"), active_renderer, ActiveDataFunc);
            view.AppendColumn(Catalog.GetString("Type"), new CellRendererText(), TypeDataFunc);
            view.AppendColumn(Catalog.GetString("Title"), EditTextRenderer(), "text", 1);
            view.AppendColumn(Catalog.GetString("URI"), EditTextRenderer(), "text", 2);
            view.Show();
            
            (glade["link_view_container"] as ScrolledWindow).Add(view);
            
            group_combo = ComboBoxEntry.NewText();
            group_combo.Show();
            (glade["group_combo_container"] as Box).PackStart(group_combo, true, true, 0);
            
            title_entry.Text = station.Title == null ? String.Empty : station.Title;
            description_entry.Text = station.Description == null ? String.Empty : station.Description;
            group_combo.Entry.Text = station.Group == null ? String.Empty : station.Group;
            
            foreach(string group in stations.Groups) {
                group_combo.AppendText(group);
            }
        }
        
        private CellRendererText EditTextRenderer()
        {
            CellRendererText renderer = new CellRendererText();
            renderer.Editable = true;
            renderer.Edited += OnCellEdited;
            renderer_map[renderer] = renderers++;
            return renderer;
        }
        
        private void ActiveDataFunc(TreeViewColumn tree_column, CellRenderer cell, ITreeNode node)
        {
            CellRendererToggle toggle = cell as CellRendererToggle;
            Link link = node as Link;
            
            if(toggle == null || link == null) {
                return;
            }
            
            toggle.Active = link.Selected;
            toggle.Sensitive = link.Type == LinkType.Stream;
        }
        
        private void TypeDataFunc(TreeViewColumn tree_column, CellRenderer cell, ITreeNode node)
        {
            CellRendererText renderer = cell as CellRendererText;
            Link link = node as Link;
            
            if(renderer == null || link == null) {
                return;
            }
            
            switch(link.Type) {
                case LinkType.Stream: renderer.Text = Catalog.GetString("Stream"); break;
                case LinkType.Info: renderer.Text = Catalog.GetString("Information"); break;
            }
        }
        
        private void OnLinkToggled(object o, ToggledArgs args)
        {
            CellRendererToggle toggle = o as CellRendererToggle;
            TreePath path = new TreePath(args.Path);
            Link link = link_store.GetNode(path) as Link;
            
            if(toggle == null || link == null || link.Type != LinkType.Stream) {
                return;
            }

            foreach(Link i_link in link_store) {
                i_link.Selected = false;
            }
            
            link.Selected = true;
            stations.Update();
            view.QueueDraw();
        }
        
        private void OnCellEdited(object o, EditedArgs args)
        {
            Link link = SelectedLink;
            if(link == null) {
                return;
            }
            
            int renderer = (int)renderer_map[o];
            
            switch(renderer) {
                case 0: link.Title = args.NewText; break;
                case 1: link.Href = args.NewText; break;
                default: break;
            }
            
            view.ColumnsAutosize();
        }

        private void OnAddStreamLinkClicked(object o, EventArgs args)
        {
            link_store.AddNode(new Link(LinkType.Stream));
        }
                
        private void OnAddInfoLinkClicked(object o, EventArgs args)
        {
            link_store.AddNode(new Link(LinkType.Info));
        }
        
        private void OnRemoveLinkClicked(object o, EventArgs args)
        {
            link_store.RemoveNode(SelectedLink);
        }
        
        public void Run()
        {
            if(dialog.Run() == (int)ResponseType.Ok) {
                station.ClearLinks();
                
                foreach(Link link in link_store) {
                    station.AddLink(link);
                }
                            
                station.Title = title_entry.Text;
                station.Description = description_entry.Text;
                station.Group = group_combo.Entry.Text;
                
                if(is_new) {
                    stations.Add(station);
                }
                
                stations.Save();
                
                StationSavedHandler handler = Saved;
                if(handler != null) {
                    StationSavedArgs args = new StationSavedArgs();
                    args.Station = station;
                    handler(this, args);
                }
            }
            
            dialog.Destroy();
        }
        
        private Link SelectedLink {
            get { return view.NodeSelection.SelectedNode as Link; }
        }
    }
}
