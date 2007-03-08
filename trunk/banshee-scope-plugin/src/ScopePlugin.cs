using System;
using System.Runtime.InteropServices;
using Mono.Unix;
using Gtk;

using Banshee.Base;
using Banshee.MediaEngine;
 
namespace Banshee.Plugins.Scope
{
    public class ScopePlugin : Banshee.Plugins.Plugin
    {
        protected override string ConfigurationName { get { return "Scope"; } }
        public override string DisplayName { get { return "Scope Visualizer"; } }
        
        public override string Description {
            get { return Catalog.GetString("Provides a simple scope visualizer"); }
        }
        
        public override string [] Authors {
            get { return new string [] { "Aaron Bockover" }; }
        }
        
        private HandleRef scope_parser_handle;
        private ScopeView scope_view;
        private Window scope_window;
        private uint poll_timeout;
        
        protected override void PluginInitialize()
        {
            IntPtr tee_raw = IntPtr.Zero;
            
            foreach(PlayerEngine engine in PlayerEngineCore.Engines) {
                if(engine.Id != "gstreamer") {
                    continue;
                }
                
                IntPtr [] pipeline_elements = engine.GetBaseElements();
                if(pipeline_elements == null || pipeline_elements.Length < 3) {
                    continue;
                }
                
                tee_raw = pipeline_elements[2];
            }
            
            if(tee_raw == IntPtr.Zero) {
                throw new ApplicationException("GStreamer 0.10 engine not loaded");
            }
            
            IntPtr scope_parser_raw = scope_parser_new();
            if(scope_parser_raw == IntPtr.Zero) {
                throw new ApplicationException("Could not create a ScopeParser");
            }
            
            scope_parser_handle = new HandleRef(this, scope_parser_raw);

            if(!scope_parser_attach_to_tee(scope_parser_handle, tee_raw)) {
                scope_parser_free(scope_parser_handle);
                throw new ApplicationException("Could not link into pipeline");
            }
            
            if(Globals.UIManager.IsInitialized) {
                CreateScopeWindow();
            }
        }
        
        protected override void InterfaceInitialize()
        {
            CreateScopeWindow();
        }
        
        protected override void PluginDispose()
        {
            if(scope_window != null) {
                scope_window.Destroy();
            }
            
            if(poll_timeout > 0) {
                GLib.Source.Remove(poll_timeout);
            }
            
            scope_parser_free(scope_parser_handle);
        }

        private void CreateScopeWindow()
        {
            if(scope_window != null) {
                return;
            }
        
            scope_window = new Window("Scope");
            scope_window.DeleteEvent += delegate {
                scope_window.Destroy();
                scope_window = null;
                GLib.Source.Remove(poll_timeout);
                poll_timeout = 0;
            };
            
            scope_view = new ScopeView();
            scope_window.Add(scope_view);
            scope_window.SetSizeRequest(150, 80);
            scope_window.ShowAll();
            
            poll_timeout = GLib.Timeout.Add(150, PollScope);
        }

        private bool PollScope()
        {
            if(scope_window == null) {
                return false;
            }

            IntPtr raw_scope = scope_parser_poll_scope(scope_parser_handle);
            if(raw_scope == IntPtr.Zero) {
                return true;
            }
            
            int scope_resolution = 40;
            int sample_size = 256; // 512, but we ignore the upper 256
            int spread = sample_size / scope_resolution;
            
            double [] scaled_scope = new double[scope_resolution];

            for(int i = 0; i < scope_resolution; i++) {
                if(i * spread >= 512) {
                    break;
                }
                
                int peak = Marshal.ReadInt16(raw_scope, i * spread);
                scaled_scope[i] = (double)peak / 7500;
            }
            
            scope_view.PushLevels(scaled_scope);
            return true;
        }
        
        [DllImport("libscopeplugin")]
        private static extern IntPtr scope_parser_new();
        
        [DllImport("libscopeplugin")]
        private static extern void scope_parser_free(HandleRef handle);

        [DllImport("libscopeplugin")]
        private static extern bool scope_parser_attach_to_tee(HandleRef handle, IntPtr tee);
        
        [DllImport("libscopeplugin")]
        private static extern IntPtr scope_parser_poll_scope(HandleRef handle);
    }
}
