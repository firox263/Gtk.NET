using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace Gtk
{
    class GObject
    {
        protected IntPtr _handle;
        public IntPtr Handle {
            get => _handle;
        }

        List<Closure> signals = new List<Closure>();

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

        // TODO: USE DISPOSE PATTERN
        // We want to ref() an object when we instantiate the CLR type
        // and unref() upon distruction/disposal by the GC
    }
}