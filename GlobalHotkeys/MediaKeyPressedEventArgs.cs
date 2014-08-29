using System;

namespace GlobalHotkeys
{
    /// <summary>
    /// Event Args for the event that is fired after a media key has been pressed.
    /// </summary>
    public class MediaKeyPressedEventArgs : EventArgs
    {
        public ApplicationCommand Command { get; private set; }
        internal MediaKeyPressedEventArgs(ApplicationCommand command) { Command = command; }
    }
}