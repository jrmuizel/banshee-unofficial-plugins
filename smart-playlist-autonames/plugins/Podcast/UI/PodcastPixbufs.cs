/***************************************************************************
 *  PodcastPixbufs.cs
 *
 *  Written by Mike Urbanski <michael.c.urbanski@gmail.com>
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

using Gdk;
using Gtk;

using Banshee.Base;

namespace Banshee.Plugins.Podcast.UI
{
    // These should all be registered with the StockManager.

    public static class PodcastPixbufs
    {
        private static Pixbuf podcast_icon_16;
        private static Pixbuf podcast_icon_22;
        private static Pixbuf podcast_icon_48;

        private static Pixbuf refresh;
        private static Pixbuf failed_activity;
        private static Pixbuf cancel_activity;
        private static Pixbuf playing_activity;
        private static Pixbuf download_activity;
        private static Pixbuf download_activity_insensitive;

        private static Pixbuf download_menu_icon;

        private static Pixbuf download_column_icon;
        private static Pixbuf activity_column_icon;

        static PodcastPixbufs ()
        {
            refresh = Gdk.Pixbuf.LoadFromResource ("view-refresh.png");

            podcast_icon_16 = Gdk.Pixbuf.LoadFromResource ("podcast-icon-16.png");
            podcast_icon_22 = Gdk.Pixbuf.LoadFromResource ("podcast-icon-22.png");
            podcast_icon_48 = Gdk.Pixbuf.LoadFromResource ("podcast-icon-48.png");

            cancel_activity = Gdk.Pixbuf.LoadFromResource("edit-delete.png");
            failed_activity = Gdk.Pixbuf.LoadFromResource("dialog-error.png");
            playing_activity = IconThemeUtils.LoadIcon ("now-playing-arrow", 15);

            download_menu_icon = Gdk.Pixbuf.LoadFromResource("go-down.png");

            download_activity = Gdk.Pixbuf.LoadFromResource("go-next.png");
            download_activity_insensitive = Gdk.Pixbuf.LoadFromResource("go-next-grey.png");

            activity_column_icon = IconThemeUtils.LoadIcon ("blue-speaker", 16);
            download_column_icon = Gdk.Pixbuf.LoadFromResource("document-save-as-16.png");
        }

        public static Pixbuf Refresh {
            get
            {
                return refresh;
            }
        }

        public static Pixbuf CancelActivity {
            get
            {
                return cancel_activity;
            }
        }

        public static Pixbuf DownloadActivity {
            get
            {
                return download_activity;
            }
        }

        public static Pixbuf DownloadActivityInsensitive {
            get
            {
                return download_activity_insensitive;
            }
        }

        public static Pixbuf DownloadMenuIcon {
            get
            {
                return download_menu_icon;
            }
        }

        public static Pixbuf PlayingActivity {
            get
            {
                return playing_activity;
            }
        }

        public static Pixbuf FailedActivity {
            get
            {
                return failed_activity;
            }
        }

        public static Pixbuf DownloadColumnIcon {
            get
            {
                return download_column_icon;
            }
        }

        public static Pixbuf ActivityColumnIcon {
            get
            {
                return activity_column_icon;
            }
        }

        public static Pixbuf PodcastIcon16 {
            get
            {
                return podcast_icon_16;
            }
        }

        public static Pixbuf PodcastIcon22 {
            get
            {
                return podcast_icon_22;
            }
        }

        public static Pixbuf PodcastIcon48 {
            get
            {
                return podcast_icon_48;
            }
        }
    }
}
