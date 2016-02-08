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
using System.Reflection;
using System.Threading;

using log4net;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        /// <summary>
        ///     Singleton handling the shortcuts.
        /// </summary>
        /// <para>
        ///     This singleton class is the core of GlobalHotKeys: It is in charge of dispatching
        ///     the events coming from the HookHandler to the right class.
        ///     The singleton is protected by a mutex and can be retrived with <see cref="getInstance"/>.
        /// </para>
        /// <para>
        ///     It also defines some methods which are available by shortcuts
        ///     <list type="bullet">
        ///         <item><term>exit: </term><description>Phantom method to implement a special shortcut. Exits GlobalHotKeys.</description></item>
        ///         <item><term>reset</term><description>Phantom method to implement a special shortcut. </description></item>
        ///         <item><term>resetShortcuts</term><description></description></item>
        ///         <item><term>loadConfig</term><description></description></item>
        ///         <item><term>noop</term><description></description></item>
        ///     </list>
        /// </para>
        class Handler
        {
            /// <summary>
            ///     Logger for GlobalHokKeys.
            /// </summary>
            /// <remarks>See Apache Log4net documentation for the logging interface.</remarks>
            static private readonly ILog log = LogManager.GetLogger(typeof(Handler));
            /// <summary>
            ///     Queue of actions to be done.
            /// </summary>
            /// <remarks>This is used for thread synchronization</remarks>
            /// <seealso cref="mActionSemaphore"/>
            private Queue<int> mActionQueue;
            /// <summary>
            ///     Semaphore holding the number of actions to be done.
            /// </summary>
            /// <remarks>This is used for thread synchronization</remarks>
            /// <seealso cref="mActionQueue"/>
            private Semaphore mActionSemaphore;
            /// <summary>
            ///     Stores the initial configuration.
            /// </summary>
            /// <para>
            ///     This member stores the configuration which is loaded at startup from the given <see cref="ConfigProvider"/>.
            ///     It can be altered thanks to <see cref="loadConfig"/> and reset thanks to <see cref="resetConfig"/>
            /// </para>
            /// <seealso cref="mCurrentShortcutsList"/>
            private List<ShortcutData> mDefaultShortcutsList;
            /// <summary>
            ///     The list of currently available shortcuts.
            /// </summary>
            /// <para>
            ///     This member stors the list of shortcuts which are currently available.
            ///     This list can be altered at run time using the method <see cref="loadConfig"/>
            ///     and can be reset to its original value with <see cref="resetConfig"/>.
            /// </para>
            /// <seealso cref="mDefaultShortcutsList"/>
            private List<ShortcutData> mCurrentShortcutsList;
            /// <summary>
            ///     Handler for the events coming from the Keyboard.
            /// </summary>
            private HookHandler mHookHandler;
            /// <summary>
            ///     A mutex for the singleton.
            /// </summary>
            private static Mutex sInstanceMutex = new Mutex();
            /// <summary>
            ///     The singleton.
            /// </summary>
            private static Handler sInstance;

            /// <summary>
            ///     Where to search for a shortcut.
            /// </summary>
            public enum SearchScope
            {
                /// <summary>Search everywhere (equivalent to <see cref="AllCurrentFirst"/>)</summary>
                All,
                /// <summary>Search everywhere, current shortcuts first and then default shortcuts</summary>
                AllCurrentFirst = All,
                /// <summary>Search everywhere, default shortcuts first and then current shortcuts</summary>
                AllDefaultFirst,
                /// <summary>Search only in current shortcut list</summary>
                Current,
                /// <summary>Search only in default shortcut list</summary>
                Default
            }

            /// <summary>
            ///     List of declared methods.
            /// </summary>
            /// <remarks>Only these methods should be called externally.</remarks>
            /// <remarks><c>exit</c> and <c>reset</c> should not be called externally. Such calls will fail.</remarks>
            static public List<string> AuthorizedMethods
            {
                get
                {
                    return new List<string>() { "exit", "reset", "resetShortcuts", "loadConfig", "noop" };
                }
            }

            /// <summary>
            ///     Get the singleton.
            /// </summary>
            /// <remarks>This method is protected by a mutex.</remarks>
            /// <returns>The singleton</returns>
            /// <seealso cref="getInstance(ConfigProvider)"/>
            public static Handler getInstance()
            {
                Handler instance;

                sInstanceMutex.WaitOne();
                instance = sInstance;
                sInstanceMutex.ReleaseMutex();

                return instance;
            }
            /// <summary>
            ///     Get the singleton with the given config.
            /// </summary>
            /// <param name="config">The config to read</param>
            /// <remarks>This method is protected by a mutex.</remarks>
            /// <returns>The singleton</returns>
            /// <seealso cref="getInstance()"/>
            public static Handler getInstance(ConfigProvider config)
            {
                Handler instance;

                sInstanceMutex.WaitOne();
                if (sInstance == null)
                    sInstance = new Handler(config);
                else
                    sInstance.changeDefaultConfig(config);
                instance = sInstance;
                sInstanceMutex.ReleaseMutex();

                return instance;
            }
            /// <summary>
            ///     Get the config provider from a config file path.
            /// </summary>
            /// <param name="path">The path to the config file</param>
            /// <remarks>Currently only <see cref="PlainTextConfig"/> is available, so this method is equivalent to
            /// <code>
            ///     new PlainTextConfig(path);
            /// </code></remarks>
            /// <returns>A <see cref="ConfigProvider"/> for the given configuration file</returns>
            public static ConfigProvider getConfigProvider(string path)
            {
                // TODO Add XML, database and registry key configuration possiblilities.
                return new PlainTextConfig(path);
            }

            /// <summary>
            ///     Private constructor.
            /// </summary>
            /// <param name="config">The config to read.</param>
            /// <remarks>To retrieve an instance of this class, use <see cref="getInstance"/>.</remarks>
            private Handler(ConfigProvider config)
            {
                mActionQueue = new Queue<int>();
                mActionSemaphore = new Semaphore(0, 50);
                
                mHookHandler = new HookHandler();

                loadShortcut(ShortcutData.exitShortcut, -1, false);
                loadShortcut(ShortcutData.resetShortcut, -2, false);

                mDefaultShortcutsList = new List<ShortcutData>();
                setDefaultConfig(config);

                mCurrentShortcutsList = mDefaultShortcutsList;
                loadShortcuts();
            }

            /// <summary>
            ///     Destructor.
            /// </summary>
            ~Handler()
            {
                mHookHandler.reset();
            }

            /// <summary>
            ///     Do nothing.
            /// </summary>
            /// <para>
            ///     This method does not take any arguments.
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            public void noop(List<string> args)
            {
                if (args.Count != 0)
                    throw new BadArgumentCountException("noop() admits no arguments", 0);

                log.Info("Called Shortcuts.Handler.noop()");
            }
            /// <summary>
            ///     Reset the current shortcut list.
            /// </summary>
            /// <para>
            ///     This method does not take any arguments.
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            public void resetShortcuts(List<string> args)
            {
                if (args.Count != 0)
                    throw new BadArgumentCountException("resetShortcuts() admits no arguments", 0);

                log.Info("Called Shortcuts.Handler.resetShortcuts()");

                resetShortcuts();

                mCurrentShortcutsList = mDefaultShortcutsList;
                loadShortcuts();
            }
            /// <summary>
            ///     Load a config file.
            /// </summary>
            /// <para>
            ///     Arguments
            ///     <list type="bullet">
            ///         <item><term>path: </term>The path to the config file to read.</item>    
            ///     </list>
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            public void loadConfig(List<string> args)
            {
                if (args.Count != 1)
                    throw new BadArgumentCountException("loadConfig(path) needs 1 argument", 1);

                string path = args[1];

                log.InfoFormat("Called Shortcuts.Handler.loadConfig({0})", path);

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
            /// <summary>
            ///     Reset the current shortcut list.
            /// </summary>
            /// <remarks>The special <c>exit</c> and <c>reset</c> shortcuts are automatically reloaded.</remarks>
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
            /// <summary>
            ///     Load a new config in the default shortcut list.
            /// </summary>
            /// <param name="config">The config to load</param>
            private void changeDefaultConfig(ConfigProvider config)
            {
                resetShortcuts();

                mDefaultShortcutsList.Clear();
                setDefaultConfig(config);

                loadShortcuts();
            }
            /// <summary>
            ///     Load a config in the default shortcut list.
            /// </summary>
            /// <param name="config">The config to load</param>
            private void setDefaultConfig(ConfigProvider config)
            {
                config.NewShortcutEvent += (ShortcutData shortcut) =>
                {
                    log.Info("New shortcut: " + shortcut);
                    mDefaultShortcutsList.Add(shortcut);
                };
                config.parseConfig();
            }

            /// <summary>
            ///     Thread function which calls the methods.
            /// </summary>
            /// <para>
            ///     This function is called in a separate execution thread. 
            ///     It is in charge of consuming the actions coming from the <see cref="HookHandler"/> 
            ///     and calling the corresponding methods.
            /// </para>
            // TODO make it private.
            public void actionThread()
            {
                log.InfoFormat("Worker thread started ({0})", Thread.CurrentThread.ManagedThreadId);

                while (true) {
                    int id = 0;
                    mActionSemaphore.WaitOne();
                    lock (mActionQueue) {
                        id = mActionQueue.Dequeue();
                    }

                    log.InfoFormat("Worker thread received id={0}", id);

                    if (id == -1)
                        break;
                    if (id == -2)
                        resetShortcuts(new List<string>());

                    if (id > 0) {
                        if (id <= mCurrentShortcutsList.Count)
                            callShortcut(mCurrentShortcutsList[id - 1]);
                        else
                            log.Warn("Got bad shortcut id: " + id);
                    }
                }

                log.Info("Worker thread exited");
            }

            /// <summary>
            ///     Main loop.
            /// </summary>
            /// <para>
            ///     This function starts the <see cref="actionThread"/>.
            ///     It also handles the messages, the application receives if global shortcuts are used.
            /// </para>
            public void exec()
            {
                Thread workerThread = new Thread(actionThread);
                workerThread.Start();

                User32.MSG msg;
                sbyte ans;
                int consecutiveErrorCount = 0;

                while ((ans = User32.GetMessage(out msg, IntPtr.Zero, 0, 0)) != 0) {
                    int id;

                    // Check for errors in GetMessage
                    if (ans == -1) {
                        log.ErrorFormat("GetMessage error. Code: {0}", User32.GetLastError());
                        log.InfoFormat("There where {0} consecutive errors in GetMessage", consecutiveErrorCount);
                        consecutiveErrorCount++;
                        // This prevents looping in case error is reccurant.
                        if (consecutiveErrorCount == 10)
                            id = -1;
                        else
                            continue;
                    } else {
                        consecutiveErrorCount = 0;

                        // Filters the messages (normally only get WM_HOTKEY, but who never knows?
                        if (msg.message != User32.WM_HOTKEY) {
                            log.Warn("Unhandled message received: " + msg.message);
                            continue;
                        }

                        /* Retrive id.
                            * NOTE Different versions of the .NET framework have differents settings for the wParam internal type:
                            *  -4.0: wParam is pointer to a UInt64.
                            *  -4.5: wParam is pointer to a UInt32.
                            */
                        if ((Environment.Version.Major == 4) && (Environment.Version.Minor == 5)) {
                            id = (int)msg.wParam.ToUInt32();
                        } else if ((Environment.Version.Major == 4) && (Environment.Version.Minor == 0)) {
                            id = (int)((long)msg.wParam.ToUInt64());
                        } else {
                            log.ErrorFormat("Your version of the frameWork ({0}) is not supported. Exiting.", Environment.Version);
                            break;
                        }

                        log.Info("Got hotkey: id=" + id);
                    }

                    lock(mActionQueue) {
                        mActionQueue.Enqueue(id);
                    }
                    try {
                        mActionSemaphore.Release();
                    } catch (SemaphoreFullException e) {
                        log.Warn("Semaphore is totally full", e);
                    }

                    // Handles exit special shortcut
                    if (id == -1)
                        break;
                }

                workerThread.Join();
            }

            /// <summary>
            ///     Wait for the modifiers to be released.
            /// </summary>
            /// <param name="timeout">Maximum time to wait, if set to -1, waits for ever</param>
            /// <returns><c>true</c> if the modifer were released, <c>false</c> if the function timed out</returns>
            public bool waitModifiersReleased(Int32 timeout = -1)
            {
                return mHookHandler.waitModifiersReleased(timeout);
            }

            /// <summary>
            ///     Find a shortcut in the shortcut lists by modifiers and key.
            /// </summary>
            /// <param name="shortcutModifiers">The modifier of the shortcut</param>
            /// <param name="shortcutKey">The key of the shortcut</param>
            /// <param name="where">A flag to tell where the shortcut should be searched</param>
            /// <returns>The matching shortcut data or <c>null</c> if it is not found</returns>
            /// <seealso cref="findAllShortcuts"/>
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
            /// <summary>
            ///     Find all shortcuts in the shortcut lists matching the given class and method.
            /// </summary>
            /// <param name="shortcutClass">The class of the shortcut</param>
            /// <param name="shortcutMethod">The method of the shortcut</param>
            /// <param name="where">A flag to tell where the shortcut should be searched</param>
            /// <returns>A list of matching shortcut data</returns>
            /// <seealso cref="findShortcut"/>
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

            /// <summary>
            ///     Find a shortcut in the shortcut lists by modifiers and key.
            /// </summary>
            /// <param name="shortcutModifiers">The modifier of the shortcut</param>
            /// <param name="shortcutKey">The key of the shortcut</param>
            /// <param name="where">The list of shortcuts where it should be searched</param>
            /// <returns>The matching shortcut data or <c>null</c> if it is not found</returns>
            private ShortcutData findShortcut(ShortcutData.Modifiers shortcutModifiers, ShortcutData.Keys shortcutKey, List<ShortcutData> where)
            {
                return where.Find((ShortcutData shortcut) =>
                {
                    return ((shortcut.Modifier == shortcutModifiers) && (shortcut.Key == shortcutKey));
                }).Clone();
            }
            /// <summary>
            ///     Find all shortcuts in the shortcut lists matching the given class and method.
            /// </summary>
            /// <param name="shortcutClass">The class of the shortcut</param>
            /// <param name="shortcutMethod">The method of the shortcut</param>
            /// <param name="where">The list of shortcuts where it should be searched</param>
            /// <returns>A list of matching shortcut data</returns>
            private List<ShortcutData> findAllShortcuts(string shortcutClass, string shortcutMethod, List<ShortcutData> where)
            {
                return where.FindAll((ShortcutData shortcut) =>
                {
                    return ((shortcut.Class == shortcutClass) && (shortcut.Method == shortcutMethod));
                }).ConvertAll<ShortcutData>((ShortcutData input) => { return input.Clone(); });
            }

            /// <summary>
            ///     Check the given shortcut.
            /// </summary>
            /// <para>
            ///     Check that
            ///     <list type="number">
            ///         <item>The shortcut is not a special one</item>
            ///         <item>The class exists</item>
            ///         <item>The method is in the list of authorized methods for this class</item>
            ///     </list>
            /// </para>
            /// <param name="shortcut">The shortcut to check</param>
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
            /// <summary>
            ///     Call the method assiciated with a shortcut.
            /// </summary>
            /// <para>
            ///     The shortcut is first checked using <see cref="checkShortcut"/>.
            /// </para>
            /// <para>
            ///     First try to retrieve an instance of the given class and invoke the desired member.
            ///     If it fails, try to invoke a static method of the given class.
            /// </para>
            /// <param name="shortcut">The shortcut whose associated method should be invoked</param>
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

            /// <summary>
            ///     Register a shortcut and assign its identifier
            /// </summary>
            /// <para>
            ///     This method searches for a valid identifier for the given shortcut,
            ///     appends it at the right place in the current shortcut list
            ///     and loads it in the <see cref="HookHandler"/>.
            /// </para>
            /// <param name="shortcut">The shortcut to register</param>
            /// <seealso cref="unregisterShortcut"/>
            /// <seealso cref="replaceShortcut"/>
            /// <seealso cref="activateShortcut"/>
            public void registerShortcut(ShortcutData shortcut)
            {
                int s = 0;
                while (++s <= mCurrentShortcutsList.Count)
                    if ((mCurrentShortcutsList[s - 1].Id != 0) && (mCurrentShortcutsList[s - 1].Id != s))
                        break;

                loadShortcut(shortcut, s, true);
                mCurrentShortcutsList.Insert(s - 1, shortcut);
            }
            /// <summary>
            ///     Unregister a shortcut.
            /// </summary>
            /// <para>
            ///     This method finds the identifier of the shortcut,
            ///     takes it from the current shortcut list
            ///     and unloads it form the <see cref="HookHandler"/>.
            /// </para>
            /// <param name="shortcut">The shortcut to unregister</param>
            /// <seealso cref="registerShortcut"/>
            /// <seealso cref="replaceShortcut"/>
            /// <seealso cref="deactivateShortcut"/>
            public void unregisterShortcut(ShortcutData shortcut)
            {
                int s = findShortcutId(shortcut);

                if (s > mCurrentShortcutsList.Count)
                    throw new InvalidShortcutException("Shortcut is not currently loaded.", shortcut);

                mHookHandler.unloadShortcut(shortcut.Modifier, shortcut.Key);
                mCurrentShortcutsList.RemoveAt(s - 1);
            }
            /// <summary>
            ///     Replaces a shortcut by another.
            /// </summary>
            /// <para>
            ///     This function is equivalent to calling successively
            ///     <see cref="unregisterShortcut"/> with the first shortcut
            ///     and <see cref="registerShortcut"/> with the second shortcut.
            /// </para>
            /// <param name="oldShortcut">The shortcut to unregister</param>
            /// <param name="newShortcut">The shortcut to register</param>
            /// <seealso cref="registerShortcut"/>
            /// <seealso cref="unregisterShortcut"/>
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
            /// <summary>
            ///     Activate a shortcut.
            /// </summary>
            /// <para>
            ///     This method finds the identifier of the shortcut
            ///     and loads it in the <see cref="HookHandler"/>.
            /// </para>
            /// <param name="shortcut">The shortcut to activate</param>
            /// <seealso cref="deactivateShortcut"/>
            /// <seealso cref="registerShortcut"/>
            public void activateShortcut(ShortcutData shortcut)
            {
                int s = findShortcutId(shortcut);

                if (s > mCurrentShortcutsList.Count)
                    throw new InvalidShortcutException("Shortcut is not currently loaded.", shortcut);

                loadShortcut(mCurrentShortcutsList[s - 1], s, true);
            }
            /// <summary>
            ///     Deactivate a shortcut.
            /// </summary>
            /// <para>
            ///     This method finds the identifier of the shortcut
            ///     and unloads it from the <see cref="HookHandler"/>.
            /// </para>
            /// <param name="shortcut">The shortcut to deactivate</param>
            /// <seealso cref="activateShortcut"/>
            /// <seealso cref="unregisterShortcut"/>
            public void deactivateShortcut(ShortcutData shortcut)
            {
                int s = findShortcutId(shortcut);

                if (s > mCurrentShortcutsList.Count)
                    throw new InvalidShortcutException("Shortcut is not currently loaded.", shortcut);

                mHookHandler.unloadShortcut(shortcut.Modifier, shortcut.Key);
                mCurrentShortcutsList[s - 1].Id = 0;
            }
            /// <summary>
            ///     Find the id of a shortcut.
            /// </summary>
            /// <param name="shortcut">The shortcut to find</param>
            /// <returns>The identfier of the shortcut (i.e. its place in the current shortcut list). 
            /// If the item is not found in the current shortcut list the value is equal to the size of the list.</returns>
            // TODO: I think the id member is not needed any more. It can be deleted.
            private int findShortcutId(ShortcutData shortcut)
            {
                int s = 0;
                while (++s <= mCurrentShortcutsList.Count)
                    if (shortcut.Equals(mCurrentShortcutsList[s - 1]))
                        break;

                return s;
            }
            /// <summary>
            ///     Load a shortcut in the <see cref="HookHandler"/>
            /// </summary>
            /// <param name="shortcut">The shortcut to load</param>
            /// <param name="id">The identifier of the shortcut</param>
            /// <param name="userDefined">Whether the shortcut is user-defined, in which case it is checked that tha shortcut is not special</param>
            /// <seealso cref="loadShortcuts"/>
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
            /// <summary>
            ///     Load a list shortcut in the <see cref="HookHandler"/>
            /// </summary>
            /// <seealso cref="loadShortcut"/>
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
