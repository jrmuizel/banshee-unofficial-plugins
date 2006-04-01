using System;
using Gtk;

using Banshee.Base;

namespace Banshee.Plugins.Scope
{
    public class ScopeView : Gtk.DrawingArea
    {
        private double [] levels;
    
        public ScopeView()
        {
            AddEvents((int)Gdk.EventMask.ExposureMask);
        }
        
        public void PushLevels(double [] levels)
        {
            ThreadAssist.ProxyToMain(delegate { PushLevelsSafe(levels); });
        }
        
        private void PushLevelsSafe(double [] levels)
        {
            if(this.levels == null) {
                this.levels = levels;
                QueueDraw();
            } else {
                lock(this.levels) {
                    this.levels = levels;
                }
                
                QueueDraw();
            }
        }
        
        protected override bool OnExposeEvent(Gdk.EventExpose evnt)
        {
            if(this.levels == null) {
                return base.OnExposeEvent(evnt);
            }
        
            lock(this.levels) {
                foreach(Gdk.Rectangle rect in evnt.Region.GetRectangles()) {
                    DrawRegion(rect);
                }
            }
            
            return base.OnExposeEvent(evnt);
        }
        
        private void DrawRegion(Gdk.Rectangle rect)
        {
            int bar_spacing = 1;
            int bar_width = (int)Math.Round((double)Allocation.Width / (double)levels.Length) - bar_spacing;
            
            for(int i = 0; i < levels.Length; i++) {
                int bar_height = (int)((double)Allocation.Height * levels[i]);
                int bar_x = (i * bar_width) + (i * bar_spacing);
                int bar_y = Allocation.Height - bar_height;
                
                Style.PaintFlatBox(Style, GdkWindow, StateType.Selected, ShadowType.None, rect,
                    this, "bar", bar_x, bar_y, bar_width, bar_height);
            }
        }
        
        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);
        }

        protected override void OnRealized()
        {
            base.OnRealized();
        }
    }
}
