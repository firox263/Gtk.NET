using System;

namespace GLib
{
    // GLib.WrapperAttribute
    // ============
    // This attribute designates a type as a wrapper as opposed to a
    // custom managed subclass. This is used to determine whether or
    // not a given GObject subclass is wrapping an existing type
    // or defining a new one.
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class WrapperAttribute : Attribute {}
}