using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GLib;

namespace Gtk
{
    [GLib.Wrapper]
    public class TextView : Widget
    {
        public TextView()
        {
            defaultConstructor = delegate() {
                return gtk_text_view_new();
            };
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_gtk_text_view_get_type();
        static d_gtk_text_view_get_type gtk_text_view_get_type = FuncLoader.LoadFunction<d_gtk_text_view_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_view_get_type"));

        private static IntPtr GType => gtk_text_view_get_type();

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_gtk_text_view_new();
        static d_gtk_text_view_new gtk_text_view_new = FuncLoader.LoadFunction<d_gtk_text_view_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_text_view_new"));
    }
}