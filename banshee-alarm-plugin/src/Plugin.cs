using System;
using System.Threading;
using Mono.Unix;
using Gtk;
 
using Banshee.Base;
using Banshee.MediaEngine;

namespace Banshee.Plugins.Alarm
{
	public class AlarmPlugin : Banshee.Plugins.Plugin
	{
		protected override string ConfigurationName { get { return "Alarm"; } }
		public override string DisplayName { get { return "Alarm & Sleep Timer"; } }

		public override string Description {
			get {
				return Catalog.GetString(
					"A very simple Banshee plugin that starts playing " +
					"at a predefined time, " +
					"and allows you to set a sleep timer."
				);
			}
		}

		public override string [] Authors {
			get {
				return new string [] {
					"Bertrand Lorentz <bertrand.lorentz@free.fr>\n" +
					"Patrick van Staveren <trick@vanstaveren.us>"
				};
			}
		}

		// --------------------------------------------------------------- //

		private Thread alarmThread;
		private static AlarmPlugin thePlugin;
		private SpinButton sleepHour = new SpinButton(0,23,1);
		private SpinButton sleepMin  = new SpinButton(0,59,1);
		public Window alarmDialog;
		private int timervalue;
		private Menu editMenu;
		private MenuItem menuItemSleep;
		private MenuItem menuItemAlarm;
		uint sleep_timer_id;

		protected override void PluginInitialize()
		{
			LogCore.Instance.PushDebug("Initializing Alarm Plugin", "");

			RegisterConfigurationKey("AlarmHour");
			RegisterConfigurationKey("AlarmMinute");
			RegisterConfigurationKey("FadeStartVolume");
			RegisterConfigurationKey("FadeEndVolume");
			RegisterConfigurationKey("FadeDuration");

			AlarmPlugin.thePlugin = this;
			ThreadStart alarmThreadStart = new ThreadStart(AlarmPlugin.DoWait);
			this.alarmThread = new Thread(alarmThreadStart);
			alarmThread.Start();

			editMenu = (Globals.ActionManager.GetWidget("/MainMenu/EditMenu") as MenuItem).Submenu as Menu;

			SeparatorMenuItem separator = new SeparatorMenuItem();
			editMenu.Insert(separator, 10);
			separator.Show();

			menuItemSleep = new MenuItem(Catalog.GetString("Sleep Timer..."));
			menuItemSleep.Activated += new EventHandler(DoSleepTimerConfigDialog);
			editMenu.Insert(menuItemSleep, 11);
			menuItemSleep.Show();
			
			menuItemAlarm = new MenuItem(Catalog.GetString("Alarm..."));
			menuItemAlarm.Activated += new EventHandler(DoAlarmConfigDialog);
			editMenu.Insert(menuItemAlarm, 12);
			menuItemAlarm.Show();
		}
        
		protected override void PluginDispose()
		{
			LogCore.Instance.PushDebug("Disposing Alarm Plugin", "");
			if(sleep_timer_id > 0){
				GLib.Source.Remove(sleep_timer_id);
				LogCore.Instance.PushDebug("Disabling old sleep timer", "");
			}
			alarmThread.Abort();
		}

		public override Gtk.Widget GetConfigurationWidget()
		{
			return new Label("put volume config stuff here.\nalarm and sleep set will be in Edit menu now.");
			//return new AlarmConfigPage(this);    
		}
        
		public static void DoWait()
		{
			LogCore.Instance.PushDebug("Alarm thread started", "");

			AlarmThread theAlarm = new AlarmThread(AlarmPlugin.thePlugin);
			theAlarm.MainLoop();
		}

		protected void DoAlarmConfigDialog(object o, EventArgs a)
		{
			alarmDialog = new Window("Set Alarm");
			alarmDialog.DeleteEvent += new DeleteEventHandler(OnSleepTimerDialogDestroy);
			alarmDialog.Add(new AlarmConfigPage(this));
			alarmDialog.ShowAll();
		}

		protected void DoSleepTimerConfigDialog(object o, EventArgs a)
		{
			if(sleep_timer_id > 0){
				GLib.Source.Remove(sleep_timer_id);
				LogCore.Instance.PushDebug("Disabling old sleep timer", "");
			}

			sleepHour.Value = (int) timervalue / 60 ;
			sleepMin.Value = timervalue - (sleepHour.Value * 60);

			alarmDialog = new Window("Set Sleep Timer");
			alarmDialog.DeleteEvent += new DeleteEventHandler(OnSleepTimerDialogDestroy);
			sleepHour.WidthChars = 3;
			sleepMin.WidthChars  = 3;

			Label prefix    = new Label("Sleep Timer :");
			Label separator = new Label(":");
			Label comment   = new Label("<i>(set to 0:00 to disable)</i>");
			comment.UseMarkup = true;

			Button OK       = new Button("OK");
			VButtonBox OKbox = new VButtonBox();
			OKbox.PackStart(OK, false, false, 0);
			OK.Clicked += new EventHandler(OnSleepTimerOK);

			HBox topbox     = new HBox();
			VBox mainbox    = new VBox();

			topbox.PackStart(prefix, false, false, 3);
			topbox.PackStart(sleepHour, false, false, 3);
			topbox.PackStart(separator, false, false, 0);
			topbox.PackStart(sleepMin, false, false, 3);

			mainbox.PackStart(topbox, false, false, 3);
			mainbox.PackStart(comment, false, false, 3);
			mainbox.PackStart(new HSeparator(), false, false, 3);
			mainbox.PackStart(OKbox, false, false, 3);

			alarmDialog.Add(mainbox);

			alarmDialog.ShowAll();
		}
       	
		void OnSleepTimerOK(object o, EventArgs a)
		{
			alarmDialog.Destroy();
			SetSleepTimer();
		}

		void SetSleepTimer()
		{
			timervalue = sleepHour.ValueAsInt * 60 + sleepMin.ValueAsInt;
			if(timervalue != 0) {
				Console.WriteLine("Sleep Timer set to {0}", timervalue);
				sleep_timer_id = GLib.Timeout.Add((uint) timervalue * 60 * 1000, onSleepTimerActivate);
			}
		
		}

		void OnSleepTimerDialogDestroy(object o, DeleteEventArgs a)
		{
			SetSleepTimer();
		}

		public bool onSleepTimerActivate()
		{
			LogCore.Instance.PushDebug("Sleep Timer has gone off - pausing", "");
			PlayerEngineCore.Pause();
			PlayerEngineCore.Position = 0;

			return(false);
		}


		#region Configuration properties
		internal ushort AlarmHour
		{
			get {
				try {
					return ushort.Parse(Globals.Configuration.Get(ConfigurationKeys["AlarmHour"]).ToString());
				} catch {
					return 0;
				}
			}

			set {
				Globals.Configuration.Set(ConfigurationKeys["AlarmHour"], (int)value);
			}
		}

		internal ushort AlarmMinute
		{
			get {
				try {
					return ushort.Parse(Globals.Configuration.Get(ConfigurationKeys["AlarmMinute"]).ToString());
				} catch {
					return 0;
				}
			}

			set {
				Globals.Configuration.Set(ConfigurationKeys["AlarmMinute"], (int)value);
			}
		}

		internal ushort FadeStartVolume
		{
			get {
				try {
					return ushort.Parse(Globals.Configuration.Get(ConfigurationKeys["FadeStartVolume"]).ToString());
				} catch {
					return 0;
				}
			}

			set {
				Globals.Configuration.Set(ConfigurationKeys["FadeStartVolume"], (int)value);
			}
		}

		internal ushort FadeEndVolume
		{
			get {
				try {
					return ushort.Parse(Globals.Configuration.Get(ConfigurationKeys["FadeEndVolume"]).ToString());
				} catch {
					return 100;
			}
		}

			set {
				Globals.Configuration.Set(ConfigurationKeys["FadeEndVolume"], (int)value);
			}
		}

		internal ushort FadeDuration
		{
			get {
				try {
					return ushort.Parse(Globals.Configuration.Get(ConfigurationKeys["FadeDuration"]).ToString());
				} catch {
					return 0;
				}
			}

			set {
				Globals.Configuration.Set(ConfigurationKeys["FadeDuration"], (int)value);
			}
		}
		#endregion
	}
}
