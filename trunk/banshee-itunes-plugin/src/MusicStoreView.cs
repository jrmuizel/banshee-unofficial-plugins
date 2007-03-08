using System;
using System.Collections;
using Mono.Unix;

using Gtk;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Widgets;

namespace Banshee.Plugins.MusicStore
{
	public class MusicStoreView : TreeView
	{
		private MusicStorePlugin plugin;
		
		public MusicStoreView (MusicStorePlugin plugin)
		{
			this.plugin = plugin;
			
			Model = new ListStore (typeof(MusicStoreTrackInfo));
			RowActivated += new RowActivatedHandler (OnRowActivated);
			
			CellRendererText renderer_text = new CellRendererText ();
			
			TreeViewColumn type_column = new TreeViewColumn ();
			type_column.Title = Catalog.GetString ("Artist");
			type_column.PackStart (renderer_text, false);
			type_column.SetCellDataFunc (renderer_text, new TreeCellDataFunc (ArtistCellDataFunc));
			AppendColumn (type_column);
			
			TreeViewColumn name_column = new TreeViewColumn ();
			name_column.Title = Catalog.GetString ("Title");
			name_column.PackStart (renderer_text, false);
			name_column.SetCellDataFunc (renderer_text, new TreeCellDataFunc (TitleCellDataFunc));
			AppendColumn (name_column);
			
			TreeViewColumn album_column = new TreeViewColumn ();
			album_column.Title = Catalog.GetString ("Album");
			album_column.PackStart (renderer_text, false);
			album_column.SetCellDataFunc (renderer_text, new TreeCellDataFunc (AlbumCellDataFunc));
			AppendColumn (album_column);
			
			TreeViewColumn time_column = new TreeViewColumn ();
			time_column.Title = Catalog.GetString ("Time");
			time_column.PackStart (renderer_text, false);
			time_column.SetCellDataFunc (renderer_text, new TreeCellDataFunc (TimeCellDataFunc));
			AppendColumn (time_column);
			
			TreeViewColumn price_column = new TreeViewColumn ();
			price_column.Title = Catalog.GetString ("Price");
			price_column.PackStart (renderer_text, false);
			price_column.SetCellDataFunc (renderer_text, new TreeCellDataFunc (PriceCellDataFunc));
			AppendColumn (price_column);
		}
		
		public void AddTrack (MusicStoreTrackInfo track)
		{
			((ListStore)Model).AppendValues (track);
		}
		
		public void Clear ()
		{
			((ListStore)Model).Clear ();
		}
		
		// --------------------------------------------------------------- //
		
		private MusicStoreTrackInfo GetSelectedTrack ()
		{
			TreeModel model;
			TreeIter iter;
			
			if (!this.Selection.GetSelected(out model, out iter))
				return null;
			
			return (MusicStoreTrackInfo) model.GetValue(iter, 0);
		}
		
		// --------------------------------------------------------------- //
		
		private void ArtistCellDataFunc (TreeViewColumn column,
						 CellRenderer renderer,
						 TreeModel model,
						 TreeIter iter)
		{
			MusicStoreTrackInfo track = (MusicStoreTrackInfo) model.GetValue (iter, 0);
			((CellRendererText)renderer).Text = track.Artist;
		}
		
		private void TitleCellDataFunc (TreeViewColumn column,
						CellRenderer renderer,
						TreeModel model,
						TreeIter iter)
		{
			MusicStoreTrackInfo track = (MusicStoreTrackInfo) model.GetValue (iter, 0);
			((CellRendererText)renderer).Text = track.Title;
		}
		
		private void AlbumCellDataFunc (TreeViewColumn column,
						CellRenderer renderer,
						TreeModel model,
						TreeIter iter)
		{
			MusicStoreTrackInfo track = (MusicStoreTrackInfo) model.GetValue (iter, 0);
			((CellRendererText)renderer).Text = track.Album;
		}
		
		private void TimeCellDataFunc (TreeViewColumn column,
					       CellRenderer renderer,
					       TreeModel model,
					       TreeIter iter)
		{
			MusicStoreTrackInfo track = (MusicStoreTrackInfo) model.GetValue (iter, 0);
			((CellRendererText)renderer).Text = track.Duration.ToString ();
		}
		
		private void PriceCellDataFunc (TreeViewColumn column,
						CellRenderer renderer,
						TreeModel model,
						TreeIter iter)
		{
			MusicStoreTrackInfo track = (MusicStoreTrackInfo) model.GetValue (iter, 0);
			((CellRendererText)renderer).Text = track.DisplayPrice;
		}
		
		// --------------------------------------------------------------- //
		
		private void OnRowActivated (object o, RowActivatedArgs args)
		{
			MusicStoreTrackInfo track = GetSelectedTrack ();
			plugin.Purchase (track);
		}
	}
}
