/***************************************************************************
 *  BeagleUtil.cs
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
using Banshee.Base;
using Beagle;

namespace Banshee.Plugins.BeaglePlugin
{
    public static class BeagleUtil
    {
        public static LibraryTrackInfo HitToTrack(Hit hit)
        {
            uint track_number = 0;
            uint track_count = 0;
            int year = 0;
            
            try {
                track_number = UInt32.Parse(hit.GetFirstProperty("fixme:tracknumber"));
            } catch { }
            
            try {
                track_count = UInt32.Parse(hit.GetFirstProperty("fixme:trackcount"));
            } catch { }
            
            try {
                year = Int32.Parse(hit.GetFirstProperty("fixme:year"));
            } catch { }
           
            try {
                LibraryTrackInfo track = new LibraryTrackInfo(
                    new SafeUri(hit.Uri),
                    hit.GetFirstProperty("fixme:artist"),
                    hit.GetFirstProperty("fixme:album"),
                    hit.GetFirstProperty("fixme:title"),
                    hit.GetFirstProperty("fixme:genre"),
                    track_number, track_count, year,
                    TimeSpan.Zero, null, RemoteLookupStatus.NoAttempt);
                       
                return track;
            } catch {
                return null;
            }
        }
    }
}
