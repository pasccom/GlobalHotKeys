using System;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Power
    {
        class User32
        {
            [DllImport("user32")]
            internal static extern void LockWorkStation();
        }
    }
}