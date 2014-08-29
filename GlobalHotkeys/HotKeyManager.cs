using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GlobalHotkeys
{
    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

    /// <summary>
    /// Encapsulates a native Window and provides Events for consumers to subscribe to.
    /// TODO: Currently this requires the MTA (Multi Threaded Applicaiton) Profile.
    /// </summary>
    public sealed class HotKeyManager : IDisposable
    {
        // Winapi Imports
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Variables
        private readonly Window window = new Window();
        private int currentId = 0;

        // Events
        public event EventHandler<HotKeyPressedEventArgs> HotKeyPressed;

        public HotKeyManager()
        {
            // register the event of the inner native window.
            window.HotKeyPressed += delegate(object sender, HotKeyPressedEventArgs args)
            {
                if (HotKeyPressed != null) { HotKeyPressed(this, args); }
            };
        }

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = currentId; i > 0; i--) { UnregisterHotKey(window.Handle, i); }

            // dispose the inner native window.
            window.Dispose();
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            // TODO: Think about creating a Hotkey Struct and keeping track of the already registered HotKeys
            // increment the counter.
            currentId++;

            // register the hot key.
            if (!RegisterHotKey(window.Handle, currentId, (uint)modifier, (uint)key))
            {
                throw new InvalidOperationException("Couldn’t register the hot key.");
            }
        }

        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// TODO: Think about encapsulating this inside of a WindowManager so we can use one window in multiple classes.
        /// </summary>
        private sealed class Window : NativeWindow, IDisposable
        {
            private static int WM_HOTKEY = 0x0312;
            public event EventHandler<HotKeyPressedEventArgs> HotKeyPressed;

            // Setup & Cleanup
            public Window() { this.CreateHandle(new CreateParams()); }
            public void Dispose() { this.DestroyHandle(); }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m">The Windows Message</param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    if (HotKeyPressed != null) { HotKeyPressed(this, new HotKeyPressedEventArgs(modifier, key)); }
                }

                // TODO: Implement Media Key Handling
            }
        }

    }
}