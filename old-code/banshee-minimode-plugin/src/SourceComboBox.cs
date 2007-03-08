using System;
using Mono.Unix;
using Gtk;
using Glade;

using Banshee.Base;
using Banshee.MediaEngine;
using Banshee.Sources;

namespace Banshee.Plugins.MiniMode
{ 
    public class SourceModel : Gtk.ListStore
    {
	// We're a singleton
	private static SourceModel this_;
	public static SourceModel This { get {
	    if (this_ == null)
	        this_ = new SourceModel();
	    return this_;
	} }
        
        private SourceModel() : base(typeof(Gdk.Pixbuf), typeof(string), typeof(Source))
        {          
            // Initial list of sources
            Clear();
            foreach(Source source in SourceManager.Sources) {
                SetSource(Append(), source);
            }
            
            // Be prepared for other sources
            SourceManager.SourceAdded += delegate(SourceAddedArgs args) {
                if(FindSource(args.Source).Equals(TreeIter.Zero)) {
                    TreeIter iter = Insert(args.Position);
                    SetSource(iter, args.Source);
                }
            };
            
            SourceManager.SourceRemoved += delegate(SourceEventArgs args) {
                TreeIter iter = FindSource(args.Source);
                if(!iter.Equals(TreeIter.Zero)) {
                    Remove(ref iter);
                }
            };
        }

        private void SetSource(TreeIter iter, Source source) {
            Gdk.Pixbuf icon = source.Icon;
            if(icon == null) {
                icon = IconThemeUtils.LoadIcon(22, "source-library");
            }
            SetValue(iter, 0, icon);
            SetValue(iter, 1, source.Name);
            SetValue(iter, 2, source);
        }

        private TreeIter FindSource(Source source) {
            for(int i = 0, n = IterNChildren(); i < n; i++) {
                TreeIter iter = TreeIter.Zero;
                if(!IterNthChild(out iter, i)) {
                    continue;
                }
                
                if((GetValue(iter, 2) as Source) == source) {
                    return iter;
                }
            }

            return TreeIter.Zero;
        }
    }
    

    public class SourceComboBox : Gtk.ComboBox
    {
        private bool updating = false;
        
        public SourceComboBox()
        {
            // Prepare the renderer
            Clear();
            CellRendererPixbuf image = new CellRendererPixbuf();
            PackStart(image, false);
            AddAttribute(image, "pixbuf", 0);
            CellRendererText text = new CellRendererText();
            PackStart(text, true);
            AddAttribute(text, "text", 1);
            
            // Take the model
            Model = SourceModel.This;
            
            // Hook up everything
            SourceManager.ActiveSourceChanged += delegate { UpdateSelectedSource(); };
            
            SourceManager.SourceUpdated += delegate(SourceEventArgs args) {
                QueueDraw();
            };
        }

        public void UpdateSelectedSource()
        {
            if(updating)
                return;
            updating = true;
            try {
                TreeIter iter;            
                if(SourceModel.This.IterNthChild(out iter, SourceManager.ActiveSourceIndex)) {
                    SetActiveIter(iter);
                }
            } finally {
                updating = false;
            }
        }

        protected override void OnChanged()
        {
            if(updating)
                return;
            
            TreeIter iter;
            
            if(GetActiveIter(out iter)) {
                Source new_source = SourceModel.This.GetValue(iter, 2) as Source;
                if(new_source != null && SourceManager.ActiveSource != new_source) {
                    SourceManager.SetActiveSource(new_source);
                    QueueDraw();
                }
            }
        }
    } 
}
