using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GlobalHotkeys
{
    /// <summary>
    /// Encapsulates a native Window and provides Events for consumers to subscribe to.
    /// TODO: Currently this requires the MTA (Multi Threaded Applicaiton) Profile.
    /// TODO: Think about splitting this into MediaKeyManager and HotKeyManager, also move the internal Window somewhere else
    /// </summary>
    public sealed class HotKeyManager : IDisposable
    {
        // Winapi Imports
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Variables
        private readonly Window window = new Window();
        internal IntPtr Handle { get { return window.Handle; } }
        private int currentId = 0;

        // Events
        public event EventHandler<HotKeyPressedEventArgs> HotKeyPressed;
        public event EventHandler<MediaKeyPressedEventArgs> MediaKeyPressed;

        public HotKeyManager()
        {
            // register the event of the inner native window.
            window.HotKeyPressed += delegate(object sender, HotKeyPressedEventArgs args)
            {
                if (HotKeyPressed != null) { HotKeyPressed(this, args); }
            };

            window.MediaKeyPressed += delegate(object sender, MediaKeyPressedEventArgs args)
            {
                if (MediaKeyPressed != null) { MediaKeyPressed(this, args); }
            };
        }

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = currentId; i > 0; i--) { UnregisterHotKey(Handle, i); }

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
            if (!RegisterHotKey(Handle, currentId, (uint)modifier, (uint)key))
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
            // Winapi Imports
            [DllImport("user32.dll")]
            public static extern bool RegisterShellHookWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern bool DeregisterShellHookWindow(IntPtr hWnd);
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern uint RegisterWindowMessage(string lpString);

            // Events
            public event EventHandler<HotKeyPressedEventArgs> HotKeyPressed;
            public event EventHandler<MediaKeyPressedEventArgs> MediaKeyPressed;

            // Variables
            private readonly uint shellMessageID;

            // Setup & Cleanup
            public Window()
            {
                this.CreateHandle(new CreateParams());

                // Register the ShellHook so we can receive all AppCommands even if we don't have focus. 
                // DOCUMENTATION: http://msdn.microsoft.com/en-us/library/ms911004.aspx
                shellMessageID = RegisterWindowMessage("SHELLHOOK");

                // Register the Window that receives the shell Messages. 
                // DOCUMENTATION: http://msdn.microsoft.com/en-us/library/windows/desktop/ms644989%28v=vs.85%29.aspx
                RegisterShellHookWindow(Handle);
            }
            public void Dispose()
            {
                // Cleanup our ShellHook
                DeregisterShellHookWindow(Handle);
                this.DestroyHandle();
            }

            /// <summary>
            /// Windows Message Loop
            /// </summary>
            /// <param name="m">The Windows Message</param>
            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    // Hot Key Handling
                    case Constants.WM_HOTKEY:
                    {
                        // get the keys.
                        Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                        ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                        // invoke the event to notify the parent.
                        if (HotKeyPressed != null) { HotKeyPressed(this, new HotKeyPressedEventArgs(modifier, key)); }
                    }
                    break;

                    // Media Key Handling
                    case Constants.WM_APPCOMMAND:
                    {
                        // http://msdn.microsoft.com/en-gb/library/windows/desktop/ms646275%28v=vs.85%29.aspx
                        // TODO: Fix the LParam handling and provide a method that resembles the GET_APPCOMMAND_LPARAM MACRO
                        ApplicationCommand command = (ApplicationCommand)m.LParam.ToInt32();
                        switch (command)
                        {
                            case ApplicationCommand.VolumeMute:
                            case ApplicationCommand.VolumeDown:
                            case ApplicationCommand.VolumeUp:
                            case ApplicationCommand.MediaNexttrack:
                            case ApplicationCommand.MediaPrevioustrack:
                            case ApplicationCommand.MediaStop:
                            case ApplicationCommand.MediaPlayPause:
                            case ApplicationCommand.Close:
                            case ApplicationCommand.MediaPlay:
                            case ApplicationCommand.MediaPause:
                            case ApplicationCommand.MediaFastForward:
                            case ApplicationCommand.MediaRewind:
                            {
                                // invoke the event to notify the parent.  
                                if (MediaKeyPressed != null) { MediaKeyPressed(this, new MediaKeyPressedEventArgs(command)); }

                                // According to MSDN, we should return true if we are handling this message.
                                m.Result = new IntPtr(1);
                                base.WndProc(ref m);
                                return;
                            }
                        }
                    }
                    break;

                    // Pass all unhandled Messages to the default Window Proc
                    default:
                    {
                        // A ShellHook message
                        if (m.Msg == shellMessageID)
                        {
                            // We only care about the HSHELL_APPCOMMAND = 12
                            if (m.WParam.ToInt32() == Constants.HSHELL_APPCOMMAND) { goto case Constants.WM_APPCOMMAND; }
                        }

                        // All unhandeled messages the default proc should process
                        base.WndProc(ref m);
                    }
                    break;
                }
            }
        }

    }
}