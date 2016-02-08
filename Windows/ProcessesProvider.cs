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
using log4net;

namespace GlobalHotKeys
{
    namespace Windows
    {
        /// <summary>
        ///     Abstract base for for classes providing a configuration for GlobalHotKeys processes.
        /// </summary>
        /// <para>
        ///     It handles the storage of the known processes, which can be retrieved by name
        ///     thanks to <see cref="GetProcess"/> or the <c>[]</c> operator.
        /// </para>
        /// <para>
        ///     The processes are added by implementation thanks to the protected method <see cref="newProcess"/>.
        /// </para>
        abstract class ProcessesProvider
        {
            /// <summary>
            ///     Logger for GlobalHokKeys.
            /// </summary>
            /// <remarks>See Apache Log4net documentation for the logging interface.</remarks>
            static private readonly ILog log = LogManager.GetLogger(typeof(ProcessesProvider));

            /// <summary>
            ///     The dictionnary of known processes.
            /// </summary>
            private Dictionary<string, ProcessData> mProcesses;

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <para>
            ///     Create the dictionnary of known processes.
            /// </para>
            public ProcessesProvider()
            {
                mProcesses = new Dictionary<string, ProcessData>();
            }

            /// <summary>
            ///     Add a new known process.
            /// </summary>
            /// <remarks>If the process already exists it is overwritten.</remarks>
            /// <param name="name">The name of the new process</param>
            /// <param name="process">Information about the process</param>
            protected void newProcess(string name, ProcessData process)
            {
                log.Info("Added new process: " + name);
                log.Debug("\t-Executable path: " + process.ExePath);
                log.Debug("\t-Start folder: " + process.StartFolder);
                log.Debug("\t-New shell: " + (process.Shell ? "Yes" : "No"));
                log.Debug("\t-Real executable path: " + process.StartPath);
                log.Debug("\t-Arguments: " + String.Join<string>(", ", process.StartArguments));

                try {
                    mProcesses.Add(name, process);
                } catch (ArgumentException) {
                    log.WarnFormat("A key with the same name (\"{0}\") already exists. It will be overwritten.", name);
                    mProcesses.Remove(name);
                    mProcesses.Add(name, process);
                }
            }

            /// <summary>
            ///     Get a known process by name.
            /// </summary>
            /// <param name="name">The name of the serached process</param>
            /// <returns>The data associated with the process</returns>
            /// <see cref="getProcess"/>
            public ProcessData this[string name] 
            { 
                get 
                { 
                    return getProcess(name); 
                } 
            }

            /// <summary>
            ///     Get a known process by name.
            /// </summary>
            /// <param name="name">The name of the serached process</param>
            /// <returns>The data associated with the process</returns>
            /// <see cref="this[string name]"/>
            public ProcessData getProcess(string name)
            {
                try {
                    return mProcesses[name];
                } catch (KeyNotFoundException e) {
                    throw new ProcessNotFoundException(name, "The process \"" + name + "\" was not found in known process list.", e);
                }
            }

            /// <summary>
            ///     Config file parsing.
            /// </summary>
            /// <para>This function is in charge of the parsing of the config file.</para>
            public abstract void parseConfig();
        }
    }
}
