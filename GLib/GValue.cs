using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace GLib
{
    // TODO: Investigate using a ptr to a GValue in unmanaged memory
    // and then enable the use of generics?
    [StructLayout(LayoutKind.Sequential)]
    struct GValue : IDisposable
    {
        IntPtr type;
        long pad1;
        long pad2;

        public GValue(System.Type type) : this((GType)type) {}

        public GValue(GType gtype)
        {
            pad1 = 0;
            pad2 = 0;
            type = IntPtr.Zero;
            g_value_init(ref this, gtype);
        }

        public void Dispose()
        {
            g_value_unset(ref this);
        }

        // TODO: Terrible, messy solution
        // Either commit to generics or don't use them at all
        public void Set<T>(object val)
        {
            GType gtype = (GType)typeof(T);
            if (gtype != type)
                throw new Exception();

            if (type == GType.Boolean)
                g_value_set_boolean (ref this, (bool) val);
            else if (type == GType.UChar)
                g_value_set_uchar (ref this, (byte) val);
            else if (type == GType.Char)
                g_value_set_char (ref this, (sbyte) val);
            else if (type == GType.Int)
                g_value_set_int (ref this, (int) val);
            else if (type == GType.UInt)
                g_value_set_uint (ref this, (uint) val);
            else if (type == GType.Int64)
                g_value_set_int64 (ref this, (long) val);
            /*else if (type == GType.Long)
                SetLongForPlatform ((long) val);*/
            else if (type == GType.UInt64)
                g_value_set_uint64 (ref this, (ulong) val);
            /*else if (type == GType.ULong)
                SetULongForPlatform (Convert.ToUInt64 (val));
            else if (GType.Is (type, GType.Enum))
                g_value_set_enum (ref this, (int)val);
            else if (GType.Is (type, GType.Flags))
                g_value_set_flags (ref this, (uint)(int)val);*/
            else if (type == GType.Float)
                g_value_set_float (ref this, (float) val);
            else if (type == GType.Double)
                g_value_set_double (ref this, (double) val);
            /*else if (type == GType.Variant)
                g_value_set_variant (ref this, ((GLib.Variant) val).Handle);*/
            else if (type == GType.String) {
                IntPtr native = Utils.StringToPtrGStrdup ((string)val);
                g_value_set_string (ref this, native);
                //GLib.Marshaller.Free (native);
            } else if (type == GType.Pointer) {
                if (val.GetType () == typeof (IntPtr)) {
                    g_value_set_pointer (ref this, (IntPtr) val);
                    return;
                }/* else if (val is IWrapper) {
                    g_value_set_pointer (ref this, ((IWrapper)val).Handle);
                    return;
                }
                IntPtr buf = Marshal.AllocHGlobal (Marshal.SizeOf (val.GetType()));
                Marshal.StructureToPtr (val, buf, false);
                g_value_set_pointer (ref this, buf);*/
            } else if (type == GType.Param) {
                g_value_set_param (ref this, (IntPtr) val);
            }/* else if (type == ValueArray.GType) {
                g_value_set_boxed (ref this, ((ValueArray) val).Handle);
            } else if (type == ManagedValue.GType) {
                IntPtr wrapper = ManagedValue.WrapObject (val);
                g_value_set_boxed (ref this, wrapper);
                ManagedValue.ReleaseWrapper (wrapper);
            } else if (GType.Is (type, GType.Object))
                if(val is GLib.Object)
                    g_value_set_object (ref this, (val as GLib.Object).Handle);
                else
                    g_value_set_object (ref this, ((GInterfaceAdapter)val).Handle);
            else if (GType.Is (type, GType.Boxed)) {
                if (val is IWrapper) {
                    g_value_set_boxed (ref this, ((IWrapper)val).Handle);
                    return;
                }
                IntPtr buf = Marshaller.StructureToPtrAlloc (val);
                g_value_set_boxed (ref this, buf);
                Marshal.FreeHGlobal (buf);
            } else if (GLib.GType.LookupType (type) != null) {
                FromRegisteredType (val);
            }*/ else
                throw new Exception ("Unknown type " + new GType (type).ToString ());
        }

        // It is up to the caller to ensure the value returned
        // is the result they expect it to be.
        public object Get<T>()
        {
            return default(T);
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_g_value_init(ref GValue value, GType g_type);
        static d_g_value_init g_value_init = FuncLoader.LoadFunction<d_g_value_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_init"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_g_value_unset(ref GValue value);
        static d_g_value_unset g_value_unset = FuncLoader.LoadFunction<d_g_value_unset>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_unset"));

        // Copy pasted from GtkSharp
        // TODO: Sort through, remove unused
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_boolean(ref GValue val, bool data);
		static d_g_value_set_boolean g_value_set_boolean = FuncLoader.LoadFunction<d_g_value_set_boolean>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_boolean"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_uchar(ref GValue val, byte data);
		static d_g_value_set_uchar g_value_set_uchar = FuncLoader.LoadFunction<d_g_value_set_uchar>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_uchar"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_char(ref GValue val, sbyte data);
		static d_g_value_set_char g_value_set_char = FuncLoader.LoadFunction<d_g_value_set_char>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_char"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_boxed(ref GValue val, IntPtr data);
		static d_g_value_set_boxed g_value_set_boxed = FuncLoader.LoadFunction<d_g_value_set_boxed>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_boxed"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_double(ref GValue val, double data);
		static d_g_value_set_double g_value_set_double = FuncLoader.LoadFunction<d_g_value_set_double>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_double"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_float(ref GValue val, float data);
		static d_g_value_set_float g_value_set_float = FuncLoader.LoadFunction<d_g_value_set_float>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_float"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_int(ref GValue val, int data);
		static d_g_value_set_int g_value_set_int = FuncLoader.LoadFunction<d_g_value_set_int>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_int"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_int64(ref GValue val, long data);
		static d_g_value_set_int64 g_value_set_int64 = FuncLoader.LoadFunction<d_g_value_set_int64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_int64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_long(ref GValue val, IntPtr data);
		static d_g_value_set_long g_value_set_long = FuncLoader.LoadFunction<d_g_value_set_long>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_long2(ref GValue val, int data);
		static d_g_value_set_long2 g_value_set_long2 = FuncLoader.LoadFunction<d_g_value_set_long2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_uint64(ref GValue val, ulong data);
		static d_g_value_set_uint64 g_value_set_uint64 = FuncLoader.LoadFunction<d_g_value_set_uint64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_uint64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_object(ref GValue val, IntPtr data);
		static d_g_value_set_object g_value_set_object = FuncLoader.LoadFunction<d_g_value_set_object>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_object"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_param(ref GValue val, IntPtr data);
		static d_g_value_set_param g_value_set_param = FuncLoader.LoadFunction<d_g_value_set_param>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_param"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_pointer(ref GValue val, IntPtr data);
		static d_g_value_set_pointer g_value_set_pointer = FuncLoader.LoadFunction<d_g_value_set_pointer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_pointer"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_string(ref GValue val, IntPtr data);
		static d_g_value_set_string g_value_set_string = FuncLoader.LoadFunction<d_g_value_set_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_string"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_uint(ref GValue val, uint data);
		static d_g_value_set_uint g_value_set_uint = FuncLoader.LoadFunction<d_g_value_set_uint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_uint"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_ulong(ref GValue val, UIntPtr data);
		static d_g_value_set_ulong g_value_set_ulong = FuncLoader.LoadFunction<d_g_value_set_ulong>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_ulong2(ref GValue val, uint data);
		static d_g_value_set_ulong2 g_value_set_ulong2 = FuncLoader.LoadFunction<d_g_value_set_ulong2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_enum(ref GValue val, int data);
		static d_g_value_set_enum g_value_set_enum = FuncLoader.LoadFunction<d_g_value_set_enum>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_enum"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_flags(ref GValue val, uint data);
		static d_g_value_set_flags g_value_set_flags = FuncLoader.LoadFunction<d_g_value_set_flags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_flags"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_set_variant(ref GValue val, IntPtr data);
		static d_g_value_set_variant g_value_set_variant = FuncLoader.LoadFunction<d_g_value_set_variant>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_set_variant"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_value_get_boolean(ref GValue val);
		static d_g_value_get_boolean g_value_get_boolean = FuncLoader.LoadFunction<d_g_value_get_boolean>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_boolean"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate byte d_g_value_get_uchar(ref GValue val);
		static d_g_value_get_uchar g_value_get_uchar = FuncLoader.LoadFunction<d_g_value_get_uchar>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_uchar"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate sbyte d_g_value_get_char(ref GValue val);
		static d_g_value_get_char g_value_get_char = FuncLoader.LoadFunction<d_g_value_get_char>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_char"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_boxed(ref GValue val);
		static d_g_value_get_boxed g_value_get_boxed = FuncLoader.LoadFunction<d_g_value_get_boxed>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_boxed"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate double d_g_value_get_double(ref GValue val);
		static d_g_value_get_double g_value_get_double = FuncLoader.LoadFunction<d_g_value_get_double>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_double"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate float d_g_value_get_float(ref GValue val);
		static d_g_value_get_float g_value_get_float = FuncLoader.LoadFunction<d_g_value_get_float>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_float"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_value_get_int(ref GValue val);
		static d_g_value_get_int g_value_get_int = FuncLoader.LoadFunction<d_g_value_get_int>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_int"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate long d_g_value_get_int64(ref GValue val);
		static d_g_value_get_int64 g_value_get_int64 = FuncLoader.LoadFunction<d_g_value_get_int64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_int64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_long(ref GValue val);
		static d_g_value_get_long g_value_get_long = FuncLoader.LoadFunction<d_g_value_get_long>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_value_get_long_as_int(ref GValue val);
		static d_g_value_get_long_as_int g_value_get_long_as_int = FuncLoader.LoadFunction<d_g_value_get_long_as_int>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_long"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate ulong d_g_value_get_uint64(ref GValue val);
		static d_g_value_get_uint64 g_value_get_uint64 = FuncLoader.LoadFunction<d_g_value_get_uint64>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_uint64"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate UIntPtr d_g_value_get_ulong(ref GValue val);
		static d_g_value_get_ulong g_value_get_ulong = FuncLoader.LoadFunction<d_g_value_get_ulong>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_value_get_ulong_as_uint(ref GValue val);
		static d_g_value_get_ulong_as_uint g_value_get_ulong_as_uint = FuncLoader.LoadFunction<d_g_value_get_ulong_as_uint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_ulong"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_object(ref GValue val);
		static d_g_value_get_object g_value_get_object = FuncLoader.LoadFunction<d_g_value_get_object>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_object"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_param(ref GValue val);
		static d_g_value_get_param g_value_get_param = FuncLoader.LoadFunction<d_g_value_get_param>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_param"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_pointer(ref GValue val);
		static d_g_value_get_pointer g_value_get_pointer = FuncLoader.LoadFunction<d_g_value_get_pointer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_pointer"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_string(ref GValue val);
		static d_g_value_get_string g_value_get_string = FuncLoader.LoadFunction<d_g_value_get_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_string"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_value_get_uint(ref GValue val);
		static d_g_value_get_uint g_value_get_uint = FuncLoader.LoadFunction<d_g_value_get_uint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_uint"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_value_get_enum(ref GValue val);
		static d_g_value_get_enum g_value_get_enum = FuncLoader.LoadFunction<d_g_value_get_enum>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_enum"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_value_get_flags(ref GValue val);
		static d_g_value_get_flags g_value_get_flags = FuncLoader.LoadFunction<d_g_value_get_flags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_flags"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_strv_get_type();
		static d_g_strv_get_type g_strv_get_type = FuncLoader.LoadFunction<d_g_strv_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_strv_get_type"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_get_variant(ref GValue val);
		static d_g_value_get_variant g_value_get_variant = FuncLoader.LoadFunction<d_g_value_get_variant>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_get_variant"));
    }
}