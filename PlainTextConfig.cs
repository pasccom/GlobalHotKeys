using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using log4net;

namespace GlobalHotKeys
{
    class PlainTextConfig : ConfigProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PlainTextConfig));

        private string mFileName;
        public string FileName {
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

        public PlainTextConfig(string filename)
        {
            FileName = filename;
        }

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

        private void checkConfigFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find config file", path);
        }

        private void parseModifiers(ShortcutData shortcut, string line)
        {
            ShortcutData.Modifiers[] modifiers = { ShortcutData.Modifiers.ALT, ShortcutData.Modifiers.CTRL, ShortcutData.Modifiers.SHIFT, ShortcutData.Modifiers.META };

            // Line is too short to hold modifiers:
            if (line.Length < 8)
                throw new BadConfigException("Unexpected end of line", mFileName, 0, 8);

            for (int i = 0; i < modifiers.Length; i++) {
                if (line[2 * i + 1] == 'X')
                    shortcut.addModifier(modifiers[i]);
                else if (line[2 * i + 1] == 'O')
                    shortcut.removeModifier(modifiers[i]);
                else
                    throw new BadConfigException("Invalid setting for " + modifiers[i] + " modifier", mFileName, 0, (uint)(2 * i + 2));
            }
        }

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

        private void parseParameters(ShortcutData shortcut, string line, int i)
        {
            while (i < line.Length) {
                string param = nextParameterToken(line, ref i);
                if (param != String.Empty)
                    shortcut.Params.Add(param);
            }
        }

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
