/***************************************************************************
 *  PodcastPropertiesDialog.cs
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

using System;
using System.Collections;

using Gtk;
using Pango;

using Banshee.Base;
using Banshee.Plugins.Podcast;

namespace Banshee.Plugins.Podcast.UI
{
    internal class PodcastPropertiesDialog : Dialog
    {
        private PodcastInfo pi;

        public PodcastPropertiesDialog (PodcastInfo pi) :
                base (pi.Title, InterfaceElements.MainWindow, DialogFlags.DestroyWithParent)
        {
            if (pi == null)
            {
                throw new ArgumentNullException ("pi");
            }

            this.pi = pi;
            BuildWindow ();
            IconThemeUtils.SetWindowIcon (this);
        }

        private void BuildWindow()
        {
            BorderWidth = 6;
            VBox.Spacing = 12;

            HBox content_box = new HBox();
            content_box.BorderWidth = 6;
            content_box.Spacing = 12;

            Table table = new Table (2, 6, false);
            table.RowSpacing = 6;
            table.ColumnSpacing = 12;

            ArrayList labels = new ArrayList ();
            /*
                        Label author_label = new Label ();
                        author_label.Markup = "<b>"+Catalog.GetString ("Author:")+"</b>";            
                        labels.Add (author_label);         
            */
            Label feed_label = new Label ();
            feed_label.Markup = "<b>"+Catalog.GetString ("Feed:")+"</b>";
            labels.Add (feed_label);

            Label pubdate_label = new Label ();
            pubdate_label.Markup = "<b>"+Catalog.GetString ("Date:")+"</b>";
            labels.Add (pubdate_label);

            Label url_label = new Label ();
            url_label.Markup = "<b>"+Catalog.GetString ("URL:")+"</b>";
            labels.Add (url_label);

            Label description_label = new Label ();
            description_label.Markup = "<b>"+Catalog.GetString ("Description:")+"</b>";
            labels.Add (description_label);

            Label feed_title_text = new Label (pi.Feed.Title);
            labels.Add (feed_title_text);

            Label pubdate_text = new Label (pi.PubDate.ToString ("f"));
            labels.Add (pubdate_text);

            Label url_text = new Label (pi.Url.ToString ());
            labels.Add (url_text);
            url_text.Wrap = true;
            url_text.Selectable = true;
            url_text.Ellipsize = Pango.EllipsizeMode.End;

            string description_string = (pi.Description == String.Empty ||
                                         pi.Description == null) ?
                                        Catalog.GetString ("No description available") :
                                        pi.Description;

            /*   string author_string = (pi.Author == String.Empty ||
                pi.Author == null) ? 
                 Catalog.GetString ("No author") : 
                 pi.Author;                          
                           
                        Label author_text = new Label (author_string);
                        labels.Add (author_text);                                                                            
            */
            if (!description_string.StartsWith ("\""))
            {
                description_string =  "\""+description_string;
            }

            if (!description_string.EndsWith ("\""))
            {
                description_string = description_string+"\"";
            }

            Label description_text = new Label (description_string);
            labels.Add (description_text);
            description_text.Wrap = true;
            description_text.Selectable = true;

            Viewport description_viewport = new Viewport();
            description_viewport.ShadowType = ShadowType.None;

            ScrolledWindow description_scroller = new ScrolledWindow ();
            description_scroller.HscrollbarPolicy = PolicyType.Never;
            description_scroller.VscrollbarPolicy = PolicyType.Automatic;

            description_viewport.Add (description_text);
            description_scroller.Add (description_viewport);

            /*
                        table.Attach (
                         author_label, 0, 1, 0, 1,
                             AttachOptions.Fill, AttachOptions.Fill, 0, 0
                        );                 
            */
            table.Attach (
                feed_label, 0, 1, 0, 1,
                AttachOptions.Fill, AttachOptions.Fill, 0, 0
            );

            table.Attach (
                pubdate_label, 0, 1, 1, 2,
                AttachOptions.Fill, AttachOptions.Fill, 0, 0
            );

            table.Attach (
                url_label, 0, 1, 3, 4,
                AttachOptions.Fill, AttachOptions.Fill, 0, 0
            );

            table.Attach (
                description_label, 0, 1, 5, 6,
                AttachOptions.Fill, AttachOptions.Fill, 0, 0
            );
            /*
               table.Attach (
                author_text, 1, 2, 0, 1,
                            AttachOptions.Fill, AttachOptions.Fill, 0, 0
                        );               
            */
            table.Attach (
                feed_title_text, 1, 2, 0, 1,
                AttachOptions.Fill, AttachOptions.Fill, 0, 0
            );

            table.Attach (
                pubdate_text, 1, 2, 1, 2,
                AttachOptions.Fill, AttachOptions.Fill, 0, 0
            );


            table.Attach (
                url_text, 1, 2, 3, 4,
                AttachOptions.Fill, AttachOptions.Fill, 0, 0
            );

            table.Attach (description_scroller, 1, 2, 5, 6,
                          AttachOptions.Expand | AttachOptions.Fill,
                          AttachOptions.Expand | AttachOptions.Fill, 0, 0
                         );

            foreach (Label l in labels)
            {
                AlignAndJustify (l);
            }

            content_box.PackStart (table, true, true, 0);

            Button ok_button = new Button("gtk-ok");
            ok_button.CanDefault = true;
            ok_button.Show ();

            AddActionWidget (ok_button, ResponseType.Ok);

            DefaultResponse = ResponseType.Ok;
            ActionArea.Layout = ButtonBoxStyle.End;

            content_box.ShowAll ();
            VBox.Add (content_box);

            Response += OnResponse;
        }

        private void AlignAndJustify (Label label)
        {
            label.SetAlignment (0f, 0f);
            label.Justify = Justification.Left;
        }

        private void OnResponse(object sender, ResponseArgs args)
        {
            (sender as Dialog).Response -= OnResponse;
            (sender as Dialog).Destroy();
        }
    }
}
