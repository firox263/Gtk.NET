// TODO: Update with new project headers
// Original Copyright message:

// GLib.Type.cs - GLib GType class implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//          Andres G. Aragoneses <knocte@gmail.com>
//
// Copyright (c) 2003 Mike Kestner
// Copyright (c) 2003 Novell, Inc.
// Copyright (c) 2013 Andres G. Aragoneses
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GLib
{
    enum TypeFundamentals {
        TypeInvalid	    = 0 << 2,
        TypeNone	    = 1 << 2,
        TypeInterface   = 2 << 2,
        TypeChar	    = 3 << 2,
        TypeUChar	    = 4 << 2,
        TypeBoolean	    = 5 << 2,
        TypeInt		    = 6 << 2,
        TypeUInt	    = 7 << 2,
        TypeLong	    = 8 << 2,
        TypeULong	    = 9 << 2,
        TypeInt64	    = 10 << 2,
        TypeUInt64	    = 11 << 2,
        TypeEnum	    = 12 << 2,
        TypeFlags	    = 13 << 2,
        TypeFloat	    = 14 << 2,
        TypeDouble	    = 15 << 2,
        TypeString	    = 16 << 2,
        TypePointer	    = 17 << 2,
        TypeBoxed	    = 18 << 2,
        TypeParam	    = 19 << 2,
        TypeObject	    = 20 << 2,
        TypeVariant	    = 21 << 2
    }

    public static class GTypeExtensions
    {
        // GType equivalents to typeof() and GetType()
        public static GType gtypeof(Type type) => (GType)type;
        public static GType GetGType(this object o) => (GType)o.GetType();
    }

    // Simple GType struct
    public struct GType
    {
        // List of fundamental types
        public static readonly GType Invalid = new GType ((IntPtr) TypeFundamentals.TypeInvalid);
		public static readonly GType None = new GType ((IntPtr) TypeFundamentals.TypeNone);
		public static readonly GType Interface = new GType ((IntPtr) TypeFundamentals.TypeInterface);
		public static readonly GType Char = new GType ((IntPtr) TypeFundamentals.TypeChar);
		public static readonly GType UChar = new GType ((IntPtr) TypeFundamentals.TypeUChar);
		public static readonly GType Boolean = new GType ((IntPtr) TypeFundamentals.TypeBoolean);
		public static readonly GType Int = new GType ((IntPtr) TypeFundamentals.TypeInt);
		public static readonly GType UInt = new GType ((IntPtr) TypeFundamentals.TypeUInt);
		public static readonly GType Long = new GType ((IntPtr) TypeFundamentals.TypeLong);
		public static readonly GType ULong = new GType ((IntPtr) TypeFundamentals.TypeULong);
		public static readonly GType Int64 = new GType ((IntPtr) TypeFundamentals.TypeInt64);
		public static readonly GType UInt64 = new GType ((IntPtr) TypeFundamentals.TypeUInt64);
		public static readonly GType Enum = new GType ((IntPtr) TypeFundamentals.TypeEnum);
		public static readonly GType Flags = new GType ((IntPtr) TypeFundamentals.TypeFlags);
		public static readonly GType Float = new GType ((IntPtr) TypeFundamentals.TypeFloat);
		public static readonly GType Double = new GType ((IntPtr) TypeFundamentals.TypeDouble);
		public static readonly GType String = new GType ((IntPtr) TypeFundamentals.TypeString);
		public static readonly GType Pointer = new GType ((IntPtr) TypeFundamentals.TypePointer);
		public static readonly GType Boxed = new GType ((IntPtr) TypeFundamentals.TypeBoxed);
		public static readonly GType Param = new GType ((IntPtr) TypeFundamentals.TypeParam);
		public static readonly GType Object = new GType ((IntPtr) TypeFundamentals.TypeObject);
		public static readonly GType Variant = new GType ((IntPtr) TypeFundamentals.TypeVariant);

        static GType()
        {
            // Register fundamental types
            Register (GType.Char, typeof (sbyte));
			Register (GType.UChar, typeof (byte));
			Register (GType.Boolean, typeof (bool));
			Register (GType.Int, typeof (int));
			Register (GType.UInt, typeof (uint));
			Register (GType.Int64, typeof (long));
			Register (GType.UInt64, typeof (ulong));
			Register (GType.Float, typeof (float));
			Register (GType.Double, typeof (double));
			Register (GType.String, typeof (string));
			Register (GType.Pointer, typeof (IntPtr));
			Register (GType.Object, typeof (GLib.Object));
			Register (GType.Pointer, typeof (IntPtr));
			//Register (GType.Variant, typeof (GLib.Variant));
        }

        // Mapping between GType and C# type systems
        // TODO: Investigate whether the performance impact for reverse lookup
        // is negligible so we can save memory with only one dict.
        static Dictionary<IntPtr, Type> TypeDict = new Dictionary<IntPtr, Type>();
        static Dictionary<Type, GType> TypeDictReversed = new Dictionary<Type, GType>();

        private IntPtr typeid;

        public GType(IntPtr typeid)
        {
            this.typeid = typeid;
        }

        public static void Register(GType type, Type managedType)
        {
            if (!TypeDict.ContainsKey(type.typeid))
            {
                // Recursively register GObjects
                if (managedType.IsSubclassOf(typeof(GLib.Object)))
                {
                    Type baseType = managedType.BaseType;
                    if (managedType.BaseType != null)
                        Register(GLib.Object.ResolveGType(baseType), baseType);
                }

                // Add to typedict
                TypeDict.Add(type.typeid, managedType);
                TypeDictReversed.Add(managedType, type);

                // Announce
                Console.WriteLine($"Registering {managedType.FullName} in typedict");
            }
        }

        static GType ResolveGLibType(System.Type type)
        {
            GType gtype;

            // First check typedict
            if (TypeDictReversed.TryGetValue(type, out gtype))
                return gtype;
            
            // Check if subclass of GObject
            if (type.IsSubclassOf(typeof(GLib.Object)))
            {
                gtype = GLib.Object.ResolveGType(type);
                Register(gtype, type);
                return gtype;
            }
            
            // TODO:
            // - GType for Subclasses
            // - GType for Pointers/Opaque Values
            // - etc
            throw new NotImplementedException("Unimplemented GType!");
        }

        static System.Type ResolveManagedType(GType gtype)
        {
            // First check typedict
            if (TypeDict.TryGetValue(gtype.typeid, out Type type))
                return type;

            // If not
            throw new NotImplementedException("Type lookup not implemented");

            //return type;
        }

        // Casting
        public static explicit operator IntPtr (GType gtype) => gtype.typeid;
        public static explicit operator Type (GType gtype) => ResolveManagedType(gtype);
        public static explicit operator GType (Type type) => ResolveGLibType(type);

        // Compare two GTypes
        public static bool operator == (GType t1, GType t2) => (t1.typeid == t2.typeid);
        public static bool operator != (GType t1, GType t2) => (t1.typeid != t2.typeid);

        // Check type of object
        public static bool IsType(IntPtr obj, GType gtype)
		    => g_type_check_instance_is_a(obj, (IntPtr)gtype);

        public override bool Equals(object o)
		{
			if (!(o is GType))
				return false;

			return ((GType) o) == this;
		}

        public override int GetHashCode ()
		{
			return typeid.GetHashCode ();
		}

        // Ensure class_init has been called
        internal void EnsureClass()
		{
			if (g_type_class_peek(typeid) == IntPtr.Zero)
				g_type_class_ref(typeid);
		}

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_type_check_instance_is_a(IntPtr obj, IntPtr gtype);
		static d_g_type_check_instance_is_a g_type_check_instance_is_a = FuncLoader.LoadFunction<d_g_type_check_instance_is_a>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_check_instance_is_a"));

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_class_peek(IntPtr gtype);
		static d_g_type_class_peek g_type_class_peek = FuncLoader.LoadFunction<d_g_type_class_peek>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_class_peek"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_class_ref(IntPtr gtype);
		static d_g_type_class_ref g_type_class_ref = FuncLoader.LoadFunction<d_g_type_class_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_class_ref"));
    }
}