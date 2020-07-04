using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace GLib
{
    // Stub Class (Inheritance Purposes only)
    public class GInitiallyUnowned : GObject
    {
        public GInitiallyUnowned() {}
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
    //
    // The GObject can be either owned or unowned. If owned, we hold
    // a strong reference to the unmanaged object, and can prevent it
    // from being destroyed. Otherwise, we have a weak reference and
    // can use it but must check that it still exists.
    //
    // FOR WRAPPER TYPES: Additionally, the GObject uses lazy initialisation. We
    // defer creation of the unmanaged object when the constructor is called. If
    // the GObject is to wrap an existing object, rather than create one, the
    // `InitWrapper()` function can be called manually by WrapPointer(). Thus we
    // can avoid subclasses having to define multiple constructors. This is done
    // purely for API-cleanliness.
    //
    // Since we do not want to impose arbitrary restrictions on user-subclassed objects
    // being able to use the constructor, mostly for readability purposes, we instead use a
    // ToggleRef based system to ensure that we will never be in a position where we need
    // to recreate a wrapper for an object with custom state. The subclass wrapper will exist
    // for the entire lifetime of the managed subclass object.
    public class GObject : IDisposable
    {
        // Pointer to Unmanaged Type
        private IntPtr _handle = IntPtr.Zero;

        // We use the 'Handle' variable for lazy init
        public IntPtr Handle {
            get {
                if (!isInit)
                    // When calling DefaultConstructor, we will
                    // always be owned by the managed assembly.
                    InitWrapper(defaultConstructor(), true);
                return _handle;
            }
        }

        // Static Members
        static Dictionary<IntPtr, GObject> wrappers = new Dictionary<IntPtr, GObject>();

        // Instance Members
        List<Closure> signals = new List<Closure>();

        protected delegate IntPtr DefaultConstructor();
        protected DefaultConstructor defaultConstructor;

        protected bool isOwned = false;
        protected bool isInit = false;
        protected bool isSubclass = false;

        // Creates a new GObject wrapper
        // Actual assignment of the wrapper's pointer is
        // deferred until either InitWrapper() is called or the
        // Handle is requested (in which case a new object is
        // created).
        public GObject()
        {
            if (GetType() != typeof(GObject))
                isSubclass = true;

            defaultConstructor = delegate() {
                return g_object_new_with_properties(GType.Object, 0, IntPtr.Zero, null);
            }; 
        }

        // Initialise the wrapper
        // We cannot use Handle in here as it may not
        // be initialised yet.
        protected void InitWrapper(IntPtr ptr, bool owned)
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
                Take(_handle);

            // TODO: Check if we are a subclass?

            // Add to Wrapper Dict
            wrappers.Add(_handle, this);

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

            // IMPORTANT !!!
            // TODO: Check if the type is a wrapper type or a subclass
            // Use threshold type for this?
            // If subclass, this is a fatal error
            // The subclass wrapper should live for the
            // lifetime of the subclassed object.

            T ret = (T)Activator.CreateInstance(
                objType,
                null // new object[] {}
            );

            ret.InitWrapper(ptr, owned);

            return ret;
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
        protected void Take() => Take(Handle);

        // Allows for a custom IntPtr to be passed in,
        // so we can use it in Handle's getter
        protected void Take(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new Exception("This wrapper type does not refer to a native object");

            bool floating = g_object_is_floating(handle);
            if (floating)
                g_object_ref_sink(handle);
            else
                g_object_ref(handle);

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