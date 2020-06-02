using System;
using System.Runtime.InteropServices;

namespace Gtk
{
    class Program
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_init(ref int argc, ref IntPtr argv);
		static d_gtk_init gtk_init = FuncLoader.LoadFunction<d_gtk_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_init"));



        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_main();
		static d_gtk_main gtk_main = FuncLoader.LoadFunction<d_gtk_main>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_main"));


        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_window_new(int type);
		static d_gtk_window_new gtk_window_new = FuncLoader.LoadFunction<d_gtk_window_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_new"));


        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_window_present(IntPtr raw);
		static d_gtk_window_present gtk_window_present = FuncLoader.LoadFunction<d_gtk_window_present>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_present"));

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IntPtr argv = new IntPtr(0);
            int argc = 0;

            gtk_init(ref argc, ref argv);

            IntPtr handle = gtk_window_new(0);
            gtk_window_present(handle);            

            gtk_main();

            // STATUS: Presents a window
            //
            // NEXT STEPS:
            //  - Investigate into Glib plumbing
            //  - We want to support the following:
            //     => Subclassing
            //     => Interfaces
            //     => Virtual Functions
            //     => GType interaction/registration
            //
            //  - Goals:
            //     => To create a easier-to-maintain set of bindings for Gtk
            //     => Integration with gobject-introspection
            //     => Later on, investigate native C# integrations:
            //         -> e.g. System.Drawing, XAML, etc
        }
    }
}
