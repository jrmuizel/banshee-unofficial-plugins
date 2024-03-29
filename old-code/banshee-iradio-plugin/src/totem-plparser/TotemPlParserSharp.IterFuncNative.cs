// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace TotemPlParserSharp {

	using System;

#region Autogenerated code
	[GLib.CDeclCallback]
	internal delegate void IterFuncNative(IntPtr model, ref Gtk.TreeIter iter, IntPtr uri, IntPtr title, out bool custom_title, IntPtr user_data);

	internal class IterFuncWrapper {

		public void NativeCallback (IntPtr model, ref Gtk.TreeIter iter, IntPtr uri, IntPtr title, out bool custom_title, IntPtr user_data)
		{
			Gtk.TreeModel _arg0 = GLib.Object.GetObject(model) as Gtk.TreeModel;
			Gtk.TreeIter _arg1 = iter;
			string _arg2 = GLib.Marshaller.PtrToStringGFree(uri);
			string _arg3 = GLib.Marshaller.PtrToStringGFree(title);
			bool _arg4;
			managed ( _arg0,  _arg1,  _arg2,  _arg3, out _arg4);
			custom_title = _arg4;
		}

		internal IterFuncNative NativeDelegate;
		TotemPlParser.IterFunc managed;

		public IterFuncWrapper (TotemPlParser.IterFunc managed)
		{
			this.managed = managed;
			if (managed != null)
				NativeDelegate = new IterFuncNative (NativeCallback);
		}

		public static TotemPlParser.IterFunc GetManagedDelegate (IterFuncNative native)
		{
			if (native == null)
				return null;
			IterFuncWrapper wrapper = (IterFuncWrapper) native.Target;
			if (wrapper == null)
				return null;
			return wrapper.managed;
		}
	}
#endregion
}
