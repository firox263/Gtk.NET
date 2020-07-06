using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GLib;

namespace Gtk
{
    [GLib.Wrapper]
    public class Widget : GLib.InitiallyUnowned
    {
        public Widget()
        {
            defaultConstructor = delegate() {
                throw new NotImplementedException("Add gtk_widget_new() equivalent");
            };
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_widget_get_type();
		static d_gtk_widget_get_type gtk_widget_get_type = FuncLoader.LoadFunction<d_gtk_widget_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_get_type"));

        private static IntPtr GType => gtk_widget_get_type();

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
            return GLib.Object.WrapPointerAs<Widget>(gtk_widget_get_toplevel(Handle), false);
        }
    }
}