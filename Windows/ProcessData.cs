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

namespace GlobalHotKeys.Windows
{
    /// <summary>
    ///     Describe a process.
    /// </summary>
    /// <para>
    ///     This class defines a process known to GlobalHotkeys.
    ///     It thus contains, the real (see <see cref="StartPath"/>) 
    ///     and apparent path (see <see cref="ExePath"/>) to the executable (which may differ).
    ///     It also stores, the <see cref="StartFolder"/>, the <see cref="StartArguments"/>
    ///     and a <see cref="Shell"/> flag telling whether the process should be started in a shell.
    /// </para>
    class ProcessData
    {
        /// <summary>
        ///     Internal storage of the path to the real executable of the process.
        /// </summary>
        /// <seealso cref="StartPath"/>
        /// <seealso cref="ExePath"/>
        private string mStartPath;
        /// <summary>
        ///     Path to the apparent executable of the process.
        /// </summary>
        /// <seealso cref="StartPath"/>
        public string ExePath { get; set; }
        /// <summary>
        ///     Start in a shell.
        /// </summary>
        /// <para>When <c>true</c> the process should be started in a shell.</para>
        public bool Shell { get; private set; }
        /// <summary>
        ///     Path to the folder where the process should be started.
        /// </summary>
        public string StartFolder { get; set; }
        /// <summary>
        ///     Path to the real executable of the process.
        /// </summary>
        /// <seealso cref="ExePath"/>
        public string StartPath
        {
            get
            {
                if (mStartPath == null)
                    return ExePath;
                else
                    return mStartPath;
            }

            set
            {
                mStartPath = value;
            }
        }
        /// <summary>
        ///     Arguments used to start the process.
        /// </summary>
        public List<string> StartArguments { get; private set; }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <para>
        ///     Set all fields to their empty value.
        /// </para>
        public ProcessData()
        {
            ExePath = null;
            StartPath = null;
            Shell = false;
            StartFolder = null;
            StartArguments = new List<string>();
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="exePath">Path to the apparent executable of the process</param>
        /// <param name="startFolder">Path to the folder where the process is started</param>
        public ProcessData(string exePath, string startFolder)
        {
            ExePath = exePath;
            StartPath = null;
            Shell = false;
            StartFolder = startFolder;
            StartArguments = new List<string>();
        }

        /// <summary>
        ///     Set the <see cref="Shell"/> property according to the string token.
        /// </summary>
        /// <param name="arg">A string token (should be <c>X|O</c> for the method to succeed)</param>
        /// <returns><c>false</c> on error, <c>true</c> on success</returns>
        public bool parseShell(string arg)
        {
            switch (arg) {
            case "":
                return true;
            case "X":
                Shell = true;
                return true;
            case "O":
                Shell = false;
                return true;
            default:
                return false;
            }
        }

        /// <summary>
        ///     Add a argument to the process.
        /// </summary>
        /// <param name="arg">The argument to add to <see cref="StartArguments"/></param>
        /// <seealso cref="resetArguments"/>
        public void addArgument(string arg) 
        {
            StartArguments.Add(arg);
        }

        /// <summary>
        ///     Reset arguments to the process.
        /// </summary>
        /// <seealso cref="addArguments"/>
        public void resetArguments()
        {
            StartArguments.Clear();
        }
    }
}
