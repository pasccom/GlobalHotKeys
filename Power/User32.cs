using System;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Power
    {
        class User32
        {
            [DllImport("user32", SetLastError=true)]
            internal static extern bool LockWorkStation();

            internal static int GetLastError()
            {
                return Marshal.GetLastWin32Error();
            }
        }
    }
}