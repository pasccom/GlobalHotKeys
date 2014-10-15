using System;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class User32
        {
            internal static readonly uint WM_HOTKEY = 0x0312; /*!< Hotkey message identifier */
            internal static readonly IntPtr SUCCESS = new IntPtr(1);

            internal struct KeyboardLowLevelHookStruct
            {
                public uint vkCode;
                public uint scanCode;
                public KeyboardLowLevelHookFlags flags;
                public uint time;
                public UIntPtr dwExtraInfo;
            }

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

            internal enum KeyboardLowLevelHookFlags
            {
                LLKHF_EXTENDED = 0x01,
                LLKHF_INJECTED = 0x10,
                LLKHF_ALTDOWN = 0x20,
                LLKHF_UP = 0x80,
            }

            internal enum HookType
            {
                WH_JOURNALRECORD = 0,
                WH_JOURNALPLAYBACK = 1,
                WH_KEYBOARD = 2,
                WH_GETMESSAGE = 3,
                WH_CALLWNDPROC = 4,
                WH_CBT = 5,
                WH_SYSMSGFILTER = 6,
                WH_MOUSE = 7,
                WH_HARDWARE = 8,
                WH_DEBUG = 9,
                WH_SHELL = 10,
                WH_FOREGROUNDIDLE = 11,
                WH_CALLWNDPROCRET = 12,
                WH_KEYBOARD_LL = 13,
                WH_MOUSE_LL = 14
            }

            internal enum WindowsMessage
            {
                KEYDOWN = 0x0100,
                KEYUP = 0x0101,
                SYSKEYDOWN = 0x0104,
                SYSKEYUP = 0x0105,
            }

            [DllImport("user32.dll", SetLastError=true)]
            internal static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

            [DllImport("user32.dll", SetLastError=true)]
            internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            internal delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

            internal static KeyboardLowLevelHookStruct getKeyboardLowLevelHookStruct(IntPtr lParam)
            {
                return (KeyboardLowLevelHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardLowLevelHookStruct));
            }

            internal static int GetLastError()
            {
                return Marshal.GetLastWin32Error();
            }

            [DllImport("user32.dll", SetLastError=true)]
            internal static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll")]
            internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError=true)]
            internal static extern bool UnhookWindowsHookEx(IntPtr hhk);
        }
    }
}
