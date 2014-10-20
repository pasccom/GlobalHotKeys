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

            internal struct Input
            {
                internal InputType type;
                internal InputUnion data;
                internal static int Size
                {
                    get { return Marshal.SizeOf(typeof(Input)); }
                }
            }

            [StructLayout(LayoutKind.Explicit)]
            internal struct InputUnion
            {
                [FieldOffset(0)]
                internal MouseInput mInput;
                [FieldOffset(0)]
                internal KeyboardInput kInput;
                [FieldOffset(0)]
                internal HardwareInput hInput;
            }

            internal struct MouseInput
            {
                internal int dx;
                internal int dy;
                internal int mouseData;
                internal MouseEventFlags flags;
                internal uint time;
                internal UIntPtr extraInfo;
            }

            internal struct KeyboardInput
            {
                internal short virtualCode;
                internal short scanCode;
                internal KeyboardEventFlags flags;
                internal int time;
                internal UIntPtr extraInfo;
            }

            internal struct HardwareInput
            {
                internal int message;
                internal short lowParam;
                internal short highParam;
            }

            internal enum ShowState
            {
                //Hide = 0, /*!< Hides the window and activates another window. */
                Normal = 1, /*!< Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time. */
                //ShowMinimized = 2, /*!< Activates the window and displays it as a minimized window. */
                ShowMaximized = 3, /*!< Activates the window and displays it as a maximized window. */
                //ShowNoActivate = 4, /*!< Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated. */
                //Show = 5, /*!< Activates the window and displays it in its current size and position. */
                //Minimize = 6, /*!< Minimizes the specified window and activates the next top-level window in the Z order. */
                //ShowMinNoActive = 7, /*!< Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated. */
                //ShowNA = 8, /*!< Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is not activated.*/
                Restore = 9, /*!< Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window. */
                Maximize = 10 /*!< Maximizes the specified window. */
                //ShowDefault = 10, /*!< Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application. */
                //ShowForceMinimize = 11; /*!< Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread. */
            }

            internal enum InputType : uint
            {
                MOUSE = 0, /*!< The event is a mouse event. Use the mi structure of the union. */
                KEYBOARD = 1, /*!< The event is a keyboard event. Use the ki structure of the union. */
                HARDWARE = 2 /*!< The event is a hardware event. Use the hi structure of the union. */
            }

            internal enum MouseEventFlags : uint
            {
                ABSOLUTE = 0x8000,
                HWHEEL = 0x01000,
                MOVE = 0x0001,
                MOVE_NOCOALESCE = 0x2000,
                LEFTDOWN = 0x0002,
                LEFTUP = 0x0004,
                RIGHTDOWN = 0x0008,
                RIGHTUP = 0x0010,
                MIDDLEDOWN = 0x0020,
                MIDDLEUP = 0x0040,
                VIRTUALDESK = 0x4000,
                WHEEL = 0x0800,
                XDOWN = 0x0080,
                XUP = 0x0100
            }

            internal enum KeyboardEventFlags : uint
            {
                EXTENDEDKEY = 0x0001,
                KEYUP = 0x0002,
                SCANCODE = 0x0008,
                UNICODE = 0x0004
            }

            internal delegate bool EnumWindowsCallback(IntPtr winHandle, IntPtr callbackParam);

            internal static int GetLastError()
            {
                return Marshal.GetLastWin32Error();
            }

            [DllImport("user32.dll")]
            internal static extern bool EnumThreadWindows(int tId, EnumWindowsCallback callback, IntPtr callbackParam);

            [DllImport("user32.dll", SetLastError=true)]
            internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

            [DllImport("user32.dll", SetLastError=true)]
            static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool attach);

            [DllImport("user32.dll")]
            internal static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

            [DllImport("user32.dll")]
            internal static extern bool ShowWindow(IntPtr winHandle, int cmd);

            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            private static extern bool SetForegroundWindow(IntPtr winHandle);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool BringWindowToTop(IntPtr winHandle);

            // The SetForegroundWindow() function seems not to work properly on windows 7
            // So this code does not work properly. Sometimes the windows gets only activated.
            // A trick from pinvoke.net follows.
            internal static void SetForeground(IntPtr winHandle)
            {
                uint foregroundWindowThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
                uint thisThread = Kernel32.GetCurrentThreadId();
                bool attached = true;

                if (foregroundWindowThread == 0)
                    throw new ApplicationException("Could not retrieve forgreound window owner thread. Error code: " + GetLastError());

                if ((foregroundWindowThread != thisThread) && !AttachThreadInput(foregroundWindowThread, thisThread, true))
                    attached = false;
                    //throw new ApplicationException("Could not attach to the foreground window thread. Error code: " + GetLastError());

                if (attached) {
                    if (!BringWindowToTop(winHandle))
                        throw new ApplicationException("BringWindowToTop() failed. Error code: " + GetLastError());
                } else {
                    if (!SetForegroundWindow(winHandle))
                        throw new ApplicationException("SetForegroundWindow() failded.");
                }

                if (attached && (foregroundWindowThread != thisThread) && !AttachThreadInput(foregroundWindowThread, thisThread, false))
                    throw new ApplicationException("Could not detach from the foreground window thread. Error code: " + GetLastError());
            }

            [DllImport("user32.dll")]
            internal static extern bool IsWindowVisible(IntPtr winHandle);

            [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
            internal static extern int GetWindowText(IntPtr winHandle, StringBuilder title, int len);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern int GetWindowTextLength(IntPtr winHandle);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool GetWindowRect(IntPtr winHandle, out Rect rect);
        }
    }
}
