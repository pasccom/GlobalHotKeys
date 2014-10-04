using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class Handler
        {
            static private readonly ILog log = LogManager.GetLogger(typeof(Handler));

            private List<ShortcutData> mDefaultShortcutsList;
            private List<ShortcutData> mCurrentShortcutsList;
            private HookHandler mHookHandler;
            private static Handler sInstance;

            public enum SearchScope
            {
                All,
                AllCurrentFirst = All,
                AllDefaultFirst,
                Current,
                Default
            }

            static public List<string> AuthorizedMethods
            {
                get
                {
                    /* NOTE exit and reset are phantom methods, needed for the implementation of special shortcuts.
                     * Trying to call these will fail.
                     */
                    return new List<string>() { "exit", "reset", "resetShortcuts", "loadConfig" };
                }
            }

            public static Handler getInstance()
            {
                return sInstance;
            }

            public static Handler getInstance(ConfigProvider config)
            {
                if (sInstance == null)
                    sInstance = new Handler(config);
                else
                    sInstance.changeDefaultConfig(config);

                return sInstance;
            }

            public static ConfigProvider getConfigProvider(string path)
            {
                // TODO Add XML, database and registry key configuration possiblilities.
                return new PlainTextConfig(path);
            }

            private Handler(ConfigProvider config)
            {
                mHookHandler = new HookHandler();

                loadShortcut(ShortcutData.exitShortcut, -1, false);
                loadShortcut(ShortcutData.resetShortcut, -2, false);

                mDefaultShortcutsList = new List<ShortcutData>();
                setDefaultConfig(config);

                mCurrentShortcutsList = mDefaultShortcutsList;
                loadShortcuts();
            }

            ~Handler()
            {
                mHookHandler.reset();
            }

            public void resetShortcuts(List<string> args)
            {
                if (args.Count != 0)
                    throw new BadArgumentCountException("resetShortcuts() admits no arguments", 0);

                resetShortcuts();

                mCurrentShortcutsList = mDefaultShortcutsList;
                loadShortcuts();
            }

            public void loadConfig(List<string> args)
            {
                if (args.Count != 1)
                    throw new BadArgumentCountException("loadConfig(path) needs 1 argument", 1);

                string path = args[1];

                resetShortcuts();

                mCurrentShortcutsList = new List<ShortcutData>();

                ConfigProvider config = getConfigProvider(path);
                config.NewShortcutEvent += (ShortcutData shortcut) =>
                {
                    log.Info("New shortcut: " + shortcut);
                    mCurrentShortcutsList.Add(shortcut);
                };
                config.parseConfig();

                loadShortcuts();
            }

            private void resetShortcuts()
            {
                mHookHandler.reset();

                foreach (ShortcutData shortcut in mCurrentShortcutsList)
                    shortcut.Id = 0;
                ShortcutData.exitShortcut.Id = 0;
                ShortcutData.resetShortcut.Id = 0;

                loadShortcut(ShortcutData.exitShortcut, -1, false);
                loadShortcut(ShortcutData.resetShortcut, -2, false);
            }

            private void changeDefaultConfig(ConfigProvider config)
            {
                resetShortcuts();

                mDefaultShortcutsList.Clear();
                setDefaultConfig(config);

                loadShortcuts();
            }

            private void setDefaultConfig(ConfigProvider config)
            {
                config.NewShortcutEvent += (ShortcutData shortcut) =>
                {
                    log.Info("New shortcut: " + shortcut);
                    mDefaultShortcutsList.Add(shortcut);
                };
                config.parseConfig();
            }

            public void exec()
            {
                User32.MSG msg;
                while (User32.GetMessage(out msg, IntPtr.Zero, User32.WM_HOTKEY, User32.WM_HOTKEY)) {
                    if (msg.message != User32.WM_HOTKEY) {
                        log.Warn("Unhandled message received: " + msg.message);
                        continue;
                    }

                    /* Retrive id.
                     * NOTE Different versions of the .NET framework have differents settings for the wParam internal type:
                     *  -4.0: wParam is pointer to a UInt64.
                     *  -4.5: wParam is pointer to a UInt32.
                     */
                    int id;
                    if ((Environment.Version.Major == 4) && (Environment.Version.Minor == 5)) {
                        id = (int)msg.wParam.ToUInt32();
                    } else if ((Environment.Version.Major == 4) && (Environment.Version.Minor == 0)) {
                        id = (int)((long)msg.wParam.ToUInt64());
                    } else {
                        log.ErrorFormat("Your version of the frameWork ({0}) is not supported. Exiting.", Environment.Version);
                        return;
                    }

                    log.Debug("Got hotkey: id=" + id);

                    // Handles exit special shortcut
                    if (id == -1)
                        return;
                    if (id == -2)
                        resetShortcuts(new List<string>());

                    if (id > 0) {
                        if (id <= mCurrentShortcutsList.Count)
                            callShortcut(mCurrentShortcutsList[id - 1]);
                        else
                            log.Warn("Got bad shortcut id: " + id);
                    }
                }
            }

            public ShortcutData findShortcut(ShortcutData.Modifiers shortcutModifiers, ShortcutData.Keys shortcutKey, SearchScope where = SearchScope.All)
            {
                if (mCurrentShortcutsList == mDefaultShortcutsList)
                    return findShortcut(shortcutModifiers, shortcutKey, mCurrentShortcutsList);

                switch (where) {
                case SearchScope.AllCurrentFirst:
                    return findShortcut(shortcutModifiers, shortcutKey, mCurrentShortcutsList.Concat(mDefaultShortcutsList).ToList());
                case SearchScope.AllDefaultFirst:
                    return findShortcut(shortcutModifiers, shortcutKey, mDefaultShortcutsList.Concat(mCurrentShortcutsList).ToList());
                case SearchScope.Current:
                    return findShortcut(shortcutModifiers, shortcutKey, mCurrentShortcutsList);
                case SearchScope.Default:
                    return findShortcut(shortcutModifiers, shortcutKey, mDefaultShortcutsList);
                default:
                    throw new ArgumentException("Bad value for search scope: " + where);
                }
            }

            public List<ShortcutData> findAllShortcuts(string shortcutClass, string shortcutMethod, SearchScope where = SearchScope.All)
            {
                if (mCurrentShortcutsList == mDefaultShortcutsList)
                    return findAllShortcuts(shortcutClass, shortcutMethod, mCurrentShortcutsList);

                switch (where) {
                case SearchScope.AllCurrentFirst:
                    return findAllShortcuts(shortcutClass, shortcutMethod, mCurrentShortcutsList.Concat(mDefaultShortcutsList).ToList());
                case SearchScope.AllDefaultFirst:
                    return findAllShortcuts(shortcutClass, shortcutMethod, mDefaultShortcutsList.Concat(mCurrentShortcutsList).ToList());
                case SearchScope.Current:
                    return findAllShortcuts(shortcutClass, shortcutMethod, mCurrentShortcutsList);
                case SearchScope.Default:
                    return findAllShortcuts(shortcutClass, shortcutMethod, mDefaultShortcutsList);
                default:
                    throw new ArgumentException("Bad value for search scope: " + where);
                }
            }

            private ShortcutData findShortcut(ShortcutData.Modifiers shortcutModifiers, ShortcutData.Keys shortcutKey, List<ShortcutData> where)
            {
                return where.Find((ShortcutData shortcut) =>
                {
                    return ((shortcut.Modifier == shortcutModifiers) && (shortcut.Key == shortcutKey));
                }).Clone();
            }

            private List<ShortcutData> findAllShortcuts(string shortcutClass, string shortcutMethod, List<ShortcutData> where)
            {
                return where.FindAll((ShortcutData shortcut) =>
                {
                    return ((shortcut.Class == shortcutClass) && (shortcut.Method == shortcutMethod));
                }).ConvertAll<ShortcutData>((ShortcutData input) => { return input.Clone(); });
            }

            private void checkShortcut(ShortcutData shortcut)
            {
                // Check that shortcut is valid:
                if (!shortcut.isValid())
                    throw new InvalidShortcutException("Loading an invalid shortcut is forbidden", shortcut);

                // Gets the method class :
                Type providerClass = Type.GetType("GlobalHotKeys." + shortcut.Class);
                if (providerClass == null)
                    throw new InvalidShortcutException("Couldn't find the specified class: " + shortcut.Class, shortcut);
                // Gets the list of authorized methods:
                List<string> authorizedMethods;
                try {
                    authorizedMethods = providerClass.InvokeMember(
                        "AuthorizedMethods",
                        BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public,
                        null,
                        null,
                        null
                    ) as List<string>;
                    log.Debug("Authorized methods are: " + String.Join<string>(",", authorizedMethods));
                } catch (Exception e) {
                    throw new UnauthorizedShortcutException("Could not find the static public property AuthorizedMethods in specified class: " + shortcut.Class, shortcut, e);
                }

                // Checks that the called method is in the list of authorized methods:
                if ((authorizedMethods == null) || (authorizedMethods.Find((method) =>
                {
                    return (method == shortcut.Method);
                }) == null))
                    throw new UnauthorizedShortcutException("Not authorized method: " + shortcut.Method, shortcut);
            }

            private void callShortcut(ShortcutData shortcut)
            {
                checkShortcut(shortcut);

                log.InfoFormat("Calling {0}", shortcut.action());

                // Try to get the singleton:
                try {
                    Type providerClass = Type.GetType("GlobalHotKeys." + shortcut.Class, true);
                    Object singleton = providerClass.InvokeMember(
                        "getInstance",
                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                        null,
                        null,
                        null
                    );
                    providerClass.InvokeMember(
                        shortcut.Method,
                        BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.ExactBinding,
                        null,
                        singleton,
                        new Object[] { shortcut.Params }
                    );
                    return;
                } catch (TargetInvocationException e) {
                    log.Error("Shortcut method failed with exception", e.GetBaseException());
                } catch (Exception) {
                    // Ignore excepetion.
                }

                // Try to invoke a static method (there seems to be no singleton):
                try {
                    Type providerClass = Type.GetType("GlobalHotKeys." + shortcut.Class, true);
                    providerClass.InvokeMember(
                        shortcut.Method,
                        BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.ExactBinding,
                        null,
                        null,
                        new Object[] { shortcut.Params }
                    );
                } catch (TargetInvocationException e) {
                    log.Error("Shortcut method failed with exception.", e.GetBaseException());
                } catch (Exception e) {
                    log.Error("Couldn't find the specified method.", e);
                }

            }

            public void registerShortcut(ShortcutData shortcut)
            {
                int s = 0;
                while (++s <= mCurrentShortcutsList.Count)
                    if ((mCurrentShortcutsList[s - 1].Id != 0) && (mCurrentShortcutsList[s - 1].Id != s))
                        break;

                loadShortcut(shortcut, s, true);
                mCurrentShortcutsList.Insert(s - 1, shortcut);
            }

            public void unregisterShortcut(ShortcutData shortcut)
            {
                int s = findShortcutId(shortcut);

                if (s > mCurrentShortcutsList.Count)
                    throw new InvalidShortcutException("Shortcut is not currently loaded.", shortcut);

                mHookHandler.unloadShortcut(shortcut.Modifier, shortcut.Key);
                mCurrentShortcutsList.RemoveAt(s - 1);
            }

            public void replaceShortcut(ShortcutData oldShortcut, ShortcutData newShortcut)
            {
                int s = findShortcutId(oldShortcut);

                if (s > mCurrentShortcutsList.Count)
                    throw new InvalidShortcutException("Shortcut is not currently loaded.", oldShortcut);

                mHookHandler.unloadShortcut(oldShortcut.Modifier, oldShortcut.Key);
                mCurrentShortcutsList.RemoveAt(s - 1);
                if (newShortcut.Id == 0) {
                    loadShortcut(newShortcut, s, true);
                    mCurrentShortcutsList.Insert(s - 1, newShortcut);
                }
            }

            public void activateShortcut(ShortcutData shortcut)
            {
                int s = findShortcutId(shortcut);

                if (s > mCurrentShortcutsList.Count)
                    throw new InvalidShortcutException("Shortcut is not currently loaded.", shortcut);

                loadShortcut(mCurrentShortcutsList[s - 1], s, true);
            }

            public void deactivateShortcut(ShortcutData shortcut)
            {
                int s = findShortcutId(shortcut);

                if (s > mCurrentShortcutsList.Count)
                    throw new InvalidShortcutException("Shortcut is not currently loaded.", shortcut);

                mHookHandler.unloadShortcut(shortcut.Modifier, shortcut.Key);
                mCurrentShortcutsList[s - 1].Id = 0;
            }

            // TODO: I think the id member is not needed any more. It can be deleted.
            private int findShortcutId(ShortcutData shortcut)
            {
                int s = 0;
                while (++s <= mCurrentShortcutsList.Count)
                    if (shortcut.Equals(mCurrentShortcutsList[s - 1]))
                        break;

                return s;
            }

            private void loadShortcut(ShortcutData shortcut, int id, bool userDefined = true)
            {
                if (userDefined && shortcut.isSpecial())
                    throw new InvalidShortcutException(shortcut.keyCombination() + " is a reserved shortcut", shortcut);

                checkShortcut(shortcut);

                if (shortcut.Id != 0)
                    return;

                mHookHandler.loadShortcut(shortcut.Modifier, shortcut.Key, id);
                log.Info("Sucessfully registered shortcut (id=" + id + "): " + shortcut);
                shortcut.Id = id;
            }

            private void loadShortcuts()
            {
                try {
                    for (int i = 0; i < mCurrentShortcutsList.Count; ) {
                        try {
                            loadShortcut(mCurrentShortcutsList[i], i + 1);
                            i++;
                        } catch (InvalidShortcutException e) {
                            log.Warn("Invalid shortcut ignored: " + e.InvalidShortcut);
                            mCurrentShortcutsList.RemoveAt(i);
                        }
                    }
                } catch (UnauthorizedShortcutException e) {
                    log.Error("Unauthorized shortcut : " + e.UnauthorizedShortcut + ". Clearing all shortcuts.", e);
                    resetShortcuts();
                    mCurrentShortcutsList = new List<ShortcutData>();
                }
            }
        }
    }
}
