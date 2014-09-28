using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using log4net;

namespace GlobalHotKeys
{
    namespace Windows
    {
        class PlainTextConfig : ProcessesProvider
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(PlainTextConfig));

            private static readonly IdentityReference idSystem = new NTAccount("System").Translate(Type.GetType("System.Security.Principal.SecurityIdentifier"));
            private static readonly IdentityReference idAdmin = new NTAccount("Administrateurs").Translate(Type.GetType("System.Security.Principal.SecurityIdentifier"));

            private string mFileName;
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

            public PlainTextConfig(string filename)
                : base()
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

                    ProcessData process = new ProcessData();
                    string name = String.Empty;

                    try {
                        int i = 0;
                        // Find name:
                        name = nextToken(line, ref i);

                        // Find exe path:
                        process.ExePath = nextParameterToken(line, ref i);

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

            private void checkUser(IdentityReference idOwner)
            {
                log.Debug("Owner identity: " + idOwner);

                if ((idOwner != idSystem) && (idOwner != idAdmin) && !idOwner.ToString().StartsWith("S-1-5-80"))
                    throw new UnauthorizedAccessException("Users should not be owner of the configuration file.");
            }

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

            private void parseArguments(ProcessData process, string line, int i)
            {
                while (i < line.Length) {
                    string arg = nextParameterToken(line, ref i);
                    if (arg != String.Empty)
                        process.addArgument(arg);
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
}

