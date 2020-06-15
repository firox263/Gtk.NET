using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace GLib
{
    class GObject
    {
        protected IntPtr _handle;
        public IntPtr Handle {
            get => _handle;
        }

        List<Closure> signals = new List<Closure>();

        protected GObject()
        {
            // GType Registration
            Type obj_type = this.GetType();
            Console.WriteLine("Instantiating " + obj_type.FullName);

            // Check if this is a wrapper type or a managed type
            // If managed, subclass as appropriate, otherwise set
            // is_wrapper to true or something?
        }

        protected void ConnectSignal<T>(SignalHandler<T> callback, string signal_name)
            where T: SignalArgs, new()
        {
            /*Closure.ClosureFromDelegate*/
            Closure signal_callback = new Closure<T>(callback, this, signal_name);
            signals.Add(signal_callback);
        }

        protected void DisconnectSignal(Delegate callback)
        {
            throw new NotImplementedException();
        }

        protected void SetProperty<T>(string prop_name, T val)
        {
            GValue value = new GValue(typeof(T));
            value.Set<T>(val);
            g_object_set_property(Handle, Utils.StringToPtrGStrdup(prop_name), ref value);
            value.Dispose();
        }

        protected T GetProperty<T>(string prop_name)
        {
            GValue value = new GValue(typeof(T));
            g_object_get_property(Handle, Utils.StringToPtrGStrdup(prop_name), ref value);
            object ret = value.Get<T>();
            value.Dispose();

            // Checking
            if (typeof(T) != ret.GetType())
                throw new Exception();
            
            return (T)ret;
        }

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_g_object_set_property(IntPtr obj, IntPtr gchar, ref GValue val);
        static d_g_object_set_property g_object_set_property = FuncLoader.LoadFunction<d_g_object_set_property>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_set_property"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_g_object_get_property(IntPtr obj, IntPtr property_name, ref GValue value);
        static d_g_object_get_property g_object_get_property = FuncLoader.LoadFunction<d_g_object_get_property>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_get_property"));

        // TODO: USE DISPOSE PATTERN
        // We want to ref() an object when we instantiate the CLR type
        // and unref() upon distruction/disposal by the GC
    }
}