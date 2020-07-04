using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace GLib
{
    public struct GObjectArgs
    {
        internal IntPtr ptr;
        internal bool owned;

        public GObjectArgs(IntPtr ptr, bool owned = false)
        {
            this.ptr = ptr;
            this.owned = owned;
        }
    }

    // Stub Class (Inheritance Purposes only)
    public class GInitiallyUnowned : GObject
    {
        public GInitiallyUnowned() {}
        public GInitiallyUnowned(GObjectArgs args) : base(args) {}
    }

    // GLib.GObject
    // ============
    // This class represents one single ref-counted reference
    // to a GObject. When this class is disposed of by the GC,
    // the reference is released.
    // 
    // Our initial naive implementation assumes there can only
    // ever be one C# wrapper for a GObject instance at any one
    // time.
    public class GObject : IDisposable
    {
        // Wrapper to Unmanaged Type
        protected IntPtr _handle = IntPtr.Zero;
        public IntPtr Handle {
            get => _handle;
        }

        // Static Members
        static Dictionary<IntPtr, GObject> wrappers = new Dictionary<IntPtr, GObject>();

        // Instance Members
        List<Closure> signals = new List<Closure>();

        protected bool isOwned = false;
        protected bool isInit = false;

        // Create a new GObject
        // This will *always* be owned by the managed assembly
        public GObject()
        {
            if (GetType() != typeof(GObject))
            {
                return;
            }
            
            _handle = g_object_new_with_properties(GType.Object, 0, IntPtr.Zero, null);
            Init(_handle, true);
        }

        // Wraps an existing GObject
        // We do not assume ownership by default
        public GObject(GObjectArgs args) => Init(args.ptr, args.owned);

        protected void Init(IntPtr ptr, bool owned)
        {
            // Sanity Check
            GObject obj;
            if (wrappers.TryGetValue(ptr, out obj))
                throw new Exception($"Wrapper {ptr.ToString()} for type {obj.GetType().FullName} already exists!");

            // TODO: Make sure the ptr actually is a GObject, otherwise
            // this is a fatal error.

            if (ptr != IntPtr.Zero)
                _handle = ptr;

            this.isOwned = owned;
            if (isOwned)
                Take();

            // TODO: Check if we are a subclass?

            // Add to Wrapper Dict
            wrappers.Add(Handle, this);

            // Announce Creation
            Type obj_type = this.GetType();
            Console.WriteLine("Instantiating " + obj_type.FullName);

            isInit = true;
        }

        // Wraps a pointer to a GObject with a native wrapper type
        // NOTE: generic type T is the type we want to cast as,
        // NOT the actual type of the object.
        public static T WrapPointer<T>(IntPtr ptr, bool owned)
            where T: GObject, new()
        {
            // Sanity Check
            if (ptr == IntPtr.Zero)
                return null;

            // 1. Check if we already have a wrapper for this instance.
            // TODO: Make this sound for subclasses later on
            GObject obj;
            if (wrappers.TryGetValue(ptr, out obj))
            {
                if (owned && !obj.isOwned)
                    obj.Take();

                if (obj.GetType() != typeof(T))
                    throw new Exception($"Cannot get pointer as {typeof(T).FullName}");

                return (T)obj;
            }

            // 2. Lookup GType for ptr
            // TODO: Use GType
            System.Type objType = typeof(T);

            // Check if the type is a wrapper type or a subclass
            // Use threshold type for this? 

            // If we are a wrapper, we can assume there will
            // be a `new(GObjectArgs args)` constructor
            // TODO: Find a better method for this... whatever works?
            return (T)Activator.CreateInstance(
                objType,
                new object[] { new GObjectArgs(ptr, owned) }
            );
        }

        // Lookup GType
        /*protected GType LookupGType ()
		{
			if (Handle != IntPtr.Zero) {
				GTypeInstance obj = (GTypeInstance) Marshal.PtrToStructure (Handle, typeof (GTypeInstance));
				GTypeClass klass = (GTypeClass) Marshal.PtrToStructure (obj.g_class, typeof (GTypeClass));
				return new GLib.GType (klass.gtype);
			} else {
				return LookupGType (GetType ());
			}
		}

		protected internal static GType LookupGType (System.Type t)
		{
			return GType.LookupGObjectType (t);
		}*/

        // Steals the reference and makes it owned by the managed wrapper
        protected void Take()
        {
            if (Handle == IntPtr.Zero)
                throw new Exception("This wrapper type does not refer to a native object");

            bool floating = g_object_is_floating(Handle);
            if (floating)
                g_object_ref_sink(Handle);
            else
                g_object_ref(Handle);

            isOwned = true;
        }

        ~GObject() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (isOwned && Handle != IntPtr.Zero)
                g_object_unref(Handle);

            // if (disposing) { ... }
            // Free any IDisposable resources owned
            // by this GObject instance?
        }

        
        // TODO: Look into toggle refs

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
        delegate IntPtr d_g_object_new_with_properties(GType object_type, uint n_properties, IntPtr names, GValue[] values);
        static d_g_object_new_with_properties g_object_new_with_properties = FuncLoader.LoadFunction<d_g_object_new_with_properties>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_new_with_properties"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate bool d_g_object_is_floating(IntPtr handle);
        static d_g_object_is_floating g_object_is_floating = FuncLoader.LoadFunction<d_g_object_is_floating>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_is_floating"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_g_object_ref(IntPtr handle);
        static d_g_object_ref g_object_ref = FuncLoader.LoadFunction<d_g_object_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_ref"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void d_g_object_unref(IntPtr handle);
        static d_g_object_unref g_object_unref = FuncLoader.LoadFunction<d_g_object_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_unref"));

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate IntPtr d_g_object_ref_sink(IntPtr handle);
        static d_g_object_ref_sink g_object_ref_sink = FuncLoader.LoadFunction<d_g_object_ref_sink>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_ref_sink"));

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