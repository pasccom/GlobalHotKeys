/* Copyright 2015-2016 Pascal COMBES <pascom@orange.fr>
 * 
 * This file is part of GlobalHotKeys.
 * 
 * GlobalHotKeys is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * GlobalHotKeys is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with GlobalHotKeys. If not, see <http://www.gnu.org/licenses/>
 */

﻿using System;
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