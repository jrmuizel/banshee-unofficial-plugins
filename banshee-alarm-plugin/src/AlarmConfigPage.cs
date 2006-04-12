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
            int volumeSliderHeight = 120;
            this.spbHour = new SpinButton(0, 23, 1);
            this.spbHour.WidthChars = 2;
            this.spbMinute = new SpinButton(0, 59, 1);
            this.spbMinute.WidthChars = 2;
            
            this.spbHour.ValueChanged += new EventHandler(Hour_Changed);
            this.spbMinute.ValueChanged += new EventHandler(Minute_Changed);
            
            HBox time_box = new HBox();
            time_box.PackStart(new Label("Time: "));
            time_box.PackStart(this.spbHour);
            time_box.PackStart(new Label(" : "));
            time_box.PackStart(this.spbMinute);
            
            VScale FadeStartScale = new VScale(0, 100, 1);
            FadeStartScale.Inverted = true;
            FadeStartScale.HeightRequest = volumeSliderHeight;
				VScale FadeEndScale = new VScale(0, 100, 1);
				FadeEndScale.Inverted = true;
				FadeEndScale.HeightRequest = volumeSliderHeight;
            SpinButton FadeDuration = new SpinButton(0, 65535, 1);
            FadeDuration.WidthChars = 3;
            
				FadeStartScale.ValueChanged += new EventHandler(FadeStartVolume_Changed);
            FadeEndScale.ValueChanged += new EventHandler(FadeEndVolume_Changed);
            FadeDuration.ValueChanged += new EventHandler(FadeDuration_Changed);
            
            VBox FaderBigBox = new VBox();
            
            VBox StartBox = new VBox();
            StartBox.PackEnd(new Label("Start"));
            StartBox.PackStart(FadeStartScale);

            VBox EndBox = new VBox();
            EndBox.PackEnd(new Label("End"));
            EndBox.PackStart(FadeEndScale);

				HBox FaderBoxes = new HBox();
				FaderBoxes.PackStart(StartBox);
				FaderBoxes.PackStart(EndBox);
            
            Label labelVolume = new Label("<b>Volume</b>");
            labelVolume.UseMarkup = true;
            FaderBigBox.PackStart(labelVolume);
            FaderBigBox.PackStart(FaderBoxes);
            Label labelDuration = new Label("<b>Duration</b> <i>(seconds)</i>");
            labelDuration.UseMarkup = true;
            FaderBigBox.PackStart(labelDuration);
            FaderBigBox.PackStart(FadeDuration);
            
            Frame FadeBoxFrame = new Frame("Fade-in Adjustment");
            FadeBoxFrame.Add(FaderBigBox);
            
            Frame TimeBoxFrame = new Frame("Set Alarm Time");
            TimeBoxFrame.Add(time_box);
            
            HButtonBox OKButtonBox = new HButtonBox();
				Button OK = new Button(Gtk.Stock.Ok);
				OK.Clicked += new EventHandler(OnOKClicked);
				OKButtonBox.PackStart(OK);
				
				PackStart(TimeBoxFrame, false, false, 2);
            PackStart(FadeBoxFrame, false, false, 2);
				PackStart(OKButtonBox, false, false, 2);
				
            ShowAll();
            
            #region Initialize with current values
            this.spbHour.Value = this.plugin.AlarmHour;
            this.spbMinute.Value = this.plugin.AlarmMinute;
            FadeStartScale.Value = plugin.FadeStartVolume;
            FadeEndScale.Value = plugin.FadeEndVolume;
            FadeDuration.Value = plugin.FadeDuration;
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
		    VScale scale = source as VScale;
		    LogCore.Instance.PushDebug("Current FadeStartVolume is: " + (int) scale.Value, "");
		    plugin.FadeStartVolume = (ushort)scale.Value;
		}

        private void FadeEndVolume_Changed(object source, System.EventArgs args)
		{
		    VScale scale = source as VScale;
		    LogCore.Instance.PushDebug("Current FadeEndVolume is: " + (int) scale.Value, "");
		    plugin.FadeEndVolume = (ushort)scale.Value;
		}

        private void FadeDuration_Changed(object source, System.EventArgs args)
		{
		    SpinButton spinner = source as SpinButton;
		    LogCore.Instance.PushDebug("Current FadeDuration is: " + spinner.ValueAsInt, "");
		    plugin.FadeDuration = (ushort)spinner.Value;
		}
    }
}
