using System;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Windows
    {
        /// <summary>
        ///     Imports from <c>kernel32.dll</c>
        /// </summary>
        class Kernel32
        {
            /// <summary>
            ///     Retrieves the thread identifier of the calling thread.
            /// </summary>
            /// <remarks>
            ///     See GetCurrentThreadId on MSDN. The documentation was copied from there.
            /// </remarks>
            /// <returns>The thread identifier of the calling thread</returns>
            [DllImport("kernel32.dll")]
            internal static extern uint GetCurrentThreadId();
        }
    }
}
