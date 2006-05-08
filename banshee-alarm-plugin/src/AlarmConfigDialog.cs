using System;
using Gtk;
using GConf;
using Mono.Unix;

using Banshee.Base;
using Banshee.Widgets;

namespace Banshee.Plugins.Alarm 
{
	public class AlarmConfigDialog : Window  
	{
		private AlarmPlugin plugin;
		
		public AlarmConfigDialog(AlarmPlugin plugin) : base("Alarm")
		{
			this.plugin = plugin;
			BuildWidget();
		}

		private void BuildWidget()
		{
			SetPosition(WindowPosition.Center);
            IconThemeUtils.SetWindowIcon(this);
            
            SpinButton spbHour = new SpinButton(0, 23, 1);
			spbHour.WidthChars = 2;
			SpinButton spbMinute = new SpinButton(0, 59, 1);
			spbMinute.WidthChars = 2;

			CheckButton isEnabled = new CheckButton("Enabled");

			HBox time_box = new HBox();
			time_box.PackStart(new Label("Time: "));
			time_box.PackStart(spbHour);
			time_box.PackStart(new Label(" : "));
			time_box.PackStart(spbMinute);

			VBox time_box_outer = new VBox();
			time_box_outer.PackStart(isEnabled);
			time_box_outer.PackStart(time_box);

			Frame TimeBoxFrame = new Frame("Set Alarm Time");
			TimeBoxFrame.Add(time_box_outer);

			HButtonBox OKButtonBox = new HButtonBox();
			Button OK = new Button(Gtk.Stock.Ok);
			OK.Clicked += new EventHandler(OnOKClicked);
			OKButtonBox.PackStart(OK);

            VBox mainbox = new VBox();
            mainbox.Spacing = 10;
			
			mainbox.PackStart(TimeBoxFrame, false, false, 2);
			mainbox.PackStart(OKButtonBox, false, false, 2);
			
			this.Add(mainbox);

			#region Initialize with current values
			spbHour.Value = plugin.AlarmHour;
			spbMinute.Value = plugin.AlarmMinute;
			isEnabled.Active = plugin.AlarmEnabled;
			#endregion

			isEnabled.Toggled += new EventHandler(AlarmEnabled_Changed);
			spbHour.ValueChanged += new EventHandler(AlarmHour_Changed);
			spbMinute.ValueChanged += new EventHandler(AlarmMinute_Changed);

			ShowAll();
		}

		private void OnOKClicked(object o, EventArgs e) {
			this.Destroy();
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
	}
}
