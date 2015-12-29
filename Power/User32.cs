using System;
using System.Runtime.InteropServices;

namespace GlobalHotKeys
{
    namespace Power
    {
        /// <summary>
        ///     Imports from <c>user32.dll</c>
        /// </summary>
        class User32
        {
            /// <summary>
            ///     Locks the workstation's display.
            /// </summary>
            /// <returns><c>0</c> on error, non-zero otherwise</returns>
            [DllImport("user32", SetLastError=true)]
            internal static extern bool LockWorkStation();

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
        }
    }
}