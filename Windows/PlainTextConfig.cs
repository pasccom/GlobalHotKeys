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
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using log4net;

namespace GlobalHotKeys
{
    namespace Windows
    {
        /// <summary>
        ///     A parser for plain text configuration files
        /// </summary>
        /// <para>
        ///     The format of these file is similar to CSV.
        ///     Each line describes a shortcut and each column is a field.
        ///     The line are separated by <code>"\\r\\n"</code> and the columns are separated by one or more space.
        ///     Quote marks <code>'"'</code> can be used if spaces are needed in a field (see field params below).
        ///     Lines can be commented with <code>'#'</code>
        /// </para>
        /// <para>
        ///     For this particular configuration, the columns are the following ones:
        ///     <list type="number">
        ///         <item><term>Name: </term><description>The name of the process</description></item>
        ///         <item><term>ExePath </term><description>The path to the apprent executable of the process</description></item>
        ///         <item><term>Shell: </term><description><code>[O|X]</code> to start in shell or not</description></item>
        ///         <item><term>StartFolder</term><description>The folder where the process should be started</description></item>
        ///         <item><term>RealExePath: </term><description>The path to the real executable of the process</description></item>
        ///         <item><term>Arguments: </term><description>Argument to be passed to the process when it is started</description></item>
        ///     </list>
        ///     For an exemple of such a config file, see the file <code>globalhotkeys.processes.conf</code> in the project folder.
        /// </para>
        class PlainTextConfig : ProcessesProvider
        {
            /// <summary>
            ///     Logger for GlobalHokKeys.
            /// </summary>
            /// <remarks>See Apache Log4net documentation for the logging interface.</remarks>
            private static readonly ILog log = LogManager.GetLogger(typeof(PlainTextConfig));
            /// <summary>
            ///     Id of the system user.
            /// </summary>
            private static readonly IdentityReference idSystem = new NTAccount("System").Translate(Type.GetType("System.Security.Principal.SecurityIdentifier"));
            /// <summary>
            ///     Id of the Administrators user.
            /// </summary>
            private static readonly IdentityReference idAdmin = new NTAccount("Administrateurs").Translate(Type.GetType("System.Security.Principal.SecurityIdentifier"));
            /// <summary>
            ///     Internal storage for the config file name.
            /// </summary>
            private string mFileName;

            /// <summary>
            ///     The config file name.
            /// </summary>
            /// <remarks>
            ///     The setter check that the file has the right permissions.
            /// </remarks>
            public string FileName
            {
                get
                {
                    return mFileName;
                }

                set
                {
                    checkConfigFile(value);
                    mFileName = value;
                }
            }

            /// <summary>
            ///     Constructs a new configuration.
            /// </summary>
            /// <remarks>
            ///     The permissions on the file are checked.
            /// </remarks>
            /// <param name="filename">The file name containing the configuration</param>
            public PlainTextConfig(string filename)
                : base()
            {
                FileName = filename;
            }

            /// <summary>
            ///     Parses the configuration file.
            /// </summary>
            public override void parseConfig()
            {
                TextReader inStream = new StreamReader(new FileStream(mFileName, FileMode.Open, FileAccess.Read));
                uint l = 0;

                while (true) {
                    string line = inStream.ReadLine();
                    l++;

                    if (line == null)
                        break;

                    // Ignores lines starting with another character than space. May be used for comments:
                    if (!line.StartsWith(" ")) {
                        log.DebugFormat("Ignored line \"{0}\"", line);
                        continue;
                    }

                    ProcessData process = new ProcessData();
                    string name = String.Empty;

                    try {
                        int i = 0;
                        // Find name:
                        name = nextToken(line, ref i);

                        // Find exe path:
                        process.ExePath = nextParameterToken(line, ref i);

                        // Find shell flag:
                        if (!process.parseShell(nextToken(line, ref i)))
                            throw new BadConfigException("Could not parse shell flag token", mFileName, l, (uint) i);

                        // Find start folder:
                        string startFolder = nextParameterToken(line, ref i);
                        if (startFolder != String.Empty)
                            process.StartFolder = startFolder;

                        string startExe = nextParameterToken(line, ref i);
                        if (startExe != String.Empty)
                            process.StartPath = startExe;

                        // Parse arguments:
                        parseArguments(process, line, i);
                    } catch (BadConfigException e) {
                        e.Line = l;
                        throw e;
                    }

                    newProcess(name, process);
                }

                inStream.Close();
            }

            /// <summary>
            ///     Check that the user corresponding to the given id is an authorized user.
            /// </summary>
            /// <param name="idOwner">Id of a user</param>
            /// <seealso cref="checkAcl"/>
            private void checkUser(IdentityReference idOwner)
            {
                log.Debug("Owner identity: " + idOwner);

                if ((idOwner != idSystem) && (idOwner != idAdmin) && !idOwner.ToString().StartsWith("S-1-5-80"))
                    throw new UnauthorizedAccessException("Users should not be owner of the configuration file.");
            }

            /// <summary>
            ///     Check an Access Control List.
            /// </summary>
            /// <para>
            ///     No ordinary user should able to modify the corresponding file.
            /// </para>
            /// <param name="acl">An ACL</param>
            private void checkAcl(AuthorizationRuleCollection acl)
            {
                Dictionary<IdentityReference, FileSystemRights> userAllowRights = new Dictionary<IdentityReference, FileSystemRights>();
                Dictionary<IdentityReference, FileSystemRights> userDenyRights = new Dictionary<IdentityReference, FileSystemRights>();

                for (int i = 0; i < acl.Count; i++) {
                    log.Debug("Authorization rule type: " + acl[i].GetType());
                    FileSystemAccessRule ace = acl[i] as FileSystemAccessRule;
                    if (ace != null) {
                        log.Debug("Access type: " + ace.AccessControlType);
                        log.Debug("Access mask: " + ace.FileSystemRights);
                        log.Debug("Identity: " + ace.IdentityReference);
                    }

                    // If rule is not a FileSystemSecurityRule then it is ignored.
                    if (ace == null)
                        continue;

                    // Computes allowance of the identity:
                    if (ace.AccessControlType == AccessControlType.Allow) {
                        if (!userAllowRights.ContainsKey(ace.IdentityReference))
                            userAllowRights.Add(ace.IdentityReference, 0);
                        userAllowRights[ace.IdentityReference] |= ace.FileSystemRights;
                    }

                    if (ace.AccessControlType == AccessControlType.Deny) {
                        if (!userDenyRights.ContainsKey(ace.IdentityReference))
                            userDenyRights.Add(ace.IdentityReference, 0);
                        userDenyRights[ace.IdentityReference] |= ace.FileSystemRights;
                    }
                }

                foreach (KeyValuePair<IdentityReference, FileSystemRights> keyval in userAllowRights) {
                    // Authorized users can do whatever they want:
                    try {
                        checkUser(keyval.Key);
                        continue;
                    } catch (UnauthorizedAccessException) {
                    }

                    // Computes real user rights:
                    FileSystemRights rights = keyval.Value;
                    if (userDenyRights.ContainsKey(keyval.Key))
                        rights &= (~userDenyRights[keyval.Key]);

                    // Check user rights
                    if (((rights & FileSystemRights.WriteData) != 0)
                     || ((rights & FileSystemRights.AppendData) != 0)
                     || ((rights & FileSystemRights.WriteAttributes) != 0)
                     || ((rights & FileSystemRights.WriteExtendedAttributes) != 0)
                     || ((rights & FileSystemRights.ChangePermissions) != 0)
                     || ((rights & FileSystemRights.Delete) != 0)
                     || ((rights & FileSystemRights.TakeOwnership) != 0))
                        throw new UnauthorizedAccessException("Users should not be able to modify the config file.");
                }
            }

            /// <summary>
            ///     Check the config file.
            /// </summary>
            /// <para>
            ///     Check that the file exists, is on a path that normal users cannot change
            ///     and cannot be changed itself by ordinary users.
            /// </para>
            /// <param name="path">The path to the config file</param>
            /// <see cref="checkConfigPath"/>
            private void checkConfigFile(string path)
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("Could not find config file", path);

                int sep = path.LastIndexOf('\\');
                if (sep != -1)
                    checkConfigPath(path.Substring(0, sep));

                checkUser(File.GetAccessControl(path, AccessControlSections.Owner).GetOwner(Type.GetType("System.Security.Principal.SecurityIdentifier")));
                checkAcl(File.GetAccessControl(path, AccessControlSections.Access).GetAccessRules(true, true, Type.GetType("System.Security.Principal.SecurityIdentifier")));
            }

            /// <summary>
            ///     Recursively check a path
            /// </summary>
            /// <para>
            ///     Check that the path can not be altered by an ordinary user.
            /// </para>
            /// <param name="path">The path to check</param>
            /// <see cref="checkConfigFile"/>
            private void checkConfigPath(string path)
            {
                int sep = path.LastIndexOf('\\');
                if (sep != -1)
                    checkConfigPath(path.Substring(0, sep));
                else
                    return; // This is the drive letter.

                checkUser(Directory.GetAccessControl(path, AccessControlSections.Owner).GetOwner(Type.GetType("System.Security.Principal.SecurityIdentifier")));
                checkAcl(Directory.GetAccessControl(path, AccessControlSections.Access).GetAccessRules(true, true, Type.GetType("System.Security.Principal.SecurityIdentifier")));
            }

            /// <summary>
            ///     Returns the next token.
            /// </summary>
            /// <remarks>
            ///     Quotes are not removed.
            /// </remarks>
            /// <param name="line">The line of data from the config file</param>
            /// <param name="i">The current index</param>
            /// <returns>The next token</returns>
            /// <seealso cref="nextParameterToken"/>
            private string nextToken(string line, ref int i)
            {
                int b;

                // Find beginning of token:
                while ((i < line.Length) && (line[i] == ' '))
                    i++;
                b = i;

                // Find end of token:
                while ((i < line.Length) && (line[i] != ' '))
                    i++;

                return line.Substring(b, i - b);
            }

            /// <summary>
            ///     Parses process arguments.
            /// </summary>
            /// <param name="process">The information about the process</param>
            /// <param name="line">The line of data from the config file</param>
            /// <param name="i">The current index</param>
            private void parseArguments(ProcessData process, string line, int i)
            {
                while (i < line.Length) {
                    string arg = nextParameterToken(line, ref i);
                    if (arg != String.Empty)
                        process.addArgument(arg);
                }
            }

            /// <summary>
            ///     Returns the next parameter token.
            /// </summary>
            /// <remarks>
            ///     Quotes are removed.
            /// </remarks>
            /// <param name="line">The line of data from the config file</param>
            /// <param name="i">The current index</param>
            /// <returns>The next parameter token</returns>
            /// <seealso cref="nextToken"/>
            private string nextParameterToken(string line, ref int i)
            {
                int b;
                bool escaped = false;

                // Find beginning of token:
                while ((i < line.Length) && (line[i] != '"'))
                    i++;

                b = i;
                i++;

                // Find end of token:
                while (i < line.Length) {
                    // End of parameter token:
                    if (!escaped && (line[i] == '"'))
                        break;
                    if (line[i] == '\\')
                        escaped = !escaped;
                    else
                        escaped = false;
                    i++;
                }
                i++;

                if (b >= line.Length)
                    return String.Empty;
                if (i > line.Length)
                    throw new BadConfigException("Unexpected end of line", mFileName, 0, (uint)line.Length);

                return line.Substring(b + 1, i - b - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
            }
        }
    }
}

