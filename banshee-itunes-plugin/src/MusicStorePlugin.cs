using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;

using Gtk;
using Mono.Unix;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Widgets;

namespace Banshee.Plugins.MusicStore
{
	public class MusicStorePlugin : Banshee.Plugins.Plugin
	{
		private GConf.Client config;

		protected override string ConfigurationName { get { return "iTunes"; } }
		public override string DisplayName { get { return "iTunes Music Store"; } }
		
		public override string Description {
			get {
				return Catalog.GetString("Enables you to purchase music and audiobooks from the iTunes Music Store." +
							 "The audio files that you buy and download are encoded using Advanced " + 
							 "Audio Encoding (AAC). You must have a registered iTunes account.");
			}
		}
		
		public override string [] Authors {
			get {
				return new string [] {
					"Jon Lech Johansen",
					"Fredrik Hedberg"
				};
			}
		}
		
		// --------------------------------------------------------------- //
		
		private MusicStoreSource source;
		private MusicStoreView view;
		
		public Gtk.Widget View {
			get { 
				if (view == null)
					view = new MusicStoreView (this);

				return this.view; 
			}
		}
		
		protected override void PluginInitialize()
		{
			RegisterConfigurationKey("Username");
			RegisterConfigurationKey("Password");
			RegisterConfigurationKey("Country");
			config = Globals.Configuration;

			source = new MusicStoreSource(this);
			
			SourceManager.AddSource(source);
		}
		
		protected override void PluginDispose()
		{
			SourceManager.RemoveSource(source);
		}
                
		public override Gtk.Widget GetConfigurationWidget()
		{
			return new MusicStoreConfigPage(this);
		}
		
		// --------------------------------------------------------------- //
		
		private FairStore fstore;
		
		public FairStore Store
		{
			get {
				if (fstore == null) {
					fstore = new FairStore ();
					
					System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatesPolicy();
				}
				fstore.Country = Country;
				return fstore;
			}
		}
		
		// --------------------------------------------------------------- //

		public string Username {
			get {
				return GetStringPref(ConfigurationKeys["Username"], String.Empty);
			}
			
			set {
				config.Set(ConfigurationKeys["Username"], value);
			}
		}
		
		public string Password {
			get {
				return GetStringPref(ConfigurationKeys["Password"], String.Empty);
			}
			
			set {
				config.Set(ConfigurationKeys["Password"], value);
			}
		}		

		private static string default_country = "United States";

		public string Country {
			get {
				return GetStringPref(ConfigurationKeys["Country"], default_country);
			}
			
			set {
				config.Set(ConfigurationKeys["Country"], value);
			}
		}		
		
		// --------------------------------------------------------------- //
		
		public bool Login ()
		{
			if (Username == "") {
				HigMessageDialog.RunHigMessageDialog (null,
								      0, 
								      MessageType.Warning,
								      ButtonsType.Ok,
								      "Account Information Missing",
								      "You have not yet entered you iTunes Music Store account information " + 
								      "in the plugin configuration dialog. You must do this before you can " +
								      "purchase and download songs.");
				return false;
			}

			try {
				Store.Country = Country;
				Store.Login (Username, Password, 0);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return false;
			}
			
			return true;
		}

		public void Search (string terms)
		{
			// FIXME: Spawn and proxy the tracks back to the view in the mainloop.
			view.Clear ();
			foreach (Hashtable song in Store.Search (terms))
				view.AddTrack (new MusicStoreTrackInfo (song));
		}
		
		public void Purchase (MusicStoreTrackInfo track)
		{
			// FIXME: Login shouldnt be here		
			if (!Login ())
				return;
			
                        string message = String.Format (Catalog.GetString ("You are about to purchase the song <i>{0}</i> from <i>{1}</i>, " +
                                                                           "by <i>{2}</i>. Your account will be charged a fee of {3}."),
							track.Title,
							track.Album,
							track.Artist,
							track.DisplayPrice);
			
                        ResponseType response = HigMessageDialog.RunHigMessageDialog (null,
                                                                                      0,
                                                                                      MessageType.Info,
                                                                                      ButtonsType.OkCancel,
                                                                                      "Purchase Song",
                                                                                      message);
			
                        if (response != ResponseType.Ok)
                                return;

			MusicStorePurchaseTransaction trans = new MusicStorePurchaseTransaction (this, track);
			trans.Run ();
		}
		
		// --------------------------------------------------------------- //

		private string GetStringPref(string key, string def)
		{
			try {
				return (string) config.Get(key);
			} catch {
				return def;
			}
		}

		public static string GetLibraryPathForTrack (MusicStoreTrackInfo track)
		{
			// FIXME: Subdirectories
			return Path.Combine (Globals.Library.Location, String.Format ("{0}-{1}-{2}.m4a", track.Artist, track.Album, track.Title));
		}
	}
}
