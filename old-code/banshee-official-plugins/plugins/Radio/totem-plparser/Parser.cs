// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace TotemPlParser {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public  class Parser : GLib.Object {

		~Parser()
		{
			Dispose();
		}

		[Obsolete]
		protected Parser(GLib.GType gtype) : base(gtype) {}
		public Parser(IntPtr raw) : base(raw) {}

		[DllImport("totem-plparser.so.1")]
		static extern IntPtr totem_pl_parser_new();

		public Parser () : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Parser)) {
				CreateNativeObject (new string [0], new GLib.Value[0]);
				return;
			}
			Raw = totem_pl_parser_new();
		}

		[GLib.Property ("recurse")]
		public bool Recurse {
			get {
				GLib.Value val = GetProperty ("recurse");
				bool ret = (bool) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("recurse", val);
				val.Dispose ();
			}
		}

		[GLib.CDeclCallback]
		delegate void PlaylistEndSignalDelegate (IntPtr arg0, IntPtr arg1, IntPtr gch);

		static void PlaylistEndSignalCallback (IntPtr arg0, IntPtr arg1, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			TotemPlParser.PlaylistEndArgs args = new TotemPlParser.PlaylistEndArgs ();
			args.Args = new object[1];
			args.Args[0] = GLib.Marshaller.Utf8PtrToString (arg1);
			TotemPlParser.PlaylistEndHandler handler = (TotemPlParser.PlaylistEndHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void PlaylistEndVMDelegate (IntPtr parser, IntPtr title);

		static PlaylistEndVMDelegate PlaylistEndVMCallback;

		static void playlistend_cb (IntPtr parser, IntPtr title)
		{
			Parser parser_managed = GLib.Object.GetObject (parser, false) as Parser;
			parser_managed.OnPlaylistEnd (GLib.Marshaller.Utf8PtrToString (title));
		}

		private static void OverridePlaylistEnd (GLib.GType gtype)
		{
			if (PlaylistEndVMCallback == null)
				PlaylistEndVMCallback = new PlaylistEndVMDelegate (playlistend_cb);
			OverrideVirtualMethod (gtype, "playlist-end", PlaylistEndVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(TotemPlParser.Parser), ConnectionMethod="OverridePlaylistEnd")]
		protected virtual void OnPlaylistEnd (string title)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (title);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("playlist-end")]
		public event TotemPlParser.PlaylistEndHandler PlaylistEnd {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "playlist-end", new PlaylistEndSignalDelegate(PlaylistEndSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "playlist-end", new PlaylistEndSignalDelegate(PlaylistEndSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[GLib.CDeclCallback]
		delegate void EntrySignalDelegate (IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr gch);

		static void EntrySignalCallback (IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			TotemPlParser.EntryArgs args = new TotemPlParser.EntryArgs ();
			args.Args = new object[3];
			args.Args[0] = GLib.Marshaller.Utf8PtrToString (arg1);
			args.Args[1] = GLib.Marshaller.Utf8PtrToString (arg2);
			args.Args[2] = GLib.Marshaller.Utf8PtrToString (arg3);
			TotemPlParser.EntryHandler handler = (TotemPlParser.EntryHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void EntryVMDelegate (IntPtr parser, IntPtr uri, IntPtr title, IntPtr genre);

		static EntryVMDelegate EntryVMCallback;

		static void entry_cb (IntPtr parser, IntPtr uri, IntPtr title, IntPtr genre)
		{
			Parser parser_managed = GLib.Object.GetObject (parser, false) as Parser;
			parser_managed.OnEntry (GLib.Marshaller.Utf8PtrToString (uri), GLib.Marshaller.Utf8PtrToString (title), GLib.Marshaller.Utf8PtrToString (genre));
		}

		private static void OverrideEntry (GLib.GType gtype)
		{
			if (EntryVMCallback == null)
				EntryVMCallback = new EntryVMDelegate (entry_cb);
			OverrideVirtualMethod (gtype, "entry", EntryVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(TotemPlParser.Parser), ConnectionMethod="OverrideEntry")]
		protected virtual void OnEntry (string uri, string title, string genre)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (4);
			GLib.Value[] vals = new GLib.Value [4];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (uri);
			inst_and_params.Append (vals [1]);
			vals [2] = new GLib.Value (title);
			inst_and_params.Append (vals [2]);
			vals [3] = new GLib.Value (genre);
			inst_and_params.Append (vals [3]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("entry")]
		public event TotemPlParser.EntryHandler Entry {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "entry", new EntrySignalDelegate(EntrySignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "entry", new EntrySignalDelegate(EntrySignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[GLib.CDeclCallback]
		delegate void PlaylistStartSignalDelegate (IntPtr arg0, IntPtr arg1, IntPtr gch);

		static void PlaylistStartSignalCallback (IntPtr arg0, IntPtr arg1, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			TotemPlParser.PlaylistStartArgs args = new TotemPlParser.PlaylistStartArgs ();
			args.Args = new object[1];
			args.Args[0] = GLib.Marshaller.Utf8PtrToString (arg1);
			TotemPlParser.PlaylistStartHandler handler = (TotemPlParser.PlaylistStartHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void PlaylistStartVMDelegate (IntPtr parser, IntPtr title);

		static PlaylistStartVMDelegate PlaylistStartVMCallback;

		static void playliststart_cb (IntPtr parser, IntPtr title)
		{
			Parser parser_managed = GLib.Object.GetObject (parser, false) as Parser;
			parser_managed.OnPlaylistStart (GLib.Marshaller.Utf8PtrToString (title));
		}

		private static void OverridePlaylistStart (GLib.GType gtype)
		{
			if (PlaylistStartVMCallback == null)
				PlaylistStartVMCallback = new PlaylistStartVMDelegate (playliststart_cb);
			OverrideVirtualMethod (gtype, "playlist-start", PlaylistStartVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(TotemPlParser.Parser), ConnectionMethod="OverridePlaylistStart")]
		protected virtual void OnPlaylistStart (string title)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (title);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("playlist-start")]
		public event TotemPlParser.PlaylistStartHandler PlaylistStart {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "playlist-start", new PlaylistStartSignalDelegate(PlaylistStartSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "playlist-start", new PlaylistStartSignalDelegate(PlaylistStartSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[DllImport("totem-plparser.so.1")]
		static extern int totem_pl_parser_parse(IntPtr raw, IntPtr url, bool fallback);

		public TotemPlParser.Result Parse(string url, bool fallback) {
			IntPtr url_as_native = GLib.Marshaller.StringToPtrGStrdup (url);
			int raw_ret = totem_pl_parser_parse(Handle, url_as_native, fallback);
			TotemPlParser.Result ret = (TotemPlParser.Result) raw_ret;
			GLib.Marshaller.Free (url_as_native);
			return ret;
		}

		[DllImport("totem-plparser.so.1")]
		static extern void totem_pl_parser_add_ignored_mimetype(IntPtr raw, IntPtr mimetype);

		public void AddIgnoredMimetype(string mimetype) {
			IntPtr mimetype_as_native = GLib.Marshaller.StringToPtrGStrdup (mimetype);
			totem_pl_parser_add_ignored_mimetype(Handle, mimetype_as_native);
			GLib.Marshaller.Free (mimetype_as_native);
		}

		[DllImport("totem-plparser.so.1")]
		static extern IntPtr totem_pl_parser_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = totem_pl_parser_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("totem-plparser.so.1")]
		static extern int totem_pl_parser_error_quark();

		public static int ErrorQuark() {
			int raw_ret = totem_pl_parser_error_quark();
			int ret = raw_ret;
			return ret;
		}

		[DllImport("totem-plparser.so.1")]
		static extern void totem_pl_parser_add_ignored_scheme(IntPtr raw, IntPtr scheme);

		public void AddIgnoredScheme(string scheme) {
			IntPtr scheme_as_native = GLib.Marshaller.StringToPtrGStrdup (scheme);
			totem_pl_parser_add_ignored_scheme(Handle, scheme_as_native);
			GLib.Marshaller.Free (scheme_as_native);
		}


		static Parser ()
		{
			GtkSharp.TotemplparserSharp.ObjectManager.Initialize ();
		}
#endregion
#region Customized extensions
#line 1 "Parser.custom"
		[DllImport("totem-plparser.so.1")]
		static extern unsafe bool totem_pl_parser_write_with_title(IntPtr raw, IntPtr model, TotemPlParserSharp.IterFuncNative func, IntPtr output, IntPtr title, int type, IntPtr user_data, out IntPtr error);
		
		public unsafe bool Write(Gtk.TreeModel model, TotemPlParser.IterFunc func, string output, string title, TotemPlParser.Type type) {
			TotemPlParserSharp.IterFuncWrapper func_wrapper = new TotemPlParserSharp.IterFuncWrapper (func);
			IntPtr output_as_native = GLib.Marshaller.StringToPtrGStrdup (output);
			IntPtr title_as_native = GLib.Marshaller.StringToPtrGStrdup (title);
			IntPtr error = IntPtr.Zero;
			bool raw_ret = totem_pl_parser_write_with_title(Handle, model == null ? IntPtr.Zero : model.Handle, func_wrapper.NativeDelegate, output_as_native, title_as_native, (int) type, IntPtr.Zero, out error);
			bool ret = raw_ret;
			GLib.Marshaller.Free (output_as_native);
			GLib.Marshaller.Free (title_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("totem-plparser.so.1")]
		static extern unsafe bool totem_pl_parser_write(IntPtr raw, IntPtr model, TotemPlParserSharp.IterFuncNative func, IntPtr output, int type, IntPtr user_data, out IntPtr error);

		public unsafe bool Write(Gtk.TreeModel model, TotemPlParser.IterFunc func, string output, TotemPlParser.Type type) {
			TotemPlParserSharp.IterFuncWrapper func_wrapper = new TotemPlParserSharp.IterFuncWrapper (func);
			IntPtr output_as_native = GLib.Marshaller.StringToPtrGStrdup (output);
			IntPtr error = IntPtr.Zero;
			bool raw_ret = totem_pl_parser_write(Handle, model == null ? IntPtr.Zero : model.Handle, func_wrapper.NativeDelegate, output_as_native, (int) type, IntPtr.Zero, out error);
			bool ret = raw_ret;
			GLib.Marshaller.Free (output_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}


#endregion
	}
}
