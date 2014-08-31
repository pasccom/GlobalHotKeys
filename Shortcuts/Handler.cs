using System;
using System.Collections.Generic;
using System.Reflection;

using GlobalHotkeys.Shortcuts;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class Handler
        {
            private List<Shortcut> mDefaultShortcutsList;
            private List<Shortcut> mCurrentShortcutsList;
            private static Handler sInstance;

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
                loadShortcut(Shortcut.exitShortcut, -1, false);
                loadShortcut(Shortcut.resetShortcut, -2, false);

                mDefaultShortcutsList = new List<Shortcut>();
                setDefaultConfig(config);

                mCurrentShortcutsList = mDefaultShortcutsList;
                loadShortcuts();
            }

            ~Handler()
            {
                unloadShortcuts();

                unloadShortcut(Shortcut.exitShortcut, -1);
                unloadShortcut(Shortcut.resetShortcut, -2);
            }

            public void resetShortcuts()
            {
                unloadShortcuts();
                mCurrentShortcutsList = mDefaultShortcutsList;
                loadShortcuts();
            }

            public void loadConfig(string path)
            {
                unloadShortcuts();

                mCurrentShortcutsList = new List<Shortcut>();

                ConfigProvider config = getConfigProvider(path);
                config.NewShortcutEvent += (Shortcut shortcut) =>
                {
                    Console.WriteLine("New shortcut: " + shortcut);
                    mCurrentShortcutsList.Add(shortcut);
                };
                config.parseConfig();

                loadShortcuts();
            }

            private void changeDefaultConfig(ConfigProvider config)
            {
                unloadShortcuts();

                mDefaultShortcutsList.Clear();
                setDefaultConfig(config);

                loadShortcuts();
            }

            private void setDefaultConfig(ConfigProvider config)
            {
                config.NewShortcutEvent += (Shortcut shortcut) =>
                {
                    Console.WriteLine("New shortcut: " + shortcut);
                    mDefaultShortcutsList.Add(shortcut);
                };
                config.parseConfig();
            }

            public void exec()
            {
                User32.MSG msg;
                while (User32.GetMessage(out msg, IntPtr.Zero, User32.WM_HOTKEY, User32.WM_HOTKEY)) {
                    if (msg.message != User32.WM_HOTKEY) {
                        Console.WriteLine("Unhandled message received: " + msg.message);
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
                        Console.WriteLine("Your version of the frameWork (" + Environment.Version + ") is not supported. Exiting.");
                        return;
                    }

                    Console.WriteLine("Got hotkey: id=" + id);

                    // Handles exit special shortcut
                    if (id == -1)
                        return;
                    if (id == -2)
                        resetShortcuts();

                    if (id > 0) {
                        if (id <= mCurrentShortcutsList.Count)
                            callShortcut(mCurrentShortcutsList[id - 1]);
                        else
                            Console.WriteLine("Got bad shortcut id: " + id);
                    }
                }
            }

            private void checkShortcut(Shortcut shortcut)
            {
                // Prevents an empty shortcut to load:
                if ((shortcut.Class == null) || (shortcut.Class == String.Empty))
                    throw new InavalidShortcutException("Loading a shortcut with no or empty class is forbidden", shortcut);
                if ((shortcut.Method == null) || (shortcut.Method == String.Empty))
                    throw new InavalidShortcutException("Loading a shortcut with no or empty method is forbidden", shortcut);

                // Gets the method class :
                Type providerClass = Type.GetType("GlobalHotKeys." + shortcut.Class);
                if (providerClass == null)
                    throw new InavalidShortcutException("Couldn't find the specified class: " + shortcut.Class, shortcut);
                // Gets the list of authorized methods:
                List<string> authorizedMethods;
                try {
                    authorizedMethods = providerClass.InvokeMember(
                        "AuthorizedMethods", 
                        BindingFlags.DeclaredOnly | BindingFlags.Static |  BindingFlags.GetProperty | BindingFlags.Public,
                        null, 
                        null,
                        null
                     ) as List<string>;
                    Console.WriteLine("Authorized methods are: " + authorizedMethods);
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

            private void callShortcut(Shortcut shortcut)
            {
                checkShortcut(shortcut);

                Object[] param = new Object[shortcut.Params.Count];
                for (int i = 0; i < shortcut.Params.Count; i++)
                    param[i] = shortcut.Params[i];

                Console.WriteLine("Calling {0}.{1}({2})", shortcut.Class, shortcut.Method, String.Join<string>(",", shortcut.Params));

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
                        BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public,
                        null,
                        singleton,
                        param
                    );
                    return;
                } catch (TargetInvocationException e) {
                    Console.WriteLine("Shortcut method failed with exception: " + e.GetBaseException());
                } catch (Exception) {
                    // Ignore excepetion.
                }

                // Try to invoke a static method (there seems to be no singleton):
                try {
                    Type providerClass = Type.GetType("GlobalHotKeys." + shortcut.Class, true);
                    providerClass.InvokeMember(
                        shortcut.Method,
                        BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public,
                        null,
                        null,
                        param
                    );
                } catch (TargetInvocationException e) {
                    Console.WriteLine("Shortcut method failed with exception: " + e.GetBaseException());
                } catch (Exception e) {
                    Console.WriteLine("Couldn't find the specified method. Exception: " + e);
                }

            }

            private void loadShortcut(Shortcut shortcut, int id, bool userDefined = true)
            {
                // Prevents the user to load CTRL + ALT + Escape reserved shortcut:
                if (userDefined && (shortcut.Modifier == (Shortcut.Modifiers.ALT | Shortcut.Modifiers.CTRL)) && (shortcut.Key == Shortcut.Keys.Esc))
                    throw new InavalidShortcutException("CTRL + ALT + ECHAP is a reserved shortcut", shortcut);
                // Prevents the user to load CTRL + ALT + C reserved shortcut:
                if (userDefined && (shortcut.Modifier == (Shortcut.Modifiers.ALT | Shortcut.Modifiers.CTRL)) && (shortcut.Key == Shortcut.Keys.C))
                    throw new InavalidShortcutException("CTRL + ALT + C is a reserved shortcut", shortcut);

                checkShortcut(shortcut);

                if (shortcut.Loaded)
                    return;

                if (!User32.RegisterHotKey(IntPtr.Zero, id, (int)shortcut.Modifier | User32.MOD_NOREPEAT, (int)shortcut.Key))
                    throw new InavalidShortcutException("Couldn't register shortcut (id=" + id + ").", shortcut);
                Console.WriteLine("Sucessfully registered shortcut (id=" + id + "): " + shortcut);
                shortcut.Loaded = true;
            }

            private void unloadShortcut(Shortcut shortcut, int id)
            {
                if (!shortcut.Loaded)
                    return;

                if (!User32.UnregisterHotKey(IntPtr.Zero, id))
                    throw new InavalidShortcutException("Couldn't unregister shortcut (id=" + id + ").", shortcut);

                Console.WriteLine("Sucessfully unregistered shortcut (id=" + id + "): " + shortcut);
                shortcut.Loaded = false;
            }

            private void loadShortcuts()
            {
                try {
                    for (int i = 0; i < mCurrentShortcutsList.Count; ) {
                        try {
                            loadShortcut(mCurrentShortcutsList[i], i + 1);
                            i++;
                        } catch (InavalidShortcutException e) {
                            Console.WriteLine("Invalid shortcut ignored: " + e.InvalidShortcut);
                            mCurrentShortcutsList.RemoveAt(i);
                        }
                    }
                } catch (UnauthorizedShortcutException e) {
                    Console.WriteLine("Unauthorized shortcut : " + e.UnauthorizedShortcut + ". Clearing all shortcuts.");
                    unloadShortcuts();
                    mCurrentShortcutsList = new List<Shortcut>();
                }
            }

            private void unloadShortcuts()
            {
                for (int i = 0; i < mCurrentShortcutsList.Count; ) {
                    try {
                        unloadShortcut(mCurrentShortcutsList[i], i + 1);
                        i++;
                    } catch (InavalidShortcutException e) {
                        Console.WriteLine("Invalid shortcut ignored: " + e.InvalidShortcut);
                    }
                }
            }
        }
    }
}
