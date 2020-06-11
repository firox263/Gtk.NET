using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace Gtk
{
    enum WindowType
    {
        Toplevel,
        Popup
    }

    class Window : GObject
    {
        // Delegates
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_gtk_window_new(int type);
        static d_gtk_window_new gtk_window_new = FuncLoader.LoadFunction<d_gtk_window_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_new"));


        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_gtk_window_present(IntPtr raw);
        static d_gtk_window_present gtk_window_present = FuncLoader.LoadFunction<d_gtk_window_present>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_present"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_window_get_title(IntPtr raw);
		static d_gtk_window_get_title gtk_window_get_title = FuncLoader.LoadFunction<d_gtk_window_get_title>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_get_title"));

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_window_set_title(IntPtr raw, IntPtr title);
		static d_gtk_window_set_title gtk_window_set_title = FuncLoader.LoadFunction<d_gtk_window_set_title>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_set_title"));

        public Window() : this(WindowType.Toplevel) {}

        public Window(WindowType type)
        {
            this._handle = gtk_window_new((int)type);
        }

        public void Present()
        {
            gtk_window_present(Handle);
        }

        public void SetTitle(string title)
        {
            gtk_window_set_title(Handle, Utils.StringToPtrGStrdup(title));
        }

        public void GetTitle()
        {
            gtk_window_get_title(Handle);
        }

        public event SignalHandler<DestroySignalArgs> Destroy
        {
            add {
                ConnectSignal(value, "destroy");
            }
            remove {
                DisconnectSignal(value);
            }
        }

        public class DestroySignalArgs : SignalArgs
        {
            internal override void Populate(Value[] values) {}
        }
    }
}