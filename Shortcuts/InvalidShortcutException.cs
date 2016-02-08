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

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        /// <summary>
        ///     Exception thrown when an invalid <see cref="ShortcutData"/> is used.
        /// </summary>
        /// <para>
        ///     It stores the <see cref="InvalidShortcut"/> in a property.
        /// </para>
        class InvalidShortcutException : ApplicationException
        {
            /// <summary>
            ///     The invalid shortcut.
            /// </summary>
            public ShortcutData InvalidShortcut {get; set;}

            /// <summary>
            ///     Default constructor
            /// </summary>
            /// <remarks>The <see cref="InvalidShortcut"/> is not set.</remarks>
            public InvalidShortcutException() :
                base() { }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <remarks>The <see cref="InvalidShortcut"/> is not set.</remarks>
            public InvalidShortcutException(string msg) :
                base(msg) { }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="e">Parent exception</param>
            /// <remarks>The <see cref="InvalidShortcut"/> is not set.</remarks>
            public InvalidShortcutException(string msg, Exception e) :
                base(msg, e) { }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="shortcut">The <see cref="InvalidShortcut"/></param>
            public InvalidShortcutException(ShortcutData shortcut) :
                base()
            {
                InvalidShortcut = shortcut;
            }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="shortcut">The <see cref="InvalidShortcut"/></param>
            public InvalidShortcutException(string msg, ShortcutData shortcut) :
                base(msg)
            {
                InvalidShortcut = shortcut;
            }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="shortcut">The <see cref="InvalidShortcut"/></param>
            /// <param name="e">Parent exception</param>
            public InvalidShortcutException(string msg, ShortcutData shortcut, Exception e) :
                base(msg, e)
            {
                InvalidShortcut = shortcut;
            }
        }
    }
}
