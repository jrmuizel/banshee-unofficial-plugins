using System;
using Gtk;

using Banshee.Base;

namespace Banshee.Plugins.Alarm 
{
	public class SleepTimerConfigDialog : Window 
	{
		AlarmPlugin plugin;
		
		public SleepTimerConfigDialog(AlarmPlugin plugin) : base("Sleep Timer")
		{
			this.plugin = plugin;
			BuildWidget();
		}
		
		private void BuildWidget()
		{
			plugin.sleepHour.Value = (int) plugin.timervalue / 60 ;
			plugin.sleepMin.Value = plugin.timervalue - (plugin.sleepHour.Value * 60);

			this.DeleteEvent += new DeleteEventHandler(OnSleepTimerDialogDestroy);
			plugin.sleepHour.WidthChars = 3;
			plugin.sleepMin.WidthChars  = 3;

			Label prefix    = new Label("Sleep Timer :");
			Label separator = new Label(":");
			Label comment   = new Label("<i>(set to 0:00 to disable)</i>");
			comment.UseMarkup = true;

			Button OK = new Button(Gtk.Stock.Ok);
			VButtonBox OKbox = new VButtonBox();
			OKbox.PackStart(OK, false, false, 0);
			OK.Clicked += new EventHandler(OnSleepTimerOK);

			HBox topbox     = new HBox();
			VBox mainbox    = new VBox();

			topbox.PackStart(prefix, false, false, 3);
			topbox.PackStart(plugin.sleepHour, false, false, 3);
			topbox.PackStart(separator, false, false, 0);
			topbox.PackStart(plugin.sleepMin, false, false, 3);

			mainbox.PackStart(topbox, false, false, 3);
			mainbox.PackStart(comment, false, false, 3);
			mainbox.PackStart(new HSeparator(), false, false, 3);
			mainbox.PackStart(OKbox, false, false, 3);

			this.Add(mainbox);

			this.ShowAll();
		}
		
		private void OnSleepTimerDialogDestroy(object o, DeleteEventArgs a){
			plugin.SetSleepTimer();
		}

		public void OnSleepTimerOK(object o, EventArgs a)
		{
			this.Destroy();
			plugin.SetSleepTimer();
		}
	}
}
