/***************************************************************************
 *  RadioPlugin.cs
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
using Gtk;

using Banshee.Base;
using Banshee.Sources;
 
namespace Banshee.Plugins.Radio
{
    public class RadioPlugin : Banshee.Plugins.Plugin
    {
        protected override string ConfigurationName { get { return "iradio"; } }
        public override string DisplayName { get { return "Internet Radio"; } }
        
        public override string Description {
            get {
                return Catalog.GetString(
                    "Provides Internet radio/streaming audio station support"
                );
            }
        }
        
        public override string [] Authors {
            get {
                return new string [] {
                    "Aaron Bockover"
                };
            }
        }
        
        private RadioSource source;
                
        private ActionGroup global_actions;
        private ActionGroup station_actions;
        private uint ui_manager_id;
        private Menu music_menu;
        private MenuItem new_station;
        
        protected override void PluginInitialize()
        {
            source = new RadioSource(this);
            SourceManager.AddSource(source);
            LoadMenus();
        }
        
        protected override void PluginDispose()
        {
            SourceManager.RemoveSource(source);
            UnloadMenus();
        }
        
        private void LoadMenus()
        {
            global_actions = new ActionGroup("RadioGlobal");
            global_actions.Add(new ActionEntry [] {
                new ActionEntry("RadioStationAddAction", null,
                    Catalog.GetString("New Radio Station..."), null,
                    Catalog.GetString("Add a new streaming radio station"), OnRadioStationAddActivate),
            });
            
            station_actions = new ActionGroup("RadioStation");
            station_actions.Add(new ActionEntry [] {
                new ActionEntry("RadioStationEditAction", null,
                    Catalog.GetString("Edit Station"), null,
                    Catalog.GetString("Edit selected streaming radio station"), OnRadioStationEditActivate),
                    
                new ActionEntry("RadioStationRemoveAction", null,
                    Catalog.GetString("Remove Station"), null,
                    Catalog.GetString("Remove selected streaming radio station"), OnRadioStationRemoveActivate)
            });
            
            Globals.ActionManager.UI.InsertActionGroup(global_actions, 0);
            Globals.ActionManager.UI.InsertActionGroup(station_actions, 0);
            
            ui_manager_id = Globals.ActionManager.UI.AddUiFromResource("radioactions.xml");
            
            music_menu = (Globals.ActionManager.GetWidget("/MainMenu/MusicMenu") as MenuItem).Submenu as Menu;
            new_station = Globals.ActionManager.GetWidget("/RadioMergeMenu/RadioStationAdd") as MenuItem;
            (new_station.Parent as Container).Remove(new_station);
            music_menu.Insert(new_station, 2);
        }
        
        private void UnloadMenus()
        {
            Globals.ActionManager.UI.RemoveUi(ui_manager_id);
            Globals.ActionManager.UI.RemoveActionGroup(global_actions);
            Globals.ActionManager.UI.RemoveActionGroup(station_actions);
            music_menu.Remove(new_station);
            music_menu = null;
            new_station = null;
        }
        
        private void OnRadioStationAddActivate(object o, EventArgs args)
        {
            source.NewStation();
        }
        
        private void OnRadioStationEditActivate(object o, EventArgs args)
        {
            source.EditStation();
        }
        
        private void OnRadioStationRemoveActivate(object o, EventArgs args)
        {
            source.RemoveStation();
        }
        
        public ActionGroup GlobalActions {
            get { return global_actions; }
        }
        
        public ActionGroup StationActions {
            get { return station_actions; }
        }
    }
}
