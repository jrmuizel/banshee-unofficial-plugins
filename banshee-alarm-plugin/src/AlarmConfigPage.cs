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
		private CheckButton isEnabled;
		private VScale FadeStartScale;
		private VScale FadeEndScale;
		private SpinButton FadeDuration;

		public AlarmConfigPage(AlarmPlugin plugin) : base()
		{
			this.plugin = plugin;
			BuildWidget();
		}

		private void BuildWidget()
		{
			Spacing = 10;
			int volumeSliderHeight = 120;
			spbHour = new SpinButton(0, 23, 1);
			spbHour.WidthChars = 2;
			spbMinute = new SpinButton(0, 59, 1);
			spbMinute.WidthChars = 2;

			isEnabled = new CheckButton("Enabled");

			HBox time_box = new HBox();
			time_box.PackStart(new Label("Time: "));
			time_box.PackStart(this.spbHour);
			time_box.PackStart(new Label(" : "));
			time_box.PackStart(this.spbMinute);

			VBox time_box_outer = new VBox();
			time_box_outer.PackStart(isEnabled);
			time_box_outer.PackStart(time_box);

			FadeStartScale = new VScale(0, 100, 1);
			FadeStartScale.Inverted = true;
			FadeStartScale.HeightRequest = volumeSliderHeight;
			FadeEndScale = new VScale(0, 100, 1);
			FadeEndScale.Inverted = true;
			FadeEndScale.HeightRequest = volumeSliderHeight;
			FadeDuration = new SpinButton(0, 65535, 1);
			FadeDuration.WidthChars = 3;

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
			TimeBoxFrame.Add(time_box_outer);

			HButtonBox OKButtonBox = new HButtonBox();
			Button OK = new Button(Gtk.Stock.Ok);
			OK.Clicked += new EventHandler(OnOKClicked);
			OKButtonBox.PackStart(OK);

			PackStart(TimeBoxFrame, false, false, 2);
			PackStart(FadeBoxFrame, false, false, 2);
			PackStart(OKButtonBox, false, false, 2);

			#region Initialize with current values
			spbHour.Value = plugin.AlarmHour;
			spbMinute.Value = plugin.AlarmMinute;
			FadeStartScale.Value = plugin.FadeStartVolume;
			FadeEndScale.Value = plugin.FadeEndVolume;
			FadeDuration.Value = plugin.FadeDuration;
			isEnabled.Active = plugin.AlarmEnabled;
			#endregion

			isEnabled.Toggled += new EventHandler(AlarmEnabled_Changed);
			spbHour.ValueChanged += new EventHandler(AlarmHour_Changed);
			spbMinute.ValueChanged += new EventHandler(AlarmMinute_Changed);
			FadeStartScale.ValueChanged += new EventHandler(FadeStartVolume_Changed);
			FadeEndScale.ValueChanged += new EventHandler(FadeEndVolume_Changed);
			FadeDuration.ValueChanged += new EventHandler(FadeDuration_Changed);

			ShowAll();
		}

		private void OnOKClicked(object o, EventArgs e) {
			plugin.alarmDialog.Destroy();
		}

		private void AlarmEnabled_Changed(object source, System.EventArgs args) {
			CheckButton button = source as CheckButton;
			plugin.AlarmEnabled = button.Active;
		}

		private void AlarmHour_Changed(object source, System.EventArgs args) {
			SpinButton spinner = source as SpinButton;
			plugin.AlarmHour = (ushort) spinner.Value;
		}

		private void AlarmMinute_Changed(object source, System.EventArgs args) {
			SpinButton spinner = source as SpinButton;
			plugin.AlarmMinute = (ushort) spinner.Value;
		}

		private void FadeStartVolume_Changed(object source, System.EventArgs args) {
			VScale scale = source as VScale;
			plugin.FadeStartVolume = (ushort)scale.Value;
		}

		private void FadeEndVolume_Changed(object source, System.EventArgs args) {
			VScale scale = source as VScale;
			plugin.FadeEndVolume = (ushort)scale.Value;
		}

		private void FadeDuration_Changed(object source, System.EventArgs args) {
			SpinButton spinner = source as SpinButton;
			plugin.FadeDuration = (ushort)spinner.Value;
		}
	}
}
