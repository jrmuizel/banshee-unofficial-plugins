
using System;
using Banshee.Base;
using Mono.Unix;
namespace Banshee.Plugins.Wikipedia
{
	
	public class WikipediaQueryController
	{
		private WikipediaPane pane;
		private WikiQuery wq;
		private WikipediaHeader wh;
		private WikipediaFooter wf;
		private WikipediaWaitBody wp;
		private TrackInfo track;
		public TrackInfo Track {
			get {
				return this.track;
			}
			set {
				if ( this.track == null || this.track.Artist != value.Artist ) {
					this.track = value;
					this.pane.Visible = true;
					this.LookUpArtist();
				} else {
					this.track = value;
				}
			}
		}
				
		public WikipediaPane Pane {
			get {
				if ( pane == null ) {
					//Console.WriteLine("Pane ist NULL");
					return null;
				}
				else return this.pane;
				//return null;
			}
		}
		
		public WikipediaQueryController()
		{
			
			this.pane = new WikipediaPane();
			this.wf   = new CommonWikipediaFooter();
			this.wp   = new WikipediaWaitBody(Catalog.GetString("Please Wait while retrieving infomation"));
			pane.ArtistButton.Clicked   += new EventHandler(onArtistButtonClicked);
			pane.AlbumButton.Clicked    += new EventHandler(onAlbumButtonClicked);
			pane.GenreButton.Clicked    += new EventHandler(onGenreButtonClicked);
			//Console.WriteLine("Initializing {0}",this.GetType());
		}
		
		private void PerformLookUp() {
			ThreadAssist.Spawn(delegate {
				ThreadAssist.ProxyToMain(delegate {
					wh.Component = this.wp;
					wf.Component = wh;			
					this.Pane.Browser.Render(wf);
				});
				WikipediaPage p = null;
				if ( wq.Find() ) {
					p = wq.GetResult();
				} else { //show error page or hide pane
					p = new WikipediaErrorPage(Catalog.GetString("Error:"),Catalog.GetString("Information not found"));
				}
				wh.Component = p;
				wf.Component = wh;
			
				ThreadAssist.ProxyToMain(delegate {
					this.Pane.Browser.Render(wf);
				});
			});
			
			
		}
		
		public void Destroy() {
			pane.Destroy();
		}
		
		
		private void LookUpArtist() {
			this.wq = new WikipediaArtistQuery(track);
			this.wh = new WikipediaArtistHeader();
			this.PerformLookUp();
		}
		
		private void onArtistButtonClicked(object sender, EventArgs args) {
			this.LookUpArtist();
		}
		private void onAlbumButtonClicked(object sender, EventArgs args) {
			this.wq = new WikipediaAlbumQuery(track);
			this.wh = new WikipediaAlbumHeader();
			this.PerformLookUp();
		}
		private void onGenreButtonClicked(object sender, EventArgs args) {
			this.wq = new WikipediaGenreQuery(track);
			this.wh = new WikipediaGenreHeader();
			this.PerformLookUp();
		}
	} 
}
