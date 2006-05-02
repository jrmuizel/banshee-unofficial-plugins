using System;
using Mono.Unix;
using Gtk;

using Banshee.Base;
using Banshee.Sources;
 
namespace Banshee.Plugins.MiniMode
{
    public class MiniModePlugin : Banshee.Plugins.Plugin
    {
        private MiniMode mini_mode = null;
        private Menu viewMenu;
        private MenuItem menuItem;
        
        protected override string ConfigurationName { 
            get { return "MiniMode"; } 
        }
        
        public override string DisplayName { 
            get { return "Mini Mode"; } 
        }
        
        public override string Description {
            get {
                return Catalog.GetString(
                    "Mini Mode allows controlling Banshee through a small " +
                    "window with only playback controls"
                );
            }
        }
        
        public override string [] Authors {
            get { return new string [] { "Felipe Almeida Lessa" }; }
        }
 
        // --------------------------------------------------------------- //

        protected override void PluginInitialize()
        {
            // Defer creation of window to when it's needed
            // mini_mode = new MiniMode();
        }
        
        protected override void InterfaceInitialize()
        {
            viewMenu = (Globals.ActionManager.GetWidget("/MainMenu/ViewMenu") as MenuItem).Submenu as Menu;
            menuItem = new MenuItem(Catalog.GetString("Mini mode"));
            menuItem.Activated += delegate {
                if (mini_mode == null)
                    mini_mode =  new MiniMode();
                mini_mode.Show();
            };
            viewMenu.Insert(menuItem, 2);
            menuItem.Show();
        }
        
        protected override void PluginDispose()
        {
            if(viewMenu != null && menuItem != null)
                viewMenu.Remove(menuItem);

        
            if (mini_mode != null) {
                // We'll do our visual cleaning in a timeout to avoid
                // glitches when Banshee quits. Besides, the plugin window is
                // accessible only on the full mode, so this won't cause any
                // trouble.
                GLib.Timeout.Add(1000, delegate {
                    try {
                        mini_mode.Hide();
                    } catch { /* Do not do anything -- we tried! =) */ }
                    return false;
                });
            }
        }
    }
}
