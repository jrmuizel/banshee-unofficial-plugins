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
                    "Gives Banshee alarm clock type functions.  Provides an alarm " +
                    "which can start playback at a predefined time, " +
                    "and allows you to set a sleep timer to pause playback after a set delay."
                );
            }
        }

        public override string [] Authors {
            get {
                return new string [] {
                    "Bertrand Lorentz\n" +
                    "Patrick van Staveren"
                };
            }
        }

        // --------------------------------------------------------------- //

        private Thread alarmThread;
        private static AlarmPlugin thePlugin;
        public SpinButton sleepHour = new SpinButton(0,23,1);
        public SpinButton sleepMin  = new SpinButton(0,59,1);
        public Window alarmDialog;
        public int timervalue;
        private Menu editMenu;
        private MenuItem menuItemSleep;
        private MenuItem menuItemAlarm;
        private SeparatorMenuItem menuItemSeparator;
        uint sleep_timer_id;

        protected override void PluginInitialize()
        {
            LogCore.Instance.PushDebug("Initializing Alarm Plugin", "");

            RegisterConfigurationKey("AlarmEnabled");
            RegisterConfigurationKey("AlarmHour");
            RegisterConfigurationKey("AlarmMinute");
            RegisterConfigurationKey("AlarmCommand");
            RegisterConfigurationKey("FadeStartVolume");
            RegisterConfigurationKey("FadeEndVolume");
            RegisterConfigurationKey("FadeDuration");

            AlarmPlugin.thePlugin = this;
            ThreadStart alarmThreadStart = new ThreadStart(AlarmPlugin.DoWait);
            alarmThread = new Thread(alarmThreadStart);
            alarmThread.Start();
        }

        protected override void InterfaceInitialize()
        {
            editMenu = (Globals.ActionManager.GetWidget("/MainMenu/EditMenu") as MenuItem).Submenu as Menu;

            menuItemSeparator = new SeparatorMenuItem();
            editMenu.Insert(menuItemSeparator, 10);
            menuItemSeparator.Show();

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
            editMenu.Remove(menuItemSleep);
            editMenu.Remove(menuItemAlarm);
            editMenu.Remove(menuItemSeparator);
            LogCore.Instance.PushDebug("Disposing Alarm Plugin", "");
            if(sleep_timer_id > 0){
                GLib.Source.Remove(sleep_timer_id);
                LogCore.Instance.PushDebug("Disabling old sleep timer", "");
            }
            alarmThread.Abort();
        }

        public override Gtk.Widget GetConfigurationWidget()
        {
            return new ConfigurationWidget(this);
        }
        
        public static void DoWait()
        {
            LogCore.Instance.PushDebug("Alarm thread started", "");
            AlarmThread theAlarm = new AlarmThread(AlarmPlugin.thePlugin);
            theAlarm.MainLoop();
        }

        protected void DoAlarmConfigDialog(object o, EventArgs a)
        {
            new AlarmConfigDialog(this);
        }

        protected void DoSleepTimerConfigDialog(object o, EventArgs a)
        {
            if(sleep_timer_id > 0){
                GLib.Source.Remove(sleep_timer_id);
                LogCore.Instance.PushDebug("Disabling old sleep timer", "");
            }
            new SleepTimerConfigDialog(this);
        }
           
        public void SetSleepTimer()
        {
            timervalue = sleepHour.ValueAsInt * 60 + sleepMin.ValueAsInt;
            if(timervalue != 0) {
                Console.WriteLine("Sleep Timer set to {0}", timervalue);
                sleep_timer_id = GLib.Timeout.Add((uint) timervalue * 60 * 1000, onSleepTimerActivate);
            }
        }

        public bool onSleepTimerActivate()
        {
            if(PlayerEngineCore.CurrentState == PlayerEngineState.Playing){
                LogCore.Instance.PushDebug("Sleep Timer has gone off.  Fading out till end of song.", "");
                new VolumeFade(PlayerEngineCore.Volume, 0,
                        (ushort) (PlayerEngineCore.Length - PlayerEngineCore.Position));
                GLib.Timeout.Add((PlayerEngineCore.Length - PlayerEngineCore.Position) * 1000, delegate{
                    LogCore.Instance.PushDebug("Sleep Timer: Pausing.", "");
                    PlayerEngineCore.Pause();
                    return false;
                    }
                );
                
            }else{
                LogCore.Instance.PushDebug("Sleep Timer has gone off, but we're not playing.  Refusing to pause.", "");
            }
            return(false);
        }
        
        #region Configuration properties
        internal bool AlarmEnabled
        {
            get {
                try {
                    return bool.Parse(Globals.Configuration.Get(ConfigurationKeys["AlarmEnabled"]).ToString());
                } catch {
                    return false;
                }
            }

            set {
                Globals.Configuration.Set(ConfigurationKeys["AlarmEnabled"], value);
            }
        }

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

        internal string AlarmCommand
        {
            get {
                try {
                    return Globals.Configuration.Get(ConfigurationKeys["AlarmCommand"]).ToString();
                } catch {
                    return null;
                }
            }

            set {
                Globals.Configuration.Set(ConfigurationKeys["AlarmCommand"], value);
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
