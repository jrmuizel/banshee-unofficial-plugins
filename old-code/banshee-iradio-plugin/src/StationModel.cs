/***************************************************************************
 *  StationModel.cs
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
using System.Xml;
using System.Xml.Serialization;

using Gtk;

using Banshee.Base;

namespace Banshee.Plugins.Radio
{
    [XmlRoot(ElementName="stations")]
    public class Stations
    {
        public event EventHandler Updated;
        
        private string path;
    
        [XmlIgnore()]
        public string Path {
            get { return path; }
            private set { path = value; }
        }
    
        [XmlElement("station", typeof(Station))]
        public ArrayList SerializeList = new ArrayList();
        
        public void Add(Station station)
        {
            SerializeList.Add(station);
        }
        
        public void Remove(Station station)
        {
            SerializeList.Remove(station);
        }
        
        public void Update()
        {
            EventHandler handler = Updated;
            if(handler != null) {
                handler(this, new EventArgs());
            }
        }
        
        [XmlIgnore()]
        public IEnumerable List {
            get { return SerializeList; }
        }
        
        [XmlIgnore()]
        public int Count {
            get { return SerializeList.Count; }
        }
        
        [XmlIgnore()]
        public IEnumerable Groups {
            get {
                ArrayList groups = new ArrayList();
                
                foreach(Station station in List) {
                    bool can_add = true;
                    foreach(string group in groups) {
                        string a = station.Group;
                        string b = group.ToLower().Trim();
                        
                        if(a != null) {
                            a = a.ToLower().Trim();
                        }
                        
                        if(a == b) {
                            can_add = false;
                            break;
                        }
                    }
                    
                    if(can_add) {
                        groups.Add(station.Group);
                    }
                }
                
                return groups;
            }
        }
        
        public override string ToString()
        {
            string ret = String.Empty;
            
            foreach(Station station in List) {
                ret += station + "\n";
            }
            
            return ret;
        }
        
        public static Stations Load(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Stations));
            
            try {
                Stations stations;
                
                if(File.Exists(path)) {
                    stations = (Stations)serializer.Deserialize(new XmlTextReader(path));
                } else {
                    stations = new Stations();
                }
                
                stations.Path = path;
                return stations;
            } catch(Exception e) {
                Console.WriteLine("Could not deserialize Stations XML file: " + e.Message);
                Stations stations = new Stations();
                stations.Path = path;
                return stations;
            }
        }
        
        public void Save()
        {
            Save(Path);
        }
        
        public void Save(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Stations));
            if(File.Exists(path)) {
                File.Delete(path);
            }
            TextWriter writer = new StreamWriter(path);
            serializer.Serialize(writer, this);
            writer.Close();
        }
    }
    
    public class StationGroup : Station
    {
        public StationGroup(string title)
        {
            Title = title;
        }
    }
    
    [TreeNode(ColumnCount=3)]
    public class Station : TreeNode
    {
        private string title;
        private string description;
        private string group;
    
        [XmlElement("link", typeof(Link))]
        public ArrayList SerializeLinks = new ArrayList();
        
        public void AddLink(Link link)
        {
            SerializeLinks.Add(link);
        }
        
        public void RemoveLink(Link link)
        {
            SerializeLinks.Remove(link);
        }
        
        public void ClearLinks()
        {
            SerializeLinks.Clear();
        }
        
        [XmlIgnore()]
        public IEnumerable Links {
            get { return SerializeLinks; }
        }
        
        [XmlIgnore()]
        public Link SelectedStream {
            get {
                if(Links == null) {
                    return null;
                }
            
                foreach(Link link in Links) {
                    if(link.Selected) {
                        return link;
                    }
                }
                
                foreach(Link link in Links) {
                    if(link.Type == LinkType.Stream) {
                        return link;
                    }
                }
                
                return null;
            }
        }

        [XmlIgnore()]
        [TreeNodeValue(Column=2)]
        public string SelectedStreamString {
            get { 
                Link stream = SelectedStream;
                if(stream == null) {
                    return String.Empty;
                }
                
                return stream.Title;
            }
        }
                
        [XmlElement(ElementName="title")]
        [TreeNodeValue(Column=0)]
        public string Title {
            get { return title; }
            set { title = value; }
        }
        
        [XmlElement(ElementName="description")]
        [TreeNodeValue(Column=1)]
        public string Description {
            get { return description; }
            set { description = value; }
        }
                
        [XmlElement(ElementName="group")]
        public string Group {
            get { return group; }
            set { group = value; }
        }
        
        public override string ToString()
        {
            string ret = String.Empty;
            
            ret += String.Format("Title:       {0}\n", title);
            ret += String.Format("Description: {0}\n", description);
            ret += String.Format("Links:       ({0})\n", SerializeLinks.Count);
            foreach(Link link in Links) {
                ret += String.Format("  {0}\n", link);
            }
            
            return ret;
        }
    }

    public enum LinkType {
        [XmlEnum(Name="info")] Info,
        [XmlEnum(Name="stream")] Stream
    }
    
    [TreeNode(ColumnCount=3)]
    public class Link : TreeNode
    {
        private bool selected;
        private LinkType type;
        private string href;
        private string title;

        public Link() : this(LinkType.Info)
        {
        }
        
        public Link(LinkType type)
        {
            this.type = type;
        }

        [XmlAttribute(AttributeName="type")]
        public LinkType Type {
            get { return type; }
            set { type = value; }
        }
        
        [TreeNodeValue(Column=0)]
        public string TypeAsString {
            get { return type.ToString(); }
        }

        [XmlAttribute(AttributeName="href")]
        [TreeNodeValue(Column=2)]
        public string Href {
            get { return href; }
            set { href = value; }
        }

        [XmlAttribute(AttributeName="title")]
        [TreeNodeValue(Column=1)]
        public string Title {
            get { return title; }
            set { title = value; }
        }

        [XmlAttribute(AttributeName="selected")]
        public bool Selected {
            get { return selected; }
            set { selected = value && type == LinkType.Stream; }
        }
        
        public override string ToString()
        {
            return String.Format("{0} \"{1}\" <{2}>", type, title, href) 
                + (selected ? " (selected)" : "");
        }
    }
}
