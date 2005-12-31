using System;
using Mono.Unix;
 
using Banshee.Base;
 
namespace Banshee.Plugins.Sample
{
    public class SamplePlugin : Banshee.Plugins.Plugin
    {
        public override string DisplayName { get { return "Sample"; } }
        
        public override string Description {
            get {
                return Catalog.GetString(
                    "A very simple Banshee plugin that shows a random " +
                    "artist from the library every five seconds"
                );
            }
        }
        
        public override string [] Authors {
            get {
                return new string [] {
                    "Peter Griffin"
                };
            }
        }
 
        // --------------------------------------------------------------- //
        
        private uint timeout_id;
 
        protected override void PluginInitialize()
        {
            Console.WriteLine("Initializing Sample Plugin");
            timeout_id = GLib.Timeout.Add(5000, OnTimeout);
        }
        
        // optional, this is a virtual override, only
        // only provide an implementation if there are
        // resources to dispose of or other objects that
        // need notification, etc.
        protected override void PluginDispose()
        {
            Console.WriteLine("Disposing Sample Plugin");
            GLib.Source.Remove(timeout_id);
            timeout_id = 0;
        }
        
        // optional, this is a virtual override, only 
        // provide an implementation if there is a 
        // configuration GUI to show
        public override void ShowConfigurationDialog()
        {
            LogCore.Instance.PushWarning("Present a Gtk.Dialog here",
                "Also, Banshee.Base.LogCore.Instance is available for " +
                "debugging and showing warnings and errors");
        }
        
        private bool OnTimeout()
        {
            int track_id = Convert.ToInt32(Globals.Library.Db.QuerySingle(
                "SELECT TrackID FROM Tracks ORDER BY RANDOM() LIMIT 1"));
            Console.WriteLine(Globals.Library.GetTrack(track_id));
            return true;
        }
    }
}
