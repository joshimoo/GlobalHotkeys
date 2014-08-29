using System;
using System.Windows.Forms;

namespace GlobalHotkeys
{
    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class HotKeyPressedEventArgs : EventArgs
    {
        public ModifierKeys Modifier { get; private set; }
        public Keys Key { get; private set; }

        internal HotKeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            Modifier = modifier;
            Key = key;
        }
    }
}