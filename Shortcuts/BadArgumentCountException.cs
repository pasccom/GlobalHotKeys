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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        /// <summary>
        ///     Exception thrown when a method receives too many or too few arguments.
        /// </summary>
        /// <para>
        ///     It has two properties which store the <see cref="Min"/>imum
        ///     and <see cref="Max"/>imum number of arguments the invoked method expects.
        /// </para>
        class BadArgumentCountException : ApplicationException
        {
            /// <summary>
            ///     Maximum number of arguments the invoked method expects.
            /// </summary>
            public uint Max { get; set; }
            /// <summary>
            ///     Minimum number of arguments the invoked method expects.
            /// </summary>
            public uint Min { get; set; }

            /// <summary>
            ///     Default constrcutor
            /// </summary>
            /// <remarks><see cref="Min"/>imum and <see cref="Max"/>imum number of arguments are not set.</remarks>
            public BadArgumentCountException() :
                base() { }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <remarks><see cref="Min"/>imum and <see cref="Max"/>imum number of arguments are not set.</remarks>
            public BadArgumentCountException(string msg) :
                base(msg) { }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="e">Exception message</param>
            /// <remarks><see cref="Min"/>imum and <see cref="Max"/>imum number of arguments are not set.</remarks>
            public BadArgumentCountException(string msg, Exception e) :
                base(msg, e) { }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="n">Number of arguments the method expects</param>
            /// <remarks><see cref="Min"/>imum and <see cref="Max"/>imum number of arguments are set to the same value.</remarks>
            public BadArgumentCountException(uint n) :
                base() 
            { 
                Min = n; 
                Max = n; 
            }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="n">Number of arguments the method expects</param>
            /// <remarks><see cref="Min"/>imum and <see cref="Max"/>imum number of arguments are set to the same value.</remarks>
            public BadArgumentCountException(string msg, uint n) :
                base(msg)
            {
                Min = n; 
                Max = n; 
            }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="n">Number of arguments the method expects</param>
            /// <param name="e">Exception message</param>
            /// <remarks><see cref="Min"/>imum and <see cref="Max"/>imum number of arguments are set to the same value.</remarks>
            public BadArgumentCountException(string msg, uint n, Exception e) :
                base(msg, e)
            {
                Min = n; 
                Max = n; 
            }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="min"><see cref="Min"/>imum number of arguments the method expects</param>
            /// <param name="max"><see cref="Max"/>imum number of arguments the method expects</param>
            public BadArgumentCountException(uint min, uint max) :
                base() 
            { 
                Min = min; 
                Max = max; 
            }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="min"><see cref="Min"/>imum number of arguments the method expects</param>
            /// <param name="max"><see cref="Max"/>imum number of arguments the method expects</param>
            public BadArgumentCountException(string msg, uint min, uint max) :
                base(msg)
            { 
                Min = min; 
                Max = max; 
            }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="min"><see cref="Min"/>imum number of arguments the method expects</param>
            /// <param name="max"><see cref="Max"/>imum number of arguments the method expects</param>
            /// <param name="e">Exception message</param>
            public BadArgumentCountException(string msg, uint min, uint max, Exception e) :
                base(msg, e) 
            {
                Min = min;
                Max = max;
            }
        }
    }
}
