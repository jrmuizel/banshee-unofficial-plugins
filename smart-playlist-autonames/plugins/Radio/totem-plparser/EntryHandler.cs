// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace TotemPlParser {

	using System;

	public delegate void EntryHandler(object o, EntryArgs args);

	public class EntryArgs : GLib.SignalArgs {
		public string Uri{
			get {
				return (string) Args[0];
			}
		}

		public string Title{
			get {
				return (string) Args[1];
			}
		}

		public string Genre{
			get {
				return (string) Args[2];
			}
		}

	}
}
