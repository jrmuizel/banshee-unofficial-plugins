// created on 03/23/2006 at 16:32


using System;
using Gtk;
using GConf;
using Mono.Unix;

using Banshee.Base;
using Banshee.Widgets;

namespace Banshee.Plugins.Alarm 
{
    public class AlarmConfigPage : VBox
    {
        private AlarmPlugin plugin;
        private SpinButton spbHour;
        private SpinButton spbMinute;
        
        public AlarmConfigPage(AlarmPlugin plugin) : base()
        {
            this.plugin = plugin;
            BuildWidget();
        }
        
        private void BuildWidget()
        {
            this.Spacing = 10;
            
            this.spbHour = new SpinButton(0, 23, 1);
            this.spbHour.WidthChars = 2;
            this.spbMinute = new SpinButton(0, 59, 1);
            this.spbMinute.WidthChars = 2;
            
            this.spbHour.ValueChanged += new EventHandler(Hour_Changed);
            this.spbMinute.ValueChanged += new EventHandler(Minute_Changed);
            
            HBox time_box = new HBox();
            time_box.PackStart(new Label("Time: "));
            time_box.PackStart(this.spbHour);
            time_box.PackStart(this.spbMinute);
            
            SpinButton spbFadeStartVolume = new SpinButton(0, 100, 1);
            spbFadeStartVolume.WidthChars = 3;
            SpinButton spbFadeEndVolume = new SpinButton(0, 100, 1);
            spbFadeEndVolume.WidthChars = 3;
            SpinButton spbFadeDuration = new SpinButton(0, 100, 1);
            spbFadeDuration.WidthChars = 3;
            
            spbFadeStartVolume.ValueChanged += new EventHandler(FadeStartVolume_Changed);
            spbFadeEndVolume.ValueChanged += new EventHandler(FadeEndVolume_Changed);
            spbFadeDuration.ValueChanged += new EventHandler(FadeDuration_Changed);
            
            HBox fade_box = new HBox();
            fade_box.PackStart(new Label("Start Volume: "));
            fade_box.PackStart(spbFadeStartVolume);
            fade_box.PackStart(new Label("End Volume: "));
            fade_box.PackStart(spbFadeEndVolume);
            fade_box.PackStart(new Label("Duration: "));
            fade_box.PackStart(spbFadeDuration);
            
            Frame fade_box_frame = new Frame("Fade-in Adjustment (BETA)");
            fade_box_frame.Add(fade_box);
            
            Frame time_box_frame = new Frame("Set Alarm Time");
            time_box_frame.Add(time_box);
            
            HButtonBox button_box = new HButtonBox();
				Button OK = new Button("OK");
				OK.Clicked += new EventHandler(OnOKClicked);
				button_box.PackStart(OK);
				
				this.PackStart(time_box_frame, false, false, 2);
            this.PackStart(fade_box_frame, false, false, 2);
				this.PackStart(button_box, false, false, 2);
				
            ShowAll();
            
            #region Initialize with current values
            this.spbHour.Value = this.plugin.AlarmHour;
            this.spbMinute.Value = this.plugin.AlarmMinute;
            spbFadeStartVolume.Value = this.plugin.FadeStartVolume;
            spbFadeEndVolume.Value = this.plugin.FadeEndVolume;
            spbFadeDuration.Value = this.plugin.FadeDuration;
            #endregion
        }
        
        private void OnOKClicked(object o, EventArgs e){
        	plugin.alarmDialog.Destroy();
        }

        private void Hour_Changed(object source, System.EventArgs args)
		{
		    SpinButton spinner = source as SpinButton;
		    LogCore.Instance.PushDebug("Current hour is: " + spinner.ValueAsInt, "");
		    this.plugin.AlarmHour = (ushort)spinner.ValueAsInt;
		}

        private void Minute_Changed(object source, System.EventArgs args)
		{
		    SpinButton spinner = source as SpinButton;
		    LogCore.Instance.PushDebug("Current minute is: " + spinner.ValueAsInt, "");
		    this.plugin.AlarmMinute = (ushort)spinner.ValueAsInt;
		}

        private void FadeStartVolume_Changed(object source, System.EventArgs args)
		{
		    SpinButton spinner = source as SpinButton;
		    LogCore.Instance.PushDebug("Current FadeStartVolume is: " + spinner.ValueAsInt, "");
		    this.plugin.FadeStartVolume = (ushort)spinner.ValueAsInt;
		}

        private void FadeEndVolume_Changed(object source, System.EventArgs args)
		{
		    SpinButton spinner = source as SpinButton;
		    LogCore.Instance.PushDebug("Current FadeEndVolume is: " + spinner.ValueAsInt, "");
		    this.plugin.FadeEndVolume = (ushort)spinner.ValueAsInt;
		}

        private void FadeDuration_Changed(object source, System.EventArgs args)
		{
		    SpinButton spinner = source as SpinButton;
		    LogCore.Instance.PushDebug("Current FadeDuration is: " + spinner.ValueAsInt, "");
		    this.plugin.FadeDuration = (ushort)spinner.ValueAsInt;
		}
    }
}