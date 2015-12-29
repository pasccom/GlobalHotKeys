using System;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        /// <summary>
        ///     Imports from <c>user32.dll</c>
        /// </summary>
        class User32
        {
            /// <summary>
            ///     Hotkey message identifier.
            /// </summary>
            /// <seealso cref="MSG.message"/>
            internal static readonly uint WM_HOTKEY = 0x0312;
            /// <summary>
            ///     Success value.
            /// </summary>
            internal static readonly IntPtr SUCCESS = new IntPtr(1);

            /// <summary>
            ///     Internal data of low-level keyboard events.
            /// </summary>
            /// <para>
            ///     This structure stores the information related to low-level keyboard events.
            ///     It can be retriewed in the <see cref="HookProc"/> callback.
            /// </para>
            /// <remarks>
            ///     See KBDLLHOOKSTRUCT on MSDN
            /// </remarks>
            internal struct KeyboardLowLevelHookStruct
            {
                /// <summary>The virtual key code</summary>
                public uint vkCode;
                /// <summary>The scan code</summary>
                public uint scanCode;
                /// <summary>Associated flags</summary>
                public KeyboardLowLevelHookFlags flags;
                /// <summary>Time stamp</summary>
                public uint time;
                /// <summary>Additional information (unused)</summary>       
                public UIntPtr dwExtraInfo;             
            }

            /// <summary>
            ///     Stores a point (in cartesian coordinates)
            /// </summary>
            /// <remarks>
            ///     See POINT structure on MSDN.
            /// </remarks>
            internal struct Point
            {
                /// <summary>X coordinate</summary>
                public long x;
                /// <summary>Y coordinate</summary>
                public long y;
            }

            /// <summary>
            ///     Stores a message.
            /// </summary>
            /// <para>
            ///     This structure is used to convey internal message in Windows.
            /// </para>
            /// <remarks>
            ///     See MSG structure on MSDN
            /// </remarks>
            /// <seealso cref="GetMessage"/>
            internal struct MSG
            {
                /// <summary>Windows handle</summary>
                public IntPtr hwnd;
                /// <summary>Message type</summary>
                public uint message;
                /// <summary>Information on the message.</summary>
                /// <remarks>See WM_HOTKEY message on MSDN. </remarks>
                public UIntPtr wParam;
                /// <summary>Information on the message.</summary>
                /// <remarks>See WM_HOTKEY message on MSDN. </remarks>
                public IntPtr lParam;
                /// <summary>Time stamp</summary>
                public uint time;
                /// <summary>Point</summary>
                public Point pt;
            }

            /// <summary>
            ///     Flags for low-level keyboard events.
            /// </summary>
            /// <remarks>
            ///     See KBDLLHOOKSTRUCT structure on MSDN.
            /// </remarks>
            internal enum KeyboardLowLevelHookFlags
            {
                /// <summary>Test the extended-key flag</summary>
                LLKHF_EXTENDED = 0x01,
                /// <summary>Test the event-injected (from any process) flag</summary>
                LLKHF_INJECTED = 0x10,
                /// <summary>Test the event-injected (from a process running at lower integrity level) flag</summary>
                LLKHF_LOWER_IL_INJECTED = 0x02,
                /// <summary>Test the context code</summary>
                LLKHF_ALTDOWN = 0x20,
                /// <summary>Test the transition-state flag</summary>
                LLKHF_UP = 0x80,
            }

            /// <summary>
            ///     Flags for hook types.
            /// </summary>
            /// <remarks>
            ///     See SetWindowsHookEx function on MSDN.
            /// </remarks>
            internal enum HookType
            {
                /// <summary>Monitors input events</summary>
                WH_MSGFILTER = -1,
                /// <summary>Records messages (to replay them with <see cref="WH_JOURNALPLAYBACK"/></summary>
                WH_JOURNALRECORD = 0,
                /// <summary>Replays messages (recorded with <see cref="WH_JOURNALRECORD"/></summary>
                WH_JOURNALPLAYBACK = 1,
                /// <summary>Monitor keystroke messages</summary>
                WH_KEYBOARD = 2,
                /// <summary>Monitors messages posted to the message queue</summary>
                WH_GETMESSAGE = 3,
                /// <summary>Monitors message before the destination receives them</summary>
                WH_CALLWNDPROC = 4,
                /// <summary>Notifications for a CBT application</summary>
                WH_CBT = 5,
                /// <summary>Monitors input events for all applications</summary>
                WH_SYSMSGFILTER = 6,
                /// <summary>Monitor mouse messages</summary>
                WH_MOUSE = 7,
                /// <summary>Not yet implemented (nor documented)</summary>
                WH_HARDWARE = 8,
                /// <summary>Debug other hook procedures</summary>
                WH_DEBUG = 9,
                /// <summary>Monitors shell events</summary>
                WH_SHELL = 10,
                /// <summary>Monitors the foreground thread of an application before it becomes idle</summary>
                WH_FOREGROUNDIDLE = 11,
                /// <summary>Monitors message after the destination returns</summary>
                WH_CALLWNDPROCRET = 12,
                /// <summary>Monitor low-level keyboard messages</summary>
                WH_KEYBOARD_LL = 13,
                /// <summary>Monitor low-level mouse messages</summary>
                WH_MOUSE_LL = 14
            }

            /// <summary>
            ///     Types of message for low-level keyboard messages.
            /// </summary>
            /// <remarks>
            ///     See LowLevelKeyboardProc callback function on MSDN.
            /// </remarks>
            internal enum WindowsMessage
            {
                /// <summary>A non-system key (without ALT) is pressed.</summary>
                KEYDOWN = 0x0100,
                /// <summary>A non-system key (without ALT) is released.</summary>
                KEYUP = 0x0101,
                /// <summary>A system key (with ALT) is pressed.</summary>
                SYSKEYDOWN = 0x0104,
                /// <summary>A system key (with ALT) is released.</summary>
                SYSKEYUP = 0x0105,
            }

            /// <summary>
            ///     Retrieves a message from the calling thread's message queue. 
            ///     The function dispatches incoming sent messages until a posted message is available for retrieval. 
            /// </summary>
            /// <remarks>
            ///     See GetMessage help on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="lpMsg">A pointer to an MSG structure that receives message information from the thread's message queue</param>
            /// <param name="hWnd">A handle to the window whose messages are to be retrieved</param>
            /// <param name="wMsgFilterMin">The integer value of the lowest message value to be retrieved</param>
            /// <param name="wMsgFilterMax">The integer value of the highest message value to be retrieved</param>
            /// <returns><c>-1</c> on error, <c>0</c> to quit and a non-negative integer otherwise</returns>
            [DllImport("user32.dll", SetLastError=true)]
            internal static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

            /// <summary>
            ///     Places (posts) a message in the message queue associated with the thread that created the specified window 
            ///     and returns without waiting for the thread to process the message.
            /// </summary>
            /// <remarks>
            ///     See PostMessage help on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="hWnd">A handle to the window whose messages are to be retrieved</param>
            /// <param name="Msg">The message to be posted</param>
            /// <param name="wParam">Additional message-specific information (see MSDN for a specific message)</param>
            /// <param name="lParam">Additional message-specific information (see MSDN for a specific message)</param>
            /// <returns><c>0</c> on error, non-zero otherwise</returns>
            [DllImport("user32.dll", SetLastError=true)]
            internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            /// <summary>
            ///     Delegate defining type for hook callbacks
            /// </summary>
            /// <para>
            ///     An application-defined or library-defined callback function used with the <see cref="SetWindowsHookEx"/> function. 
            ///     The system calls this function whenever an application calls 
            ///     the <see cref="GetMessage"/> or <see cref="PeekMessage"/> function with the corresponding message type.
            /// </para>
            /// <remarks>
            ///     See HOOKPROC on MSDN.
            /// </remarks>
            /// <param name="code">A code the hook procedure uses to determine how to process the message</param>
            /// <param name="wParam">Additional message-specific information (see MSDN for a specific message)</param>
            /// <param name="lParam">Additional message-specific information (see MSDN for a specific message)</param>
            /// <returns>Non-zero to prevent further processing</returns>
            internal delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

            /// <summary>
            ///     Converts a raw pointer into a <see cref="KeyboardLowLevelHookStruct"/> structure.
            /// </summary>
            /// <param name="lParam">The raw pointer from the hook callback <see cref="WH_KEYBOARD_LL"/></param>
            /// <returns>The pointed structure.</returns>
            internal static KeyboardLowLevelHookStruct getKeyboardLowLevelHookStruct(IntPtr lParam)
            {
                return (KeyboardLowLevelHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardLowLevelHookStruct));
            }

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
            ///     Installs an application-defined hook procedure into a hook chain.
            /// </summary>
            /// <remarks>
            ///     See SetWindowsHookEx help on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="hookType">The type of hook procedure to be installed</param>
            /// <param name="lpfn">A pointer to the hook procedure</param>
            /// <param name="hMod">A handle to the DLL containing the hook procedure</param>
            /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated</param>
            /// <returns>A handle to the hook procedure on success and <c>NULL</c> on error</returns>
            /// <seealso cref="UnhookWindowsHookEx"/>
            [DllImport("user32.dll", SetLastError=true)]
            internal static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

            /// <summary>
            ///     Passes the hook information to the next hook procedure in the current hook chain
            /// </summary>
            /// <remarks>
            ///     See CallNextHookEx help on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <param name="hhk">This parameter is ignored</param>
            /// <param name="nCode">The hook code passed to the current hook procedure</param>
            /// <param name="wParam">The <c>wParam</c> value passed to the current hook procedure</param>
            /// <param name="lParam">The <c>lParam</c> value passed to the current hook procedure</param>
            /// <returns>This value is returned by the next hook procedure in the chain</returns>
            [DllImport("user32.dll")]
            internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            /// <summary>
            ///     Removes a hook procedure installed in a hook chain by the <see cref="SetWindowsHookEx"/> function.
            /// </summary>
            /// <param name="hhk">A handle to the hook to be removed</param>
            /// <returns><c>0</c> on error, non-zero on success</returns>
            /// <seealso cref="SetWindowsHookEx"/>
            [DllImport("user32.dll", SetLastError=true)]
            internal static extern bool UnhookWindowsHookEx(IntPtr hhk);
        }
    }
}
