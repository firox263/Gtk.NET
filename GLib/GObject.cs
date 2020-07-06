using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;

namespace GLib
{
    // Stub Class (Inheritance Purposes only)
    [Wrapper]
    public class InitiallyUnowned : Object
    {
        public InitiallyUnowned() {}

        // Type Lookup
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_g_initially_unowned_get_type();
		static d_g_initially_unowned_get_type g_initially_unowned_get_type = FuncLoader.LoadFunction<d_g_initially_unowned_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_initially_unowned_get_type"));

        private static IntPtr GType => g_initially_unowned_get_type();
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
    public class Object : IDisposable
    {
        // Pointer to Unmanaged Type
        private IntPtr _handle = IntPtr.Zero;

        // We use the 'Handle' variable for lazy init
        public IntPtr Handle {
            get {
                if (!isInit)
                {
                    if (IsManaged(this.GetType()))
                        Console.WriteLine("Initialised user subclass. Add custom code here!");
                    
                    // When calling DefaultConstructor, we will
                    // always be owned by the managed assembly.
                    InitWrapper(defaultConstructor(), true);
                }
                return _handle;
            }
        }

        public static bool IsManaged(Type type)
            => !Attribute.IsDefined(type, typeof(WrapperAttribute));

        // Static Members
        static Dictionary<IntPtr, GLib.Object> wrappers = new Dictionary<IntPtr, GLib.Object>();

        // Instance Members
        List<Closure> signals = new List<Closure>();

        protected delegate IntPtr ConstructorDelegate();
        protected ConstructorDelegate defaultConstructor;

        protected bool isOwned = false;
        protected bool isInit = false;
        protected bool isSubclass = false;

        // Creates a new GObject wrapper
        // Actual assignment of the wrapper's pointer is
        // deferred until either InitWrapper() is called or the
        // Handle is requested (in which case a new object is
        // created).
        public Object()
        {
            // If we are not wrapping an existing GObject type,
            // then we are a subclass by definition
            isSubclass = IsManaged(this.GetType());

            // If not a subclass, register type
            // TODO: Is this necessary or just a waste or CPU time?
            if (!isSubclass)
                GType.Register(ResolveGType(this.GetType()), this.GetType());
            
            if (isSubclass)
            {
                // Make sure we initialise the class
                Console.WriteLine("Class Initialisation goes here!");
            }

            defaultConstructor = delegate() {
                return g_object_new_with_properties(GType.Object, 0, IntPtr.Zero, null);
            };
        }

        // Initialise the wrapper
        // IMPORTANT: We can only use static methods/fields here as we have
        // not yet initialised Handle
        protected void InitWrapper(IntPtr ptr, bool owned)
        {
            // Sanity Checks
            GLib.Object wrapper;
            if (wrappers.TryGetValue(ptr, out wrapper))
                throw new Exception($"Wrapper {ptr.ToString()} for type {wrapper.GetType().FullName} already exists!");

            if (!GType.IsType(ptr, GType.Object))
                throw new Exception("The pointer does not refer to an GObject");
            // End Checking

            if (ptr != IntPtr.Zero)
                _handle = ptr;

            this.isOwned = owned;
            if (isOwned)
                Take(_handle);

            // Add to Wrapper Dict
            wrappers.Add(_handle, this);

            // Announce Creation
            Type obj_type = this.GetType();
            Console.WriteLine("Instantiating " + (isSubclass ? "subclass " : "wrapper ") + obj_type.FullName);

            isInit = true;
        }

        // Wraps a pointer to a GObject with a native wrapper type
        // NOTE: generic type T is the type we want to cast as, NOT the
        // actual type of the object.
        public static T WrapPointerAs<T>(IntPtr ptr, bool owned)
            where T: GLib.Object, new()
        {
            // Sanity Check
            if (ptr == IntPtr.Zero)
                return null;

            // 1. Check if we already have a wrapper for this instance
            //    We should always have an existing wrapper for subclasses
            //    because the object must never outlive the wrapper.
            //
            // NOTE: If we for some reason need to create wrappers for user-defined
            // types in the future, it may be worth adding a GObjectArgs type to pass
            // to constructors to inhibit construction.
            GLib.Object wrapper;
            if (wrappers.TryGetValue(ptr, out wrapper))
            {
                if (owned && !wrapper.isOwned)
                    wrapper.Take();

                // Ensure it is possible to return the wrapper
                // as the requested type.
                bool isEqual = wrapper.GetType() == typeof(T);
                bool isSubclass = wrapper.GetType().IsSubclassOf(typeof(T));

                if (!(isEqual || isSubclass))
                    throw new InvalidCastException($"Cannot get pointer as {typeof(T).FullName}");

                return (T)wrapper;
            }

            // 2. No wrapper exists for this pointer. We assume that the pointer cannot be a
            //    user created managed subclass (this will always match the lifetime of the native
            //    object), so we initialise a new wrapper with the type of the pointer.
            GTypeInstance obj = (GTypeInstance)Marshal.PtrToStructure(ptr, typeof(GTypeInstance));
            GTypeClass klass = (GTypeClass)Marshal.PtrToStructure(obj.g_class, typeof(GTypeClass));
            System.Type realType = (Type)new GLib.GType(klass.gtype);

            // We cannot wrap a subclass
            // The subclass wrapper must always outlive the GObject and we
            // enforce this with ToggleRefs
            // TODO: Use ToggleRefs
            if (IsManaged(typeof(T)))
                throw new Exception("We cannot wrap a managed subclass. Something has gone very wrong");

            // 3. Create the wrapper type. The wrapper uses lazy initialisation to allow us to
            //    alternatively initialise it as a wrapper, so we call InitWrapper before Handle
            //    can be used.
            T ret = (T)Activator.CreateInstance(realType, null);
            
            ret.InitWrapper(ptr, owned);

            return ret;
        }
        
        // Disable 'field never used' warning
        #pragma warning disable 0649

        // Struct Definitions
        internal struct GTypeInstance {
			public IntPtr g_class;
		}

		internal struct GObject {
			public GTypeInstance type_instance;
			public uint ref_count;
			public IntPtr qdata;
		}

        internal struct GTypeClass {
			public IntPtr gtype;
		}

        #pragma warning restore 0649

        public static GType ResolveGType(Type type)
        {
            if (type == typeof(GLib.Object))
                return GType.Object;

            // Make sure we are dealing with a subclass of GObject
            if (!type.IsSubclassOf(typeof(GLib.Object)))
                throw new Exception("Must be a GObject or subclass of GObject");

            if (IsManaged(type))
            {
                // Subclass type
                throw new NotImplementedException("GType Lookup is not yet implemented for user subclassed objects");
            }
            else
            {
                // Wrapper type, so lookup 'GType Property'
                PropertyInfo pi = type.GetProperty ("GType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (pi == null)
                    throw new Exception("Could not retrieve GType: fatal error");
                return new GType((IntPtr)pi.GetValue(null));
            }
        }

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

        ~Object() => Dispose(false);

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
            value.Set(val);
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