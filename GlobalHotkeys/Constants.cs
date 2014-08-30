using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalHotkeys
{
    /// <summary>
    /// Native Constants, for the WinApi
    /// </summary>
    internal static class Constants
    {
        public const int HSHELL_APPCOMMAND = 12;
        public const int WM_HOTKEY = 0x0312;
        public const int WM_APPCOMMAND = 0x0319;
    }

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
    /// Possible Media Key Actions we respond to
    /// </summary>
    public enum ApplicationCommand
    {
        VolumeMute = 8,
        VolumeDown = 9,
        VolumeUp = 10,
        MediaNexttrack = 11,
        MediaPrevioustrack = 12,
        MediaStop = 13,
        MediaPlayPause = 14,
        Close = 31,
        MediaPlay = 46,
        MediaPause = 47,
        MediaFastForward = 49,
        MediaRewind = 50
    }
}
