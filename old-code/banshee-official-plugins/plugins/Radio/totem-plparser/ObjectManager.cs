// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GtkSharp.TotemplparserSharp {

	public class ObjectManager {

		static bool initialized = false;
		// Call this method from the appropriate module init function.
		public static void Initialize ()
		{
			if (initialized)
				return;

			initialized = true;
			GLib.GType.Register (TotemPlParser.Parser.GType, typeof (TotemPlParser.Parser));
		}
	}
}
