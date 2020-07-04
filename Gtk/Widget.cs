using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GLib;

namespace Gtk
{
    public class Widget : GInitiallyUnowned
    {
        public Widget()
        {
            if (GetType() != typeof(Widget))
            {
                // Subclass
                return;
            }
            
            throw new NotImplementedException("Add gtk_widget_new() equivalent");
        }

        public Widget(GObjectArgs args) : base(args) {}

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_gtk_widget_show(IntPtr raw);
        static d_gtk_widget_show gtk_widget_show = FuncLoader.LoadFunction<d_gtk_widget_show>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_show"));

        public void Show() {
            gtk_widget_show(Handle);
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_gtk_widget_show_all(IntPtr raw);
        static d_gtk_widget_show_all gtk_widget_show_all = FuncLoader.LoadFunction<d_gtk_widget_show_all>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_show_all"));

        public void ShowAll() {
            gtk_widget_show_all(Handle);
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_gtk_widget_get_toplevel(IntPtr widget);
        static d_gtk_widget_get_toplevel gtk_widget_get_toplevel = FuncLoader.LoadFunction<d_gtk_widget_get_toplevel>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_get_toplevel"));

        public Widget GetToplevel() {
            // TODO: Fix this so we can ask for a Widget bypassing Window altogether
            return (Widget)GObject.WrapPointer<Window>(gtk_widget_get_toplevel(Handle), false);
        }
    }
}