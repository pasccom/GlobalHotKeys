using System;
using System.Text;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Windows
    {
        class User32
        {
            internal struct Rect
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            internal static readonly int SW_HIDE = 0; /*!< Hides the window and activates another window. */
            internal static readonly int SW_SHOWNORMAL = 1; /*!< Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time. */
            internal static readonly int SW_SHOWMINIMIZED = 2; /*!< Activates the window and displays it as a minimized window. */
            internal static readonly int SW_SHOWMAXIMIZED = 3; /*!< Activates the window and displays it as a maximized window. */
            internal static readonly int SW_MAXIMIZE = 3; /*!< Maximizes the specified window. */
            internal static readonly int SW_SHOWNOACTIVATE = 4; /*!< Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated. */
            internal static readonly int SW_SHOW = 5; /*!< Activates the window and displays it in its current size and position. */
            internal static readonly int SW_MINIMIZE = 6; /*!< Minimizes the specified window and activates the next top-level window in the Z order. */
            internal static readonly int SW_SHOWMINNOACTIVE = 7; /*!< Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated. */
            internal static readonly int SW_SHOWNA = 8; /*!< Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is not activated.*/
            internal static readonly int SW_RESTORE = 9; /*!< Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window. */
            internal static readonly int SW_SHOWDEFAULT = 10; /*!< Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application. */
            internal static readonly int SW_FORCEMINIMIZE = 11; /*!< Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread. */
            
            internal delegate bool EnumWindowsCallback(IntPtr winHandle, IntPtr callbackParam);

            [DllImport("user32.dll")]
            internal static extern bool EnumThreadWindows(int tId, EnumWindowsCallback callback, IntPtr callbackParam);

            [DllImport("user32.dll")]
            internal static extern bool ShowWindow(IntPtr winHandle, int cmd);

            [DllImport("user32.dll")]
            internal static extern bool SetForegroundWindow(IntPtr winHandle);
           
            [DllImport("user32.dll")]
            internal static extern bool IsWindowVisible(IntPtr winHandle);

            [DllImport("user32.dll")]
            internal static extern int GetWindowText(IntPtr winHandle, StringBuilder title, int len);

            [DllImport("user32.dll")]
            internal static extern int GetWindowTextLength(IntPtr winHandle);

            [DllImport("user32.dll")]
            internal static extern bool GetWindowRect(IntPtr winHandle, out Rect rect);
        }
    }
}
