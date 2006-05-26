using Gtk;
using System;
using Mono.Unix;
namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaPane : Gtk.VBox
	{
		private Gtk.Button genre_button;		
		private Gtk.Button artist_button;
		private Gtk.Button album_button;
		private Gtk.Button hide_show_button;
		//private Gtk.VBox main;
		private bool minimized;
		private WikipediaBrowser wb;
		
		public Gtk.Button ArtistButton {
			get {
				return this.artist_button;
			}
		}
		public Gtk.Button AlbumButton {
			get {
				return this.album_button;
			}
		}
		public Gtk.Button GenreButton {
			get {
				return this.genre_button;
			}
		}
		/*public Gtk.Entry CommoEntry {
			get {
				return this.common_entry;
			}
		}*/
		
		public WikipediaBrowser Browser {
			get { return this.wb;}
			
		}
		
		public WikipediaPane()
		{
			this.InitGui();
			Console.WriteLine("Initializing {0}",this.GetType());
			
		}

		protected /*virtual*/ void OnHideShowClicked(object sender, System.EventArgs e)
		{
			if ( !this.minimized ) {
				this.Minimize();
			} else {
				this.Maximize();
			}
			
		}		
		
		public void Minimize() {
			this.wb.Visible = false;
			this.minimized    = true;
			this.hide_show_button.Image = new Gtk.Image(null,"plus.png");
			this.hide_show_button.Show();
			this.HeightRequest = 0;
		}
		public void Maximize() {			
			this.wb.Visible = true;
			this.minimized    = false;
			this.hide_show_button.Image = new Gtk.Image(null,"minus.png");
			this.hide_show_button.Show();
			this.HeightRequest = 400;
			this.wb.ShowAll();
			
		}
		private void InitGui() {
			
			//genre button
			Gtk.Image square = new Gtk.Image(null,"cubo_verde.png");
			Gtk.HBox genre_hbox = new HBox(false,0);
			genre_hbox.Add(square);
			genre_hbox.Add(new Label(Catalog.GetString("Genre")));
			genre_button = new Gtk.Button(genre_hbox);
			genre_button.Relief = ReliefStyle.None;
			
			// artist
			Gtk.HBox artist_hbox = new HBox(false,0);
			artist_hbox.Add(new Gtk.Image(null,"cubo_verde.png"));
			artist_hbox.Add(new Label(Catalog.GetString("Artist")));
			artist_button = new Gtk.Button(artist_hbox);
			artist_button.Relief = ReliefStyle.None;
			
			//album button
			Gtk.HBox album_hbox = new HBox(false,0);
			album_hbox.Add(new Gtk.Image(null,"cubo_verde.png"));
			album_hbox.Add(new Label(Catalog.GetString("Album")));
			album_button = new Gtk.Button(album_hbox);
			album_button.Relief = ReliefStyle.None;
			
			// Button bar
			Gtk.HButtonBox hb = new Gtk.HButtonBox();
			hb.Layout  = Gtk.ButtonBoxStyle.Start;
			hb.Spacing = 5;
			hb.Add(artist_button);
			hb.Add(album_button);
			hb.Add(genre_button);
			
			// hide/show button	
			hide_show_button          = new Gtk.Button(new Gtk.Image(null,"minus.png"));
			hide_show_button.Relief   = ReliefStyle.None;
			hide_show_button.Clicked += new EventHandler(OnHideShowClicked);
			this.minimized = false;
			
			//search label
			Gtk.Label search_l = new Gtk.Label();
			search_l.Markup = "<b>"+Catalog.GetString("Search")+":</b>";
			
			// upper hbox
			Gtk.HBox toolbar = new Gtk.HBox(false,5);
			toolbar.PackStart(new Gtk.Image(null,"Wikipedia-logo.png"),false,false,5);
			toolbar.PackStart(search_l,false,false,5);
			toolbar.PackStart(hb,true,true,0);
			toolbar.PackStart(hide_show_button,false,false,0);
			
			
			this.wb = new WikipediaBrowser();
			
			//main = new Gtk.VBox(false,5);
			this.PackStart(toolbar,false,false,0);
			this.PackEnd(wb,true,true,5);
			//this.Add(main);
			this.HeightRequest = 400;
			//hb.Show();
			//this.wb.Show();
			
			//this.main.Show();
			this.Show();
			
		}
		
	}
	
}
