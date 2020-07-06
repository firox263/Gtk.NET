using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace GLib
{
    // Closure with generic type arguments
    // This should always be used
    class Closure<TArgs> : Closure
        where TArgs: SignalArgs, new()
    {
        static Hashtable ClosureTable = new Hashtable();

        SignalHandler<TArgs> callback;
        ClosureMarshal marshaller;
        string signal_name;
        GLib.Object obj;

        public Closure(SignalHandler<TArgs> callback, GLib.Object obj, string signal_name)
        {
            this.obj = obj;
            this.callback = callback;
            this.signal_name = signal_name;
            this.marshaller = new ClosureMarshal(SignalMarshaller);

            IntPtr raw_closure = g_closure_new_simple(Marshal.SizeOf(typeof(GClosure)), IntPtr.Zero);
            g_closure_set_marshal(raw_closure, marshaller);
            g_signal_connect_closure(obj.Handle, Utils.StringToPtrGStrdup(signal_name), raw_closure, false);

            ClosureTable[raw_closure] = this;

            // Value[] values = GetValues();
            // Do we want to populate args here or in signal_marshaller?
            // Perhaps marshaller is better
            // TODO: Perf profiling
        }

        void SignalMarshaller(IntPtr raw_closure, IntPtr return_val, uint n_param_vals, IntPtr param_values, IntPtr invocation_hint, IntPtr marshal_data)
        {
            // Call SignalHandler
            SignalHandler<TArgs> handler = (SignalHandler<TArgs>)this.callback;
            if (handler != null)
            {
                // TODO: Populate TArgs with GValues
                TArgs args = new TArgs();
                handler(this.obj, args);
            }
        }
    }

    // We have the non-generic Closure class to contain FFI-related
    // functions. This way we can avoid generics becoming problematic
    // while still getting nice idiomatic C# SignalArgs.
    class Closure
    {
        // Purely for sizeof(GClosure)
        // Is there a better way to do this?
        [StructLayout(LayoutKind.Sequential)]
        protected struct GClosure {
            public long fields;
            public IntPtr marshaler;
            public IntPtr data;
            public IntPtr notifiers;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void ClosureMarshal (IntPtr closure, IntPtr return_val, uint n_param_vals, IntPtr param_values, IntPtr invocation_hint, IntPtr marshal_data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate IntPtr d_g_closure_new_simple(int closure_size, IntPtr dummy);
        protected static d_g_closure_new_simple g_closure_new_simple = FuncLoader.LoadFunction<d_g_closure_new_simple>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_closure_new_simple"));

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void d_g_closure_set_marshal(IntPtr closure, ClosureMarshal marshaler);
        protected static d_g_closure_set_marshal g_closure_set_marshal = FuncLoader.LoadFunction<d_g_closure_set_marshal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_closure_set_marshal"));

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate uint d_g_signal_connect_closure(IntPtr obj, IntPtr name, IntPtr closure, bool is_after);
        protected static d_g_signal_connect_closure g_signal_connect_closure = FuncLoader.LoadFunction<d_g_signal_connect_closure>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_connect_closure"));
    }
}