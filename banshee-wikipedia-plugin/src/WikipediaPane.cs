using System;
using System.Text;
using Mono;
using Mono.Posix;
using Gtk;
using Gecko;
using GLib;
using Banshee;
using Banshee.Base;
using Gdk;

namespace Banshee.Plugins.Wikipedia
{
	public class WikipediaPane : Frame
	{
		private WebControl web;
		private Viewport win;
		private Statusbar sb;
		private VBox main;
		private HBox bot;
		private ProgressBar pb;

		private int bot_queue_length;
		public string current_artist;
		
		public WikipediaPane () {
			Visible = false;

			web = new WebControl("about:blank", "Gecko");
			win = new Viewport();
			sb = new Statusbar();
			pb = new ProgressBar();
			main = new VBox();
			bot = new HBox();
			
			bot_queue_length = 0;

			sb.TextPushed += new Gtk.TextPushedHandler (StatusbarDisplay);
			sb.Visible = false;
			sb.HasResizeGrip = false;

			pb.WidthRequest = 300;


			//web.ProgressAll += new Gecko.ProgressAllHandler(updateProgress);
			web.LinkMsg += new EventHandler (LinkMessage);
			
			
			bot.PackStart(sb, true, true, 0);
			//bot.PackEnd(pb, false, false, 0);
			
			main.PackStart(web, true, true, 0);
			main.PackEnd(bot, false, false, 0);
			
			win.Add(main);
			Add(win);

			ShowAll();
		}
		
		public void StatusbarDisplay (object o, TextPushedArgs args) {
			bot.Visible = true;
			bot_queue_length++;
			GLib.Timeout.Add(5000, delegate {
				bot_queue_length--;
				sb.Pop(1);
				if(bot_queue_length < 1) {
					bot.Visible = false;
					pb.Visible = false;
				}
				return false;
			});
		}
		
		/*public void ProgressbarDisplay (object o, ProgressAllArgs args) {
			bot.Visible = true;
			bot.PackEnd(pb);
			pb.Visible = true;
			bot_queue_length++;
			pb.Fraction = (double) args.Curprogress / (double) args.Maxprogress;
			GLib.Timeout.Add(5000, delegate {
				bot_queue_length--;
				if(bot_queue_length < 1) {
					bot.Remove(pb);
					bot.Visible = false;
				}
				return false;
			});
		}*/
		
		public void LinkMessage (object o, EventArgs args) {
			sb.Push(1, web.LinkMessage);
		}
		
		/*public void updateProgress(object o, ProgressAllArgs args) {
			Console.WriteLine("on {0} of {1}", args.Curprogress, args.Maxprogress);
			if(args.Curprogress <= args.Maxprogress && args.Curprogress > 0 && args.Maxprogress > 1)
				ProgressbarDisplay(o, args);
		}*/
		
		private void ShowArtist(object o, EventArgs e){
			/*
				A way to find pages thru google's index of wikipedia.
				PROS:
					possibly more effective.  tests show better results than just querying an artist
					If the page doesn't exist, the nearest one will automatically be returned
					Since it's google, it could return a page that's not the exact title, but is the most popular representation of this.  Might work better.
				CONS:
					slower, as we're going to google and then wikipedia
					Since it's google, it could return a page that's not the exact title, but is the most popular representation of this.  Might work worse.
					(Why this method isn't being used right now): because I want to pass to wikipedia that this is a printable page
			*/
			web.LoadUrl(
				"http://www.google.com/search?hl=en&q=site%3Aen.wikipedia.org+" +
				+ "%22" + current_artist + "%22"
				+ "band" +
				+ "&btnI=asdf"
			);
			
			
			/*
			The plain 'ol, link to wikipedia.
			tags on printable=yes.  nice, but hides links, and doesn't persist from page-to-page.
			also, no help with searching for music only...
			
			web.LoadUrl(
				"http://en.wikipedia.org/wiki/" +
				current_artist + " " + 
				"?printable=yes"
			);
			*/
			
			Console.WriteLine("Wikipedia plugin debug: URL=" + web.Location);
		}
		
		// --------------------------------------------------------------- //

		public void HideWikipedia ()
		{
			Visible = false;
		}

		public void ShowWikipedia (string artist)
		{
			/*if(web.Allocation.Width > 1) {
				web.WidthRequest = win.Allocation.Width + 200 - 4;
				win.Hadjustment = new Adjustment(200, 0, win.Allocation.Width, 1, 1, 1);
			}else{
				Console.WriteLine("Apparently it hasn't been drawn yet.  crap.");
			}*/
			if (current_artist == artist) {
				ShowArtist(null, null);
				Visible = true;
				return;
			}
			current_artist = artist;
			ShowArtist(null, null);
			Visible = true;
			ShowAll ();
			this.HeightRequest = 450;
		}
	}
}

