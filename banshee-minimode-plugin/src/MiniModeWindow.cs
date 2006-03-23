using System;
using Mono.Unix;
using Gtk;
using Glade;

using Banshee.Base;

namespace Banshee.Plugins.MiniMode
{
    public class MiniMode
    {
        [Widget] private Gtk.Window MiniModeWindow;

        private Glade.XML glade;
        
        public MiniMode()
        {
            glade = new Glade.XML("minimode.glade", "MiniModeWindow", "banshee");
            glade.Autoconnect(this);
            IconThemeUtils.SetWindowIcon(Window);
        }

        public Gtk.Window Window {
            get { return MiniModeWindow; }
        }
    }
}
