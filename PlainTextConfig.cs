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
    ///         <item><term><code>ALT: </code></term><description><code>[O|X|L|R]</code> to activate, deacitivate, select right or left ALT modifier</description></item>
    ///         <item><term><code>CTRL: </code></term><description><code>[O|X]</code> to activate, deacitivate, select right or left CTRL modifier</description></item>
    ///         <item><term><code>SHIFT: </code></term><description><code>[O|X]</code> to activate, deacitivate, select right or left SHIFT modifier</description></item>
    ///         <item><term><code>META: </code></term><description><code>[O|X]</code> to activate, deacitivate, select right or left META modifier</description></item>
    ///         <item><term>Key: </term><description>The key, see <see cref="ShortcutData.Keys"/> for key mnemonics</description></item>
    ///         <item><term>Class: </term><description>The class where the method is.</description></item>
    ///         <item><term>Method: </term><description>The method to invoke.</description></item>
    ///         <item><term>Params: </term><description>The params to pass to the methods. The can be multiple params</description></item>
    ///     </list>
    ///     For an exemple of such a config file, see the file <code>globalhotkeys.conf</code> in the project folder.
    /// </para>
    class PlainTextConfig : ConfigProvider
    {
        /// <summary>
        ///     Logger for GlobalHokKeys.
        /// </summary>
        /// <remarks>See Apache Log4net documentation for the logging interface.</remarks>
        private static readonly ILog log = LogManager.GetLogger(typeof(PlainTextConfig));

        /// <summary>
        ///     The config file name. 
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
        ///     The existance of the file is checked.
        /// </remarks>
        /// <param name="filename">The file name containing the configuration</param>
        public PlainTextConfig(string filename)
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

                ShortcutData shortcut = new ShortcutData();

                try {
                    parseModifiers(shortcut, line);

                    int i = 8;
                    string keyToken = nextToken(line, ref i);
                    try {
                        shortcut.setKey(keyToken);
                    } catch (System.Reflection.ReflectionTypeLoadException e) {
                        throw new BadConfigException("Invalid key name: " + keyToken, mFileName, l, (uint)i, e);
                    }

                    // Find class and method:
                    shortcut.Class = nextToken(line, ref i);
                    shortcut.Method = nextToken(line, ref i);

                    // Parse parameters:
                    parseParameters(shortcut, line, i);
                } catch (BadConfigException e) {
                    e.Line = l;
                    throw e;
                }

                newShortcut(shortcut);
            }

            inStream.Close();
        }

        /// <summary>
        ///     Checks the config file
        /// </summary>
        /// <para>In this case only the existance of the file is checked.</para>
        /// <param name="path">The path of the file</param>
        private void checkConfigFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find config file", path);
        }

        /// <summary>
        ///     Parse the modifiers for a shortcut.
        /// </summary>
        /// <param name="shortcut">The shortcut where to set modifiers</param>
        /// <param name="line">The line of data from the config file</param>
        private void parseModifiers(ShortcutData shortcut, string line)
        {
            Array modifiers = Enum.GetValues(typeof(ShortcutData.Modifiers));

            // Line is too short to hold modifiers:
            if (line.Length < 8)
                throw new BadConfigException("Unexpected end of line", mFileName, 0, 8);

            for (int i = 0; i < modifiers.Length / 3; i++) {
                if (line[2 * i + 1] == 'X')
                    shortcut.addModifier((ShortcutData.Modifiers)modifiers.GetValue(2 + 3 * i));
                else if (line[2 * i + 1] == 'L')
                    shortcut.addModifier((ShortcutData.Modifiers)modifiers.GetValue(1 + 3 * i));
                else if (line[2 * i + 1] == 'R')
                    shortcut.addModifier((ShortcutData.Modifiers)modifiers.GetValue(0 + 3 * i));
                else if (line[2 * i + 1] == 'O')
                    shortcut.removeModifier((ShortcutData.Modifiers)modifiers.GetValue(2 + 3 * i));
                else
                    throw new BadConfigException("Invalid setting for " + (ShortcutData.Modifiers)modifiers.GetValue(1 + 3 * i) + " modifier", mFileName, 0, (uint)(2 * i + 2));
            }
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
        ///     Parses the parameter list.
        /// </summary>
        /// <param name="shortcut">The shortcut where to set parameters</param>
        /// <param name="line">The line of data from the config file</param>
        /// <param name="i">The current index</param>
        private void parseParameters(ShortcutData shortcut, string line, int i)
        {
            while (i < line.Length) {
                string param = nextParameterToken(line, ref i);
                if (param != String.Empty)
                    shortcut.Params.Add(param);
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
