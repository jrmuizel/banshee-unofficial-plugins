using Gtk;
using Gdk;
using System;

namespace Banshee.Widgets
{
	public class Rating : Gtk.EventBox
	{
		int rating;
		Pixbuf display_pixbuf;
		public object RatedObject;
		
		static int max_rating = 5;
		static int min_rating = 1;
		static Pixbuf icon_rated;
		static Pixbuf icon_blank;
		
		public event EventHandler Changed;
		
		public Rating () : this (1) {}

		public Rating (int rating)
		{
			if (IconRated.Height != IconNotRated.Height || IconRated.Width != IconNotRated.Width)
				throw new ArgumentException ("Rating widget requires that rated and blank icons have the same height and width");
			
			this.rating = rating;
			
			CanFocus = true;
			
			display_pixbuf = new Pixbuf (Gdk.Colorspace.Rgb, true, 8, Width, Height);
			
			// Start display transparent
			display_pixbuf.Fill(0xffffff00);
			
			DrawRating (DisplayPixbuf, Value);
			
			// DirectionChanged
			
			Add (new Gtk.Image (display_pixbuf));
			
			ShowAll ();
		}
		
		~Rating ()
		{
			display_pixbuf.Dispose ();
			display_pixbuf = null;
			
			icon_rated = null;
			icon_blank = null;
		}
		
		public static Pixbuf DrawRating (int val)
		{
			Pixbuf buf = new Pixbuf (Gdk.Colorspace.Rgb, true, 8, Width, Height);
			DrawRating (buf, val);
			return buf;
		}
		
		public static void DrawRating (Pixbuf pbuf, int val)
		{
			if (val == 0) {
				pbuf.Fill(0xffffff00);
			} else {
				for (int i = 0; i < MaxRating; i++) {
					if (i <= val - MinRating) {
						IconRated.CopyArea (0, 0, IconRated.Width, IconRated.Height,
							pbuf, i * IconRated.Width, 0);
					} else {
						IconNotRated.CopyArea (0, 0, IconRated.Width, IconRated.Height,
							pbuf, i * IconRated.Width, 0);
					}
				}
			}
		}
		
		private int RatingFromPosition (double x)
		{
			return (int) (x / (double)(IconRated.Width)) + 1;
		}
		
		// Event Handlers
		[GLib.ConnectBefore]
		protected override bool OnButtonPressEvent (Gdk.EventButton eb)
		{
			if (eb.Button != 1)
				return false;
			
			Value = RatingFromPosition (eb.X);
			return true;
		}
		
		public bool HandleKeyPress (Gdk.EventKey ek)
		{
			return this.OnKeyPressEvent (ek);
		}
		
		[GLib.ConnectBefore]
		protected override bool OnKeyPressEvent (Gdk.EventKey ek)
		{
			switch (ek.Key) {
			case Gdk.Key.Up:
			case Gdk.Key.Right:
			case Gdk.Key.plus:
			case Gdk.Key.equal:
				Value++;
				return true;
				
			case Gdk.Key.Down:
			case Gdk.Key.Left:
			case Gdk.Key.minus:
				Value--;
				return true;
			}
			
			if (ek.KeyValue >= (48 + MinRating) &&
			       ek.KeyValue <= (48 + MaxRating) &&
			       ek.KeyValue <= 59) {
				Value = (int) ek.KeyValue - 48;
				return true;
			}
			
			return false;
		}
		
		[GLib.ConnectBefore]
		protected override bool OnScrollEvent (EventScroll args)
		{
			switch (args.Direction) {
			case Gdk.ScrollDirection.Up:
			case Gdk.ScrollDirection.Right:
				Value++;
				return true;
				
			case Gdk.ScrollDirection.Down:
			case Gdk.ScrollDirection.Left:
				Value--;
				return true;
			}
			
			return false;
		}
		
		[GLib.ConnectBefore]
		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evnt)
		{
			// TODO draw highlights onmouseover a rating? (and clear on leaveNotify)
			if (evnt.State != Gdk.ModifierType.Button1Mask)
				return false;
			
			Value = RatingFromPosition (evnt.X);
			return true;
		}

		// Event Changed Dispatcher
		private void OnChanged ()
		{
			DrawRating (DisplayPixbuf, Value);
			QueueDraw ();

			EventHandler changed = Changed;
			
			if (changed != null)
				changed (this, new EventArgs ());
		}
		
		// Properties
		public int Value {
			get { return rating; }
			
			set {
				if (rating != value && value >= min_rating && value <= max_rating) {
					rating = value;
					OnChanged ();
				}
			}
		}
		
		public Pixbuf DisplayPixbuf {
			get { return display_pixbuf; }
		}
		
		public static int MaxRating {
			get { return max_rating; }
			set { max_rating = value; }
		}
		
		public static int MinRating {
			get { return min_rating; }
			set { min_rating = value; }
		}
		
		public static int NumLevels {
			get { return max_rating - min_rating + 1; }
		}
		
		public static Pixbuf IconRated {
			get {
				if (icon_rated == null)
					icon_rated = Gdk.Pixbuf.LoadFromResource("icon-rated.png");
				
				return icon_rated;
			}
			
			set { icon_rated = value; }
		}
		
		public static Pixbuf IconNotRated {
			get {
				if (icon_blank == null)
					icon_blank = Gdk.Pixbuf.LoadFromResource("icon-blank.png");
				
				return icon_blank;
			}
			
			set { icon_blank = value; }
		}
		
		public static int Width {
			get { return IconRated.Width * NumLevels; }
		}
		
		public static int Height {
			get { return IconRated.Height; }
		}
	}

	public class CellRendererRating : CellRenderer
	{
		public int RatingValue = 1;
		
		public delegate void RatingEventHandler (object rated_object, int rating); 
		public event RatingEventHandler RatingChanged;
		
		Rating rating;
		
		public CellRendererRating () {}
		
		protected override void Render (Gdk.Drawable drawable,
            Widget widget, Rectangle background_area,
            Rectangle cell_area, Rectangle expose_area,
            CellRendererState flags)
        {
        	Gdk.Window window = drawable as Gdk.Window;

            Pixbuf buf = Rating.DrawRating (RatingValue);
   			window.DrawPixbuf(widget.Style.TextGC (RendererStateToWidgetState (flags)),
                buf, 0, 0,
                cell_area.X + 1, cell_area.Y + 1,
                Rating.Width, Rating.Height,
                RgbDither.None, 0, 0
            );

            buf.Dispose ();
        }
            
        public override void GetSize(Widget widget, ref Gdk.Rectangle cell_area,
            out int x_offset, out int y_offset, out int width, out int height)
        {
            height = Rating.Height;
            width = Rating.Width;
            x_offset = 0;
            y_offset = 0;
        }

        /*public override bool Activate(Gdk.Event ev, Widget widget, string path, 
                Gdk.Rectangle bg_area, Gdk.Rectangle cell_area, CellRendererState flags)
        {
            StartEditing (ev, widget, path, bg_area, cell_area, flags);
            return true;
        }*/
        
        public override CellEditable StartEditing (Gdk.Event ev, Widget widget, string path, 
			Gdk.Rectangle background_area, Gdk.Rectangle cell_area, CellRendererState flags)
		{
			rating = new Rating (RatingValue);
			rating.RatedObject = path;
			CellEditableRating cer = new CellEditableRating (rating);
			cer.EditingDone += HandleEditingDone;
			return cer;
		}
		
		private void HandleEditingDone (object o, EventArgs args)
		{
			OnRatingChanged ();
			rating = null;
		}
        
        private static StateType RendererStateToWidgetState(CellRendererState flags)
        {
            StateType state = StateType.Normal;

            if ((CellRendererState.Insensitive & flags).Equals(
                CellRendererState.Insensitive)) {
                state = StateType.Insensitive;
            } else if((CellRendererState.Selected & flags).Equals(
                CellRendererState.Selected)) {
                state = StateType.Selected;
            }

            return state;
        }
        
        private void OnRatingChanged () {
        	RatingEventHandler h = RatingChanged;
        	if (h != null)
        		h (rating.RatedObject, rating.Value);
        }    
	}
	
	// This class is a work around (idea and most code from lluis) that allows us
	// to enable editing of the rating from within the TreeView.
	class CellEditableRating : Entry
	{
		Rating r;
		
		public CellEditableRating (Gtk.EventBox child)
		{
			r = child as Rating;
			//box.KeyPressEvent += OnClickBox;
			r.ModifyBg (StateType.Normal, Style.White);
			ModifyBg (StateType.Normal, Style.White);
			//box.Add (child);
			//child.Show ();
			r.ShowAll();
			Show ();
			
			HasFrame = false;
		}
		
		//[GLib.ConnectBefore]
		//void OnClickBox (object s, KeyPressEventArgs args)
		//{
			// Avoid forwarding the button press event to the
			// tree, since it would hide the cell editor.
		//	args.RetVal = true;
		//}
		
		protected override void OnParentSet (Gtk.Widget parent)
		{
			base.OnParentSet (parent);
			
			if (Parent != null) {
				if (ParentWindow != null)
					r.ParentWindow = ParentWindow;
				r.Parent = Parent;
				r.Show ();
			}
			else
				r.Unparent ();
		}
		
		[GLib.ConnectBefore]
		protected override bool OnKeyPressEvent (Gdk.EventKey ek)
		{
			return r.HandleKeyPress (ek);
		}
		
		protected override void OnShown ()
		{
			// Do nothing.
		}
		
		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			r.SizeRequest ();
			r.Allocation = allocation;
		}
	}
}
