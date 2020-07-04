using System;
using System.Runtime.InteropServices;

using GLib;

namespace Gtk
{
    public enum WindowType
    {
        Toplevel,
        Popup
    }

    public class Window : Widget
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

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_window_set_interactive_debugging(bool enable);
		static d_gtk_window_set_interactive_debugging gtk_window_set_interactive_debugging = FuncLoader.LoadFunction<d_gtk_window_set_interactive_debugging>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_set_interactive_debugging"));

        public Window() : this(WindowType.Toplevel) {}

        public Window(WindowType type) : base()
        {
            if (GetType() != typeof(Window))
            {
                // Subclass
                //return;
            }

            defaultConstructor = delegate() {
                return gtk_window_new((int)type);
            };
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

        public static void SetInteractiveDebugging(bool enable)
        {
            gtk_window_set_interactive_debugging(enable);
        }

        public int DefaultHeight
        {
            get => GetProperty<int>("default-height");
            set => SetProperty<int>("default-height", value);
        }

        public int DefaultWidth
        {
            get => GetProperty<int>("default-width");
            set => SetProperty<int>("default-width", value);
        }

        // TODO: REMOVE THIS
        // Window should not contain gtk_container_add
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_container_add(IntPtr raw, IntPtr widget);
		static d_gtk_container_add gtk_container_add = FuncLoader.LoadFunction<d_gtk_container_add>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_container_add"));

        public void Add(GObject widget)
        {
            gtk_container_add(Handle, widget.Handle);
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
            internal override void Populate(/*Value[] values*/) {}
        }
    }
}