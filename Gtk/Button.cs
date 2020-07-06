using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GLib;

namespace Gtk
{
    [GLib.Wrapper]
    public class Button : Widget
    {
        public Button(string label)
        {
            defaultConstructor = delegate() {
                return gtk_button_new_with_label(Utils.StringToPtrGStrdup(label));
            };
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_gtk_button_get_type();
        static d_gtk_button_get_type gtk_button_get_type = FuncLoader.LoadFunction<d_gtk_button_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_button_get_type"));

        private static IntPtr GType => gtk_button_get_type();

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_button_new_with_label(IntPtr label);
		static d_gtk_button_new_with_label gtk_button_new_with_label = FuncLoader.LoadFunction<d_gtk_button_new_with_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_button_new_with_label"));

        public event SignalHandler<SignalArgs> Clicked
        {
            add {
                ConnectSignal(value, "clicked");
            }
            remove {
                DisconnectSignal(value);
            }
        }

    }
}