/***************************************************************************
 *  StationStore.cs
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

namespace Banshee.Plugins.Radio
{
    public class StationStore : NodeStore
    {
        private Stations stations;
    
        public StationStore(Stations stations) : base(typeof(Station))
        {
            this.stations = stations;

            foreach(Station station in stations.List) {
                StationGroup group = FindGroupForStation(station);
                if(group == null) {
                    group = CreateGroup(station.Group);
                    AddNode(group);
                }
                
                group.AddChild(station);
            }
        }

        private StationGroup FindGroupForStation(Station station)
        {
            if(station == null) {
                return null;
            }
        
            foreach(TreeNode node in this) {
                if(!(node is StationGroup)) {
                    continue;
                }
                
                StationGroup group = node as StationGroup;
                if(group.Title != null && station.Group != null && 
                    group.Title.ToLower() == station.Group.ToLower()) {
                    return group;
                }
            }
            
            return null;
        }
        
        private StationGroup CreateGroup(string group)
        {
            if(group != null) {
                group = group.Trim();
            }
            
            return new StationGroup(group == null || group == String.Empty 
                ? "Unknown" : group);
        }
        
        public StationGroup AddStation(Station station)
        {
            StationGroup new_group = FindGroupForStation(station);
            StationGroup old_group = station.Parent as StationGroup;
            
            if(new_group == old_group && old_group != null) {
                return new_group;
            }
            
            if(old_group != null) {
                old_group.RemoveChild(station);
            
                if(old_group.ChildCount == 0) {
                    RemoveNode(old_group);
                }
            }
            
            if(new_group == null || old_group == null) {
                new_group = CreateGroup(station.Group);
                AddNode(new_group);
            }
            
            new_group.AddChild(station);
            return new_group;
        }
        
        public void RemoveStation(Station station)
        {
            StationGroup group = station.Parent as StationGroup;
            if(group == null) {
                return;
            }
            
            group.RemoveChild(station);
            if(group.ChildCount == 0) {
                RemoveNode(group);
            }
            
            stations.Remove(station);
            stations.Save();
        }
    }
}
