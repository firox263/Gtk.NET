using System;

using GLib;
using Gtk;

public class MyWindow : Window
{
    Box box;

    public MyWindow()
    {
        this.SetTitle("GTK.NET Demo 2");
        this.DefaultWidth = 800;
        this.DefaultHeight = 600;

        this.Destroy += OnDestroy;

        box = new Box(Orientation.Vertical, 0);

        TextView textView = new TextView();
        box.PackStart(textView, true, true, 0);

        Button btn = new Button("Click!");
        btn.Clicked += OnButtonClick;
        box.PackStart(btn, false, true, 0);
        
        Add(box);

        MyWindow win = (MyWindow)btn.GetToplevel();

        win.ShowAll();
    }

    void OnDestroy(object sender, DestroySignalArgs e)
    {
        Console.WriteLine("Bye!");
        Global.GtkMainQuit();
    }

    void OnButtonClick(object sender, SignalArgs e)
    {
        Console.WriteLine("Hello!");
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