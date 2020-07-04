using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GLib;

namespace Gtk
{
    class Button : Widget
    {
        public Button(string label) : base()
        {
            if (GetType() != typeof(Button))
            {
                // Subclass
                return;
            }

            defaultConstructor = delegate() {
                return gtk_button_new_with_label(Utils.StringToPtrGStrdup(label));
            };
        }

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