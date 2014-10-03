using System;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class User32
        {
            internal static readonly uint WM_HOTKEY = 0x0312; /*!< Hotkey message identifier */
            internal static readonly int MOD_NOREPEAT = 0x4000; /*!< Changes the hotkey behavior so that the keyboard auto-repeat does not yield multiple hotkey notifications. */

            internal struct Point
            {
                public long x;
                public long y;
            }

            internal struct MSG
            {
                public IntPtr hwnd;
                public uint message;
                public UIntPtr wParam;
                public IntPtr lParam;
                public uint time;
                public Point pt;
            }

            [DllImport("user32.dll")]
            internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
            [DllImport("user32.dll")]
            internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
            [DllImport("user32.dll")]
            internal static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        }
    }
}
