# Gtk.NET
A minimal idiomatic C# wrapper around GTK and the GObject type system.

## Rationale
GtkSharp is generated using a codegen tool called GAPI. Modern GTK bindings
should ideally be generated using the gobject-introspection infrastructure.

This is an attempt to rewrite GtkSharp in a way that is easier to maintain,
while still providing the complete GTK feature set to consumers of the
bindings.

## Architecture
Ideally the bindings will consist of three components:
 - The raw native API, which serves to create a usable, but not particularly
   ergonomic, layer over GTK. This should be automatically generated from
   gir sources.
 - A manually maintained convenience layer which wraps GTK structures and
   functions in a more natural C#-like API.
 - A language integration library which allows for other C#/.NET ecosystem
   libraries to interface with Gtk.

## Consumers
The primary consumer for this library will be Bluetype, a word-processor
for students and authors. This will be a rewrite of my school project
'BluEdit' in C# to both drive development of the bindings and make it
easier to iterate on.