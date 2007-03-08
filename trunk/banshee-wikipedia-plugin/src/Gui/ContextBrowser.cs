
using System;
using System.IO;
using Gecko;
using Gtk;
using Banshee.Base;
namespace Banshee.Plugins.Wikipedia
{
	
	public class ContextBrowser : VBox
	{
		private WebControl web;
		//private Viewport win;
		private Statusbar sb;
		//private VBox main;
		private int bot_queue_length;
		
		public ContextBrowser()
		{
			this.InitBrowser();
		}
		private void InitBrowser() {
			web = new WebControl("about:blank", "Gecko");
			//win = new Viewport();
			sb = new Statusbar();
			//main = new VBox();
			bot_queue_length = 0;
			
			sb.TextPushed += new Gtk.TextPushedHandler (StatusbarDisplay);
			sb.Visible = false;
			sb.HasResizeGrip = false;
			
			web.LinkMsg += new EventHandler (LinkMessage);
			//web.LoadUrl("about:blank");
			this.PackStart(web, true, true, 0);
			this.PackEnd(sb, false, false, 0);
			this.ShowAll();
			
		}
		
		public void Render(Page p) {
			//ThreadAssist.Spawn(delegate {
			string temp;
			if ( p != null ) {
				StringReader sr = new StringReader(p.Content);
				web.OpenStream(p.Url,"text/html");
				//Console.WriteLine(p.Url);
				//Console.WriteLine(p.GetType());
				while ( (temp = sr.ReadLine())!= null ) {
					web.AppendData(temp);					
					//Console.Write(temp);
				}
				web.CloseStream();
				web.ShowAll();
				this.ShowAll();

				//Console.WriteLine("Wikipedia plugin debug:");
			} else {
				Visible = false;
			}
			//});
		}
		
		public void StatusbarDisplay (object o, TextPushedArgs args) {
			//bot.Visible = true;
			bot_queue_length++;
			GLib.Timeout.Add(5000, delegate {
				bot_queue_length--;
				sb.Pop(1);
				if(bot_queue_length < 1) {
					//bot.Visible = false;
					//pb.Visible = false;
				}
				return false;
			});
		}
		
		public void LinkMessage (object o, EventArgs args) {
			sb.Push(1, web.LinkMessage);
		}
	}
	
}