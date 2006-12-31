/***************************************************************************
 *  StationView.cs
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
using Gtk;

using Mono.Unix;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Playlists.Formats.Xspf;
 
namespace Banshee.Plugins.Radio
{
    public delegate void StationViewPopupHandler(object o, StationViewPopupArgs args);
    
    public class StationViewPopupArgs : EventArgs
    {
        private uint time;
        
        public StationViewPopupArgs(uint time)
        {
            this.time = time;
        }
        
        public uint Time {
            get { return time; }
        }
    }
    
    public class StationView : TreeView
    {        
        public event StationViewPopupHandler Popup;
        private StationModel model;
        
        protected StationView(IntPtr ptr) : base(ptr)
        {
        }
    
        public StationView(StationModel model) : base(model)
        {
            this.model = model;
            HeadersVisible = true;
            
            TreeViewColumn station_column = AppendColumn(Catalog.GetString("Station"), 
                new CellRendererStation(model), "text", 0);
            station_column.Sizing = TreeViewColumnSizing.Autosize;
            
            TreeViewColumn comment_column = AppendColumn(Catalog.GetString("Comment"), 
                new CellRendererText(), "text", 1);
            comment_column.Sizing = TreeViewColumnSizing.Autosize;
            
            model.Reloaded += delegate {
                ExpandAll();
            };
        }
        
        protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
        {
            if(evnt.Window != BinWindow || evnt.Button != 3) {
                return base.OnButtonPressEvent(evnt);
            }
            
            bool ret = base.OnButtonPressEvent(evnt);
            
            OnPopup(evnt.Time);

            return ret;
        }
        
        private void OnPopup(uint time)
        {
            StationViewPopupHandler handler = Popup;
            if(handler != null) {
                handler(this, new StationViewPopupArgs(time));
            }
        }
        
        protected override bool OnPopupMenu()
        {
            OnPopup(Gtk.Global.CurrentEventTime);
            return base.OnPopupMenu();
        }
        
        public Track SelectedTrack {
            get {
                TreeModel _model;
                TreeIter iter;
            
                if(!Selection.GetSelected(out _model, out iter)) {
                    return null;
                }
                
                return model.GetTrack(iter);
            }
        }
        
        public RadioTrackInfo SelectedRadioTrackInfo {
            get {
                TreeModel _model;
                TreeIter iter;
            
                if(!Selection.GetSelected(out _model, out iter)) {
                    return null;
                }
                
                return model.GetRadioTrackInfo(iter);
            }
        }
    }
}
