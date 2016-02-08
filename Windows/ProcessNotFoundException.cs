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
    namespace Windows
    {
        /// <summary>
        ///     Exception thrown when a process does not exist.
        /// </summary>
        /// <para>
        ///     It stores the name of the unknown process in <see cref="ProcessName"/>
        /// </para>
        class ProcessNotFoundException : ApplicationException
        {
            /// <summary>
            ///     The name of the unknown process.
            /// </summary>
            public string ProcessName { get; private set; }

            /// <summary>
            ///     Default constructor.
            /// </summary>
            /// <remarks>The name of the unknown process is not set</remarks>
            public ProcessNotFoundException() :
                base() { }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <remarks>The name of the unknown process is not set</remarks>
            /// <param name="msg">Exception message</param>
            /// <param name="e">Parent exception</param>
            public ProcessNotFoundException(string msg, Exception e) :
                base(msg, e) { }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="filename">Name of the unknown process</param>
            public ProcessNotFoundException(string filename) :
                base()
            {
                ProcessName = filename;
            }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="filename">Name of the unknown process</param>
            /// <param name="msg">Exception message</param>
            public ProcessNotFoundException(string filename, string msg) :
                base(msg)
            {
                ProcessName = filename;
            }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="filename">Name of the unknown process</param>
            /// <param name="msg">Exception message</param>
            /// <param name="e">Parent exception</param>
            public ProcessNotFoundException(string filename, string msg, Exception e) :
                base(msg, e)
            {
                ProcessName = filename;
            }
        }
    }
}
