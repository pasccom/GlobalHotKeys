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
