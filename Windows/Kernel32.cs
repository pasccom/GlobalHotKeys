using System;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Windows
    {
        class Kernel32
        {
            [DllImport("kernel32.dll")]
            internal static extern uint GetCurrentThreadId();
        }
    }
}
