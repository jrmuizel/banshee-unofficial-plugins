/***************************************************************************
 *  BeaglePlugin.cs
 *
 *  Copyright (C) 2006 Novell, Inc.
 *	Written by Aaron Bockover <aaron@abock.org>
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
    public class BeaglePlugin : Banshee.Plugins.Plugin
    {
    		public override string DisplayName { get { return "Beagle Integration"; } }
        protected override string ConfigurationName { get { return "Beagle"; } }
        
        public override string Description {
            get {
                return Catalog.GetString(
                    "Allow importing music directly from Beagle"
                );
            }
        }
        
        public override string [] Authors {
            get {
                return new string [] {
                    "Aaron Bockover",
                    "Adam Lofts"
                };
            }
        }

        private BeagleImportSource import_source;

        protected override void PluginInitialize()
        {
            import_source = new BeagleImportSource();
            ImportSources.Add(import_source);
        }
        
        protected override void PluginDispose()
        {
            ImportSources.Remove(import_source);
            import_source = null;
        }
    }
}
