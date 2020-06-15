using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GLib;

namespace Gtk
{
    class Widget : GObject
    {
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
    }
}