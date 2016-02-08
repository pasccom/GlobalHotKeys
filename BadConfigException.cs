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
    /// <summary>
    ///     Exception thrown when configuration is invalid
    /// </summary>
    /// <para>
    ///     This exception characterizes an error in the configuration file.
    ///     It stores the <see cref="FileName"/>, the <see cref="Line"/> and the <see cref="Column"/> of the error.
    /// </para>
    class BadConfigException : ApplicationException
    {
        /// <summary>
        ///     Name of the file where the error occured.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        ///     Line in the file where the error occured.
        /// </summary>
        public uint? Line { get; set; }
        /// <summary>
        ///     Column where the error occured.
        /// </summary>
        public uint? Column { get; set; }

        /// <summary>
        ///     Constructs an exception with no message and properties.
        /// </summary>
        public BadConfigException() :
            base() { }
        /// <summary>
        ///     Constructs an exception with the given message (but no properties).
        /// </summary>
        /// <param name="msg">Message associated with the exception</param>
        public BadConfigException(string msg) :
            base(msg) { }
        /// <summary>
        ///     Constructs an exception with the given message and the given parent exception (but no properties).
        /// </summary>
        /// <param name="msg">Message associated with the exception</param>
        /// <param name="e">Parent exception</param>
        public BadConfigException(string msg, Exception e) :
            base(msg, e) { }
        /// <summary>
        ///     Constructs an exception with properties (but no message)
        /// </summary>
        /// <param name="filename"><see cref="FileName"/> where the error occured</param>
        /// <param name="line"><see cref="Line"/> in the file where the error occured</param>
        /// <param name="column"><see cref="Column"/> where the error occured</param>
        public BadConfigException(string filename, uint line, uint column) :
            base()
        {
            FileName = filename;
            Line = line;
            Column = column;
        }
        /// <summary>
        ///     Constructs an exception with the given message and properties.
        /// </summary>
        /// <param name="msg">Message associated with the exception</param>
        /// <param name="filename"><see cref="FileName"/> where the error occured</param>
        /// <param name="line"><see cref="Line"/> in the file where the error occured</param>
        /// <param name="column"><see cref="Column"/> where the error occured</param>
        public BadConfigException(string msg, string filename, uint line, uint column) :
            base(msg)
        {
            FileName = filename;
            Line = line;
            Column = column;
        }
        /// <summary>
        ///     Constructs an exception with the given message, properties and parent exception
        /// </summary>
        /// <param name="msg">Message associated with the exception</param>
        /// <param name="filename"><see cref="FileName"/> where the error occured</param>
        /// <param name="line"><see cref="Line"/> in the file where the error occured</param>
        /// <param name="column"><see cref="Column"/> where the error occured</param>
        /// <param name="e">Parent exception</param>
        public BadConfigException(string msg, string filename, uint line, uint column, Exception e) :
            base(msg, e)
        {
            FileName = filename;
            Line = line;
            Column = column;
        }
    }
}
