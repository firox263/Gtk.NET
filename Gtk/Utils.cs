using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace Gtk
{
	public enum Orientation
	{
		Horizontal,
		Vertical
	};

    class Global
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_init(ref int argc, ref IntPtr argv);
		static d_gtk_init gtk_init = FuncLoader.LoadFunction<d_gtk_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_init"));

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_main();
		static d_gtk_main gtk_main = FuncLoader.LoadFunction<d_gtk_main>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_main"));

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_main_quit();
		static d_gtk_main_quit gtk_main_quit = FuncLoader.LoadFunction<d_gtk_main_quit>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_main_quit"));

		public static void GtkInit()
		{
			IntPtr argv = new IntPtr(0);
            int argc = 0;
			Init(argc, argv);
		}

		public static void Init(int argc, IntPtr argv)
		{
			gtk_init(ref argc, ref argv);
		}

		public static void GtkMain()
		{
			gtk_main();
		}

		public static void GtkMainQuit()
		{
			gtk_main_quit();
		}
    }
}