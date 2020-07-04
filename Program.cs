using System;
using System.Diagnostics;

using GLib;
using Gtk;

public class MyWindow : Window
{
    public MyWindow(GObjectArgs args) : base(args) {}

    public MyWindow()
    {
        this.SetTitle("GTK.NET Demo 2");
        this.DefaultWidth = 800;
        this.DefaultHeight = 600;

        this.Destroy += OnDestroy;

        Button btn = new Button("Another Button!");
        btn.Clicked += OnButtonClick;
        
        Add(btn);
        ShowAll();
    }

    // TODO: Use static constructor for class initialisation?

    void OnDestroy(object sender, DestroySignalArgs e)
    {
        Console.WriteLine("Guten Tag!");
        Global.GtkMainQuit();
    }

    void OnButtonClick(object sender, SignalArgs e)
    {
        Console.WriteLine("Hello Inheritance!");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Global.GtkInit();

        //Window.SetInteractiveDebugging(true);

        /*Window win = new Window(WindowType.Toplevel);
        win.SetTitle("GTK.NET Demo");
        win.DefaultWidth = 800;
        win.DefaultHeight = 600;

        win.Destroy += delegate(object sender, Window.DestroySignalArgs e) {
            Console.WriteLine("Bye!");
            Global.GtkMainQuit();
        };

        Button btn = new Button("Click!");
        btn.Clicked += delegate(object btn, SignalArgs e) {
            Console.WriteLine("Hello World!");
        };

        win.Add(btn);

        Window winRef = (Window)btn.GetToplevel();
        Window winRef2 = (Window)btn.GetToplevel();

        Debug.Assert(winRef == win && winRef2 == win && winRef2 == winRef);

        winRef.ShowAll();
        winRef.Present();*/

        MyWindow win = new MyWindow();
        win.Present();
        Global.GtkMain();
    }
}

/*
    - Signal Handling
    - Packing
    - Subclassing
*/

// STATUS: Presents a window
//
// NEXT STEPS:
//  - Investigate into Glib plumbing
//  - We want to support the following:
//     => Subclassing
//     => Interfaces
//     => Virtual Functions
//     => GType interaction/registration
//
//  - Goals:
//     => To create a easier-to-maintain set of bindings for Gtk
//     => Integration with gobject-introspection
//     => Later on, investigate native C# integrations:
//         -> e.g. System.Drawing, XAML, etc