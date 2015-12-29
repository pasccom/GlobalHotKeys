using System;
using System.Text;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Windows
    {
        /// <summary>
        ///     Imports from <c>user32.dll</c>
        /// </summary>
        class User32
        {
            /// <summary>
            ///     Stores a rectangle.
            /// </summary>
            /// <remarks>
            ///     See the RECT structure on MSDN.
            /// </remarks>
            internal struct Rect
            {
                /// <summary>X coordinate of the left side</summary>
                public int Left;
                /// <summary>Y coordinate of the top side</summary>
                public int Top;
                /// <summary>X coordinate of the right side</summary>
                public int Right;
                /// <summary>Y coordinate of the bottom side</summary>
                public int Bottom;
            }

            /// <summary>
            ///     Used by <see cref="SendInput"/> to store information for synthesizing input events such as keystrokes, mouse movement, and mouse clicks.
            /// </summary>
            /// <remarks>
            ///     See the INPUT structure on MSDN. The documentation was copied from there.
            /// </remarks>
            internal struct Input
            {
                /// <summary>The type of the input event</summary>
                internal InputType type;
                /// <summary>The information about a simulated event</summary>
                internal InputUnion data;
                /// <summary>Added property: returns the size of the structure</summary>
                internal static int Size
                {
                    get { return Marshal.SizeOf(typeof(Input)); }
                }
            }

            /// <summary>
            ///     Stores the data associated with input events.
            /// </summary>
            /// <remarks>
            ///     See the INPUT structure on MSDN. The documentation was copied from there.
            /// </remarks>
            [StructLayout(LayoutKind.Explicit)]
            internal struct InputUnion
            {
                /// <summary>The information about a simulated mouse event.</summary>
                [FieldOffset(0)]
                internal MouseInput mInput;
                /// <summary>The information about a simulated keyboard event.</summary>
                [FieldOffset(0)]
                internal KeyboardInput kInput;
                /// <summary>The information about a simulated hardware event.</summary>
                [FieldOffset(0)]
                internal HardwareInput hInput;
            }

            /// <summary>
            ///     Stores information about a simulated mouse event.</summary>
            /// </summary>
            /// <remarks>
            ///     See the MOUSEINPUT structure on MSDN. The documentation was copied from there.
            /// </remarks>
            internal struct MouseInput
            {
                /// <summary>The X coordinate of the absolute position of the mouse, or the amount of motion since the last mouse event was generated</summary>
                internal int dx;
                /// <summary>The Y coordinate of the absolute position of the mouse, or the amount of motion since the last mouse event was generated</summary>
                internal int dy;
                /// <summary>Additionnal information about the mouse event</summary>
                internal int mouseData;
                /// <summary>A set of bit flags that specify various aspects of mouse motion and button clicks</summary>
                internal MouseEventFlags flags;
                /// <summary>The time stamp for the event, in milliseconds</summary>
                internal uint time;
                /// <summary>An additional value associated with the mouse event</summary>
                internal UIntPtr extraInfo;
            }

            /// <summary>
            ///     Stores information about a simulated keyboard event.</summary>
            /// </summary>
            /// <remarks>
            ///     See the KEYBDINPUT structure on MSDN. The documentation was copied from there.
            /// </remarks>
            internal struct KeyboardInput
            {
                /// <summary>The virtual key code</summary>
                internal short virtualCode;
                /// <summary>The key scan code</summary>
                internal short scanCode;
                /// <summary>Specifies various aspects of a keystroke</summary>
                internal KeyboardEventFlags flags;
                /// <summary>The time stamp for the event, in milliseconds</summary>
                internal int time;
                /// <summary>An additional value associated with the keystroke</summary>
                internal UIntPtr extraInfo;
            }

            /// <summary>
            ///     Stores information about a simulated harware event.</summary>
            /// </summary>
            /// <remarks>
            ///     See the HARDWAREINPUT structure on MSDN. The documentation was copied from there.
            /// </remarks>
            internal struct HardwareInput
            {
                /// <summary>The message generated by the input hardware</summary>
                internal int message;
                /// <summary>The low-order word of the <c>long</c> parameter for the message</summary>
                internal short lowParam;
                /// <summary>The high-order word of the <c>long</c> parameter for the message</summary>
                internal short highParam;
            }

            /// <summary>
            ///     Show flags. Used for the <see cref="ShowWindow"/> function.
            /// </summary>
            /// <remarks>
            ///     See the ShowWindow function on MSDN. The documentation was copied from there.
            /// </remarks>
            internal enum ShowState
            {
                ///// <summary>Hides the window and activates another window.</summary>
                //Hide = 0,
                /// <summary>Activates and displays a window. </summary>
                Normal = 1,
                ///// <summary>Activates the window and displays it as a minimized window.</summary>
                //ShowMinimized = 2,
                /// <summary>Activates the window and displays it as a maximized window.</summary>
                ShowMaximized = 3,
                ///// <summary>Displays a window in its most recent size and position (but does not activate it).</summary>
                //ShowNoActivate = 4,
                ///// <summary>Activates the window and displays it in its current size and position.</summary>
                //Show = 5,
                ///// <summary>Minimizes the specified window and activates the next top-level window in the Z order.</summary>
                //Minimize = 6,
                ///// <summary>Displays the window as a minimized window (but does not activate it).</summary>
                //ShowMinNoActive = 7,
                ///// <summary>Displays the window in its current size and position (but does not activate it).</summary>
                //ShowNA = 8, 
                /// <summary>Activates and displays the window.</summary>
                Restore = 9,
                /// <summary>Maximizes the specified window.</summary>
                Maximize = 10,
                //ShowDefault = 10,
                ///// <summary>Minimizes a window, even if the thread that owns the window is not responding.</summary>
                //ShowForceMinimize = 11
            }

            /// <summary>
            ///     Describes the type of input event.
            /// </summary>
            internal enum InputType : uint
            {
                /// <summary>The event is a mouse event.</summary>
                MOUSE = 0,
                /// <summary>The event is a keyboard event.</summary>
                KEYBOARD = 1,
                /// <summary>The event is a keyboard event.</summary>
                HARDWARE = 2
            }

            /// <summary>
            ///     Flags characterizing mouse events.
            /// </summary>
            internal enum MouseEventFlags : uint
            {
                /// <summary>The dx and dy members contain normalized absolute coordinates</summary>
                ABSOLUTE = 0x8000,
                /// <summary>The wheel was moved horizontally, if the mouse has a wheel</summary>
                HWHEEL = 0x01000,
                /// <summary>Movement occurred</summary>
                MOVE = 0x0001,
                /// <summary>The mouse move messages will not be coalesced</summary>
                MOVE_NOCOALESCE = 0x2000,
                /// <summary>The left button was pressed</summary>
                LEFTDOWN = 0x0002,
                /// <summary>The left button was released</summary>
                LEFTUP = 0x0004,
                /// <summary>The right button was pressed</summary>
                RIGHTDOWN = 0x0008,
                /// <summary>The right button was released</summary>
                RIGHTUP = 0x0010,
                /// <summary>The middle button was pressed</summary>
                MIDDLEDOWN = 0x0020,
                /// <summary>The middle button was released</summary>
                MIDDLEUP = 0x0040,
                /// <summary>Maps coordinates to the entire desktop</summary>
                VIRTUALDESK = 0x4000,
                /// <summary>The wheel was moved, if the mouse has a wheel</summary>
                WHEEL = 0x0800,
                /// <summary>An X button was pressed</summary>
                XDOWN = 0x0080,
                /// <summary>An X button was released</summary>
                XUP = 0x0100
            }

            /// <summary>
            ///     Flags characterizing keyboard events.
            /// </summary>
            internal enum KeyboardEventFlags : uint
            {
                /// <summary>If specified, the scan code was preceded by a prefix byte that has the value 0xE0</summary>
                EXTENDEDKEY = 0x0001,
                /// <summary>If specified, the key is being released, otherwise, the key is being pressed</summary>
                KEYUP = 0x0002,
                /// <summary>If specified, the scan code identifies the key and the virtual code is ignored</summary>
                SCANCODE = 0x0008,
                /// <summary>If specified, the system synthesizes a packet keystroke</summary>
                UNICODE = 0x0004
            }

            /// <summary>
            ///     Delegate defining type for <see cref="EnumThreadWindows"/> callbacks
            /// </summary>
            /// <remarks>
            ///     See EnumThreadWndProc callback function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="winHandle">A handle to a window associated with the thread specified in the <see cref="EnumThreadWindows"/> function</param>
            /// <param name="callbackParam">The application-defined value given in the <see cref="EnumThreadWindows"/> function</param>
            /// <returns><c>true</c> to stop enumeration, <c>false</c> to continue enumeration</returns>
            internal delegate bool EnumWindowsCallback(IntPtr winHandle, IntPtr callbackParam);

            /// <summary>
            ///     Retrieves the calling thread's last-error code value.
            /// </summary>
            /// <remarks>
            ///     See GetLastError help on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <returns>The calling thread's last-error code</returns>
            internal static int GetLastError()
            {
                return Marshal.GetLastWin32Error();
            }

            /// <summary>
            ///     Enumerates all nonchild windows associated with a thread 
            ///     by passing the handle to each window, in turn, to an application-defined callback function
            /// </summary>
            /// <remarks>
            ///     See EnumThreadWindows function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="tId">The identifier of the thread whose windows are to be enumerated</param>
            /// <param name="callback">A pointer to an application-defined callback function</param>
            /// <param name="callbackParam">An application-defined value to be passed to the callback function</param>
            /// <returns><c>true</c>, if the callback always returns <c>true</c>, <c>false</c> otherwise</returns>
            /// <seealso cref="EnumWindowsCallback"/>
            [DllImport("user32.dll")]
            internal static extern bool EnumThreadWindows(int tId, EnumWindowsCallback callback, IntPtr callbackParam);

            /// <summary>
            ///     Retrieves the identifier of the thread that created the specified window and,
            ///     optionally, the identifier of the process that created the window
            /// </summary>
            /// <remarks>
            ///     See GetWindowThreadProcessId function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="hWnd">A handle to the window</param>
            /// <param name="ProcessId">A pointer to a variable that receives the process identifier</param>
            /// <returns>The identifier of the thread that created the window</returns>
            [DllImport("user32.dll", SetLastError=true)]
            internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

            /// <summary>
            ///     Attaches or detaches the input processing mechanism of one thread to that of another thread.
            /// </summary>
            /// <remarks>
            ///     See AttachThreadInput function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="idAttach">The identifier of the thread to be attached to another thread</param>
            /// <param name="idAttachTo">The identifier of the thread to which idAttach will be attached</param>
            /// <param name="attach"><c>true</c> to attach, <c>false</c> to detach</param>
            /// <returns><c>false</c> on error, <c>true</c> on success</returns>
            [DllImport("user32.dll", SetLastError=true)]
            private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool attach);

            /// <summary>
            ///     Synthesizes keystrokes, mouse motions, and button clicks.
            /// </summary>
            /// <remarks>
            ///     See SendInput function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="nInputs">The number of structures in the array</param>
            /// <param name="pInputs">An array of <see cref="Input"/> structures</param>
            /// <param name="cbSize">The size, in bytes, of an <see cref="Input"/> structure</param>
            /// <returns>The number of events that it successfully inserted into the keyboard or mouse input stream</returns>
            [DllImport("user32.dll")]
            internal static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

            /// <summary>
            ///     Sets the specified window's show state. 
            /// </summary>
            /// <remarks>
            ///     See ShowWindow function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="winHandle">A handle to the window</param>
            /// <param name="cmd">Controls how the window is to be shown</param>
            /// <returns><c>0</c> if the window was previously visible, non-zero otherwise</returns>
            [DllImport("user32.dll")]
            internal static extern bool ShowWindow(IntPtr winHandle, ShowState cmd);

            /// <summary>
            ///     Retrieves a handle to the foreground window (the window with which the user is currently working)
            /// </summary>
            /// <remarks>
            ///     See GetForegroundWindow function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <returns>A handle to the foreground window</returns>
            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            /// <summary>
            ///     Brings the thread that created the specified window into the foreground and activates the window.
            /// </summary>
            /// <remarks>
            ///     See SetForegroundWindow function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="winHandle">A handle to the window that should be activated and brought to the foreground</param>
            /// <returns><c>false</c> if the window was not brought to foregound, <c>true</c> if it was</returns>
            /// <seealso cref="BringWindowToTop"/>
            /// <seealso cref="SetForeground"/>
            [DllImport("user32.dll")]
            private static extern bool SetForegroundWindow(IntPtr winHandle);

            /// <summary>
            ///     Brings the specified window to the top of the Z order.
            /// </summary>
            /// <remarks>
            ///     See BringWindowToTop function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="winHandle">A handle to the window to bring to the top of the Z order</param>
            /// <returns><c>false</c> on error, <c>true</c> on success</returns>
            /// <seealso cref="SetForegroundWindow"/>
            /// <seealso cref="SetForeground"/>
            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool BringWindowToTop(IntPtr winHandle);

            /// <summary>
            ///     Brings the thread that created the specified window into the foreground and activates the window.
            /// </summary>
            /// <remarks>
            ///     The SetForegroundWindow() function seems not to work properly on windows 7.
            ///     So this code does not work properly. Sometimes the windows gets only activated.
            ///     This is trick from <c>pinvoke.net</c>.
            /// </remarks>
            /// <param name="winHandle">A handle to the window that should be activated and brought to the foreground</param>
            /// <seealso cref="BringWindowToTop"/>
            /// <seealso cref="SetForegroundWindow"/>
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
                        throw new ApplicationException("SetForegroundWindow() failed.");
                }

                if (attached && (foregroundWindowThread != thisThread) && !AttachThreadInput(foregroundWindowThread, thisThread, false))
                    throw new ApplicationException("Could not detach from the foreground window thread. Error code: " + GetLastError());
            }

            /// <summary>
            ///     Determines the visibility state of the specified window. 
            /// </summary>
            /// <remarks>
            ///     See IsWindowVisible function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="winHandle">A handle to the window to be tested</param>
            /// <returns><c>true</c> If the specified window or one of its ancestors is visible, <c>false</c> otherwise</returns>
            [DllImport("user32.dll")]
            internal static extern bool IsWindowVisible(IntPtr winHandle);

            /// <summary>
            ///     Copies the text of the specified window's title bar (if it has one) into a buffer.
            /// </summary>
            /// <remarks>
            ///     See GetWindowText function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="winHandle">A handle to the window or control containing the text</param>
            /// <param name="title">The buffer that will receive the text</param>
            /// <param name="len">The maximum number of characters to copy to the buffer, including the null character</param>
            /// <returns>The length of the string in the buffer, if the function succeeds, <c>0</c> on error or if the window has no title</returns>
            /// <seealso cref="GetWindowTextLength"/>
            [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
            internal static extern int GetWindowText(IntPtr winHandle, StringBuilder title, int len);

            /// <summary>
            ///     Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar).
            /// </summary>
            /// <remarks>
            ///     See GetWindowTextLength function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="winHandle">A handle to the window or control</param>
            /// <returns>The length of the string in the buffer, if the function succeeds, <c>0</c> on error of if the window has no title</returns>
            /// <seealso cref="GetWindowText"/>
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern int GetWindowTextLength(IntPtr winHandle);

            /// <summary>
            ///     Retrieves the dimensions of the bounding rectangle of the specified window.
            /// </summary>
            /// <remarks>
            ///     See GetWindowRect function on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="winHandle">A handle to the window</param>
            /// <param name="rect">A pointer to a <see cref="Rect"/> structure that receives the screen coordinates of the upper-left and lower-right corners of the window</param>
            /// <returns><c>true</c> on error, <c>false</c> otherwise</returns>
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool GetWindowRect(IntPtr winHandle, out Rect rect);
        }
    }
}
