using System;
using System.Runtime.InteropServices;

namespace GLib
{
    static class Utils
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr d_g_malloc(UIntPtr size);
        static d_g_malloc g_malloc = FuncLoader.LoadFunction<d_g_malloc>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_malloc"));

        public static IntPtr StringToPtrGStrdup (string str)
        {
            if (str == null)
                return IntPtr.Zero;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes (str);
            IntPtr result = g_malloc (new UIntPtr ((ulong)bytes.Length + 1));
            Marshal.Copy (bytes, 0, result, bytes.Length);
            Marshal.WriteByte (result, bytes.Length, 0);
            return result;
        }
    }
}