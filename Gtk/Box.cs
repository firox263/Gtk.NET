using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GLib;

namespace Gtk
{
    [GLib.Wrapper]
    public class Box : Widget
    {
        public Box(Orientation orientation, int spacing)
        {
            defaultConstructor = delegate() {
                return gtk_box_new(orientation, spacing);
            };
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_gtk_box_get_type();
        static d_gtk_box_get_type gtk_box_get_type = FuncLoader.LoadFunction<d_gtk_box_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_box_get_type"));

        private static IntPtr GType => gtk_box_get_type();

        public void PackStart(Widget child, bool expand, bool fill, uint padding)
        {
            gtk_box_pack_start(this.Handle, child.Handle, expand, fill, padding);
        }

        public void PackEnd(Widget child, bool expand, bool fill, uint padding)
        {
            gtk_box_pack_end(this.Handle, child.Handle, expand, fill, padding);
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_gtk_box_new(Orientation orientation, int spacing);
        static d_gtk_box_new gtk_box_new = FuncLoader.LoadFunction<d_gtk_box_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_box_new"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_gtk_box_pack_start(IntPtr box, IntPtr child, bool expand, bool fill, uint padding);
        static d_gtk_box_pack_start gtk_box_pack_start = FuncLoader.LoadFunction<d_gtk_box_pack_start>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_box_pack_start"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_gtk_box_pack_end(IntPtr box, IntPtr child, bool expand, bool fill, uint padding);
        static d_gtk_box_pack_end gtk_box_pack_end = FuncLoader.LoadFunction<d_gtk_box_pack_end>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_box_pack_end"));
    }
}