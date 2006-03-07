/***************************************************************************
 *  BeagleImportSource.cs
 *
 *  Copyright (C) 2005-2006 Novell, Inc., Adam Lofts
 *	Written by Aaron Bockover <aaron@abock.org>
 *             Adam Lofts <adam.lofts@gmail.com>
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
using System.Threading;
using Mono.Unix;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Widgets;

using Beagle;

namespace Banshee.Plugins.BeaglePlugin
{
    public class BeagleImportSource : IImportSource
    {
        public string Name {
            get { return Catalog.GetString("Beagle"); }
        }
    
        private static Gdk.Pixbuf icon = IconThemeUtils.LoadIcon(22, "system-search", Gtk.Stock.Find);
        public Gdk.Pixbuf Icon {
            get { return icon; }
        }

        private static string [] supported_mime_types = new string [] { 
            "audio/mpeg",
            "application/ogg", 
            "video/x-ms-asf" 
        };
        
        private Query query;
        private ActiveUserEvent user_event;
        private Thread thread;
        
        public void Import()
        {
            if(query != null || thread != null) {
                Console.WriteLine("Another Beagle query is already running");
                return;
            }

            thread = ThreadAssist.Spawn(ThreadedImport);
        }

        private void ThreadedImport()
        {
            query = new Query();
            query.AddDomain(QueryDomain.Neighborhood);
            query.MaxHits = 10000; // ugh?
        
            QueryPart_Or query_part_union = new QueryPart_Or();

            foreach(string mimetype in supported_mime_types) {
                QueryPart_Property part = new QueryPart_Property();
                part.Type = PropertyType.Keyword;
                part.Key = "beagle:MimeType";
                part.Value = mimetype;
                query_part_union.Add(part);
            }

            query.AddPart(query_part_union);
            
            query.HitsAddedEvent += OnHitsAdded;
            query.FinishedEvent += OnFinished;

            user_event = new ActiveUserEvent(Catalog.GetString("Import from Beagle"));
            user_event.Header = Catalog.GetString("Importing from Beagle");
            user_event.Message = Catalog.GetString("Running query...");
            user_event.Icon = Icon;
            user_event.CancelRequested += OnCancelRequested;
            
            try {
                query.SendAsyncBlocking();
            } catch(Exception e) {
                DisposeQuery();
                LogCore.Instance.PushError(Catalog.GetString("Could not query Beagle Daemon"),
                    e.Message, true);
                return;
            }
            
            if(SourceManager.ActiveSource is LibrarySource) {
                LibrarySource.Instance.Activate();
            }
        }
        
        private void DisposeQuery()
        {
            if(query != null) {
                query.HitsAddedEvent -= OnHitsAdded;
                query.FinishedEvent -= OnFinished;
                query.Close();
                query = null;
            }
            
            if(user_event != null) {
                user_event.Dispose();
                user_event = null;
            }
            
            thread = null;
        }
        
        private static bool AcceptHit(Hit hit)
        {
            return hit.IsFile;
        }
        
        private void OnHitsAdded(HitsAddedResponse response)
        {
            foreach(Hit hit in response.Hits) {
                if(AcceptHit(hit)) {
                    BeagleUtil.HitToTrack(hit);
                }
            }
        }

        private void OnFinished(FinishedResponse response)
        {
            DisposeQuery();
        }
        
        private void OnCancelRequested(object o, EventArgs args)
        {
            DisposeQuery();
        }
    }
}
