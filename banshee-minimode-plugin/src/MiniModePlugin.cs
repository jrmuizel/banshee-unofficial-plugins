using System;
using Mono.Unix;
using Gtk;

using Banshee.Base;
using Banshee.Sources;
 
namespace Banshee.Plugins.MiniMode
{
    public class MiniModePlugin : Banshee.Plugins.Plugin
    {
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
            get { return new string [] {  "You!" }; }
        }
 
        // --------------------------------------------------------------- //

        protected override void PluginInitialize()
        {
            // This method is called when the plugin is loaded
        }
        
        protected override void InterfaceInitialize()
        {
            // if you need to hook into the main UI, start this
            // process here. This method will be called when the
            // main interface is done loading
            // 
            // For instance:
            
            MiniMode mini_mode = new MiniMode();
            mini_mode.Window.Show();
        }
        
        protected override void PluginDispose()
        {
            // if the plugin is holding onto any resources that
            // need to be explicitly disposed of, take this
            // as a hint to do so... this probably won't be 
            // necessary with the MiniMode plugin though ;)
        }
    }
}
