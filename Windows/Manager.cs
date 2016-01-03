using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace GlobalHotKeys
{
    namespace Windows
    {
        /// <summary>
        ///     Defines the methods of Windows namespace.
        /// </summary>
        /// <para>
        ///     This is the main class of Windows namespace, it defines the methods available from this namespace. They are
        ///     <list type="bullet">
        ///         <item><term><see cref="activate"/>: </term><description>Bring foreground or start a process
        ///             <list type="bullet">
        ///                 <item><term>SizeFlag: </term><description>One of <c>"Default"</c>, <c>"Normal"</c>, <c>"Maximize"</c> and <c>"ShowMaximized"</c></description></item>
        ///                 <item><term>Process: </term><description>The name of the process</description></item>
        ///                 <item><term>Title (opt): </term><description>A regexp which should match the title of the window to activate</description></item>
        ///             </list>
        ///         </description></item>
        ///         <item><term><see cref="start"/>: </term><description>Start a process
        ///             <list type="bullet">
        ///                 <item><term>Process: </term><description>The name of the process</description></item>
        ///             </list>
        ///         </description></item>
        ///         <item><term><see cref="focus"/>: </term><description>Bring foreground a process
        ///             <list type="bullet">
        ///                 <item><term>SizeFlag: </term><description>One of <c>"Default"</c>, <c>"Normal"</c>, <c>"Maximize"</c> and <c>"ShowMaximized"</c></description></item>
        ///                 <item><term>Process: </term><description>The name of the process</description></item>
        ///                 <item><term>Title (opt): </term><description>A regexp which should match the title of the window to activate</description></item>
        ///             </list>
        ///         </description></item>
        ///         <item><term><see cref="sendKeys"/>: </term><description>Send a key sequence to an application
        ///             <list type="bullet">
        ///                 <item><term>Keys: </term><description>Key sequence to send to the focussed application</description></item>
        ///             </list>
        ///         </description></item>
        ///     </list>
        /// </para>
        class Manager
        {
            /// <summary>
            ///     Provider of processes.
            /// </summary>
            /// <seealso cref="ProcessessList"/>
            static private ProcessesProvider mProcessessList;
            /// <summary>
            ///     Logger for GlobalHokKeys.
            /// </summary>
            /// <remarks>See Apache Log4net documentation for the logging interface.</remarks>
            static private readonly ILog log = LogManager.GetLogger(typeof(Manager));

            /// <summary>
            ///     Setter for the process provider
            /// </summary>
            static public ProcessesProvider ProcessessList
            {
                set
                {
                    if (value != null)
                        value.parseConfig();
                    mProcessessList = value;
                }
            }
            /// <summary>
            ///     List of declared methods.
            /// </summary>
            /// <remarks>Only these methods should be called externally.</remarks>
            static public List<string> AuthorizedMethods
            {
                get
                {
                    return new List<string>() { "activate", "start", "focus", "sendKeys" };
                }
            }

            /// <summary>
            ///     Bring foreground or start a process.
            /// </summary>
            /// <para>
            ///     Arguments
            ///     <list type="bullet">
            ///         <item><term>SizeFlag: </term><description>One of <c>"Default"</c>, <c>"Normal"</c>, <c>"Maximize"</c> and <c>"ShowMaximized"</c></description></item>
            ///         <item><term>Process: </term><description>The name of the process</description></item>
            ///         <item><term>Title (opt): </term><description>A regexp which should match the title of the window to activate</description></item>
            ///     </list>
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            /// <seealso cref="start"/>
            /// <seealso cref="focus"/>
            static public void activate(List<string> args)
            {
                if ((args.Count < 2) || (args.Count > 3))
                    throw new Shortcuts.BadArgumentCountException("activate(size, processName, [title]) needs 2 arguments at least and admits 1 optional argument.", 2, 3);

                User32.ShowState sizeFlag = parseSize(args[0]);
                string processName = args[1];
                string title = null;
                if (args.Count >= 3)
                    title = args[2];

                log.InfoFormat("Called Windows.Manager.activate({0}, \"{1}\", \"{2}\")", sizeFlag, processName, title);

                ProcessData process = mProcessessList[processName];

                List<Process> processes = findProcesses(process.ExePath);
                if (processes.Count == 0) {
                    // Start process
                    startProcess(process);
                } else {
                    if (title == null) {
                        // Use the .NET provided MainWindowHandle (not always good...)
                        activate(processes[0].MainWindowHandle, sizeFlag);
                    } else {
                        // Look for a visible window whose title matches the provided regexp
                        List<IntPtr> windows = findVisibleWindows(processes[0].Threads, title);
                        if (windows.Count == 0)
                            log.Warn("No matching windows");
                        else
                            activate(windows[0], sizeFlag);

                        if (windows.Count > 1)
                            log.WarnFormat("{0} matching windows ignored", windows.Count - 1);
                    }
                }

                if (processes.Count > 1)
                    log.WarnFormat("{0} matching processes ignored", processes.Count - 1);
            }

            /// <summary>
            ///     Bring foreground a process.
            /// </summary>
            /// <para>
            ///     Arguments
            ///     <list type="bullet">
            ///         <item><term>SizeFlag: </term><description>One of <c>"Default"</c>, <c>"Normal"</c>, <c>"Maximize"</c> and <c>"ShowMaximized"</c></description></item>
            ///         <item><term>Process: </term><description>The name of the process</description></item>
            ///         <item><term>Title (opt): </term><description>A regexp which should match the title of the window to activate</description></item>
            ///     </list>
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            /// <seealso cref="activate"/>
            static public void focus(List<string> args)
            {
                if ((args.Count < 2) || (args.Count > 3))
                    throw new Shortcuts.BadArgumentCountException("focus(size, processName, [title]) needs 2 arguments at least and admits 1 optional argument.", 2, 3);

                User32.ShowState sizeFlag = parseSize(args[0]);
                string processName = args[1];
                string title = null;
                if (args.Count >= 3)
                    title = args[2];

                log.InfoFormat("Called Windows.Manager.focus({0}, \"{1}\", \"{2}\")", sizeFlag, processName, title);

                ProcessData process = mProcessessList[processName];

                List<Process> processes = findProcesses(process.ExePath);
                if (processes.Count == 0) {
                    log.Warn("No matching process");
                } else {
                    if (title == null) {
                        // Use the .NET provided MainWindowHandle (not always good...)
                        activate(processes[0].MainWindowHandle, sizeFlag);
                    } else {
                        // Look for a visible window whose title matches the provided regexp
                        List<IntPtr> windows = findVisibleWindows(processes[0].Threads, title);
                        if (windows.Count == 0)
                            log.Warn("No matching windows");
                        else
                            activate(windows[0], sizeFlag);

                        if (windows.Count > 1)
                            log.WarnFormat("{0} matching windows ignored", windows.Count - 1);
                    }
                }

                if (processes.Count > 1)
                    log.WarnFormat("{0} matching processes ignored", processes.Count - 1);
            }

            /// <summary>
            ///     Start a process.
            /// </summary>
            /// <para>
            ///     Arguments
            ///     <list type="bullet">
            ///         <item><term>Process: </term><description>The name of the process</description></item>
            ///     </list>
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            /// <seealso cref="activate"/>
            static public void start(List<string> args)
            {
                if (args.Count != 1)
                    throw new Shortcuts.BadArgumentCountException("start(processName) needs 1 argument.", 1, 1);

                string processName = args[0];

                log.InfoFormat("Called Windows.Manager.start(\"{0}\")", processName);

                startProcess(mProcessessList[processName]);
            }

            /// <summary>
            ///     Send a key sequence to an application.
            /// </summary>
            /// <para>
            ///     Arguments
            ///     <list type="bullet">
            ///         <item><term>Keys: </term><description>Key sequence to send to the focussed application</description></item>
            ///     </list>
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            static public void sendKeys(List<string> args)
            {
                if (args.Count != 1)
                    throw new Shortcuts.BadArgumentCountException("sendKey(keys) needs 1 argument.", 1, 1);

                log.InfoFormat("Called Shortcuts.Handler.sendKey({0})", args[0]);

                User32.Input[] keyEvents = new User32.Input[args[0].Length];

                for (int c = 0; c < args[0].Length; c++) {
                    log.DebugFormat("Sending unicode: 0x{0:X4}", (short)args[0][c]);
                    keyEvents[c].type = User32.InputType.KEYBOARD;
                    keyEvents[c].data = new User32.InputUnion();
                    keyEvents[c].data.kInput = new User32.KeyboardInput();
                    keyEvents[c].data.kInput.extraInfo = UIntPtr.Zero;
                    keyEvents[c].data.kInput.time = 0;
                    keyEvents[c].data.kInput.flags = User32.KeyboardEventFlags.UNICODE;
                    keyEvents[c].data.kInput.virtualCode = 0;
                    keyEvents[c].data.kInput.scanCode = (short)args[0][c];
                }

                if (User32.SendInput((uint)args[0].Length, keyEvents, User32.Input.Size) != 1)
                    throw new ApplicationException("Could not send the key to application. Error code: " + User32.GetLastError());
            }

            /// <summary>
            ///     Parses a size flag.
            /// </summary>
            /// <param name="sizeName">The size as a string (one of <c>"Default"</c>, <c>"Normal"</c>, <c>"Maximize"</c> and <c>"ShowMaximized"</c></description>)</param>
            /// <returns>The parsed size flag</returns>
            static private User32.ShowState parseSize(String sizeName)
            {
                bool bad = sizeName.Contains(",");

                try {
                    int.Parse(sizeName);
                    bad = true;
                } catch (Exception) { }

                if (bad)
                    throw new ArgumentException("The size argument should not contain commas or be a number.");

                try {
                    return (User32.ShowState)Enum.Parse(typeof(User32.ShowState), sizeName);
                } catch (ArgumentException) {
                    throw new ArgumentException("The size argument should be either \"Default\", \"Normal\", \"Maximize\" or \"ShowMaximized\"");
                }
            }

            /// <summary>
            ///     Is the given window minimized.
            /// </summary>
            /// <param name="winHandle">A handle to the window</param>
            /// <returns><c>true</c> if the window is minimized, <c>false</c> otherwise</returns>
            static private bool isMiniMized(IntPtr winHandle)
            {
                User32.Rect rect = new User32.Rect();
                if (!User32.GetWindowRect(winHandle, out rect))
                    throw new ApplicationException("Could not get window position. Error code: " + User32.GetLastError());

                return ((rect.Top == -32000) && (rect.Left == -32000));
            }

            /// <summary>
            ///     Activates (bring foreground) the given window.
            /// </summary>
            /// <param name="winHandle">A handle to the window</param>
            /// <param name="sizeFlag">How the window should be shown if minimized</param>
            static private void activate(IntPtr winHandle, User32.ShowState sizeFlag)
            {
                switch (sizeFlag) {
                case User32.ShowState.Restore:
                    if (isMiniMized(winHandle))
                        User32.ShowWindow(winHandle, User32.ShowState.Normal);
                    User32.SetForeground(winHandle);
                    break;
                case User32.ShowState.Maximize:
                    if (isMiniMized(winHandle))
                        User32.ShowWindow(winHandle, User32.ShowState.ShowMaximized);
                    User32.SetForeground(winHandle);
                    break;
                default:
                    User32.ShowWindow(winHandle, sizeFlag);
                    User32.SetForeground(winHandle);
                    break;
                }
            }

            /// <summary>
            ///     Start a process
            /// </summary>
            /// <param name="processData">Information about th process to be started</param>
            static private void startProcess(ProcessData processData)
            {
                log.Debug("Waiting mutex release before process start.");
                Shortcuts.Handler handler = Shortcuts.Handler.getInstance();
                if (handler != null)
                    handler.waitModifiersReleased();

                log.Info("Starting process: " + processData.StartPath);

                Process process = new Process();
                process.StartInfo.FileName = processData.StartPath;
                process.StartInfo.Arguments = String.Join<string>(" ", processData.StartArguments);
                process.StartInfo.WorkingDirectory = processData.StartFolder;
                process.StartInfo.UseShellExecute = processData.Shell;

                process.Start();
            }

            /// <summary>
            ///     Find the processes associated with an executable path.
            /// </summary>
            /// <param name="exePath">A (apparent) path to an executable</param>
            /// <returns>The list of matching processes (may be empty if no process matches)</returns>
            static private List<Process> findProcesses(string exePath)
            {
                Process[] processes = Process.GetProcesses();
                List<Process> processesMatch = new List<Process>();

                foreach (Process p in processes) {
                    try {
                        if (p.Modules[0].FileName == exePath)
                            processesMatch.Add(p);
                    } catch (System.ComponentModel.Win32Exception) { // In case the user has not enough rights to read the process information
                    } catch (System.InvalidOperationException) { // In case the process has exited between GetProcesses() and access to Modules[0] 
                    }
                }

                return processesMatch;
            }

            /// <summary>
            ///     Find windows for a thread collection.
            /// </summary>
            /// <para>
            ///     Only the windows matching the given title are returned.
            /// </para>
            /// <param name="threads">A collection of threads</param>
            /// <param name="title">A regexp to select windows according to their title</param>
            /// <returns></returns>
            static private List<IntPtr> findVisibleWindows(ProcessThreadCollection threads, string title)
            {
                List<IntPtr> ret = new List<IntPtr>();

                foreach (ProcessThread t in threads)
                    User32.EnumThreadWindows(t.Id, (IntPtr winHandle, IntPtr param) =>
                    {
                        if (!User32.IsWindowVisible(winHandle))
                            return true;

                        // Gets the window title:
                        int len1 = User32.GetWindowTextLength(winHandle);
                        if (len1 == 0)
                            throw new ApplicationException("Could not get window text length. Error code: " + User32.GetLastError());
                        StringBuilder buffer = new StringBuilder(len1 + 1);
                        int len2 = User32.GetWindowText(winHandle, buffer, len1 + 1);
                        if (len2 == 0)
                            throw new ApplicationException("Could not get window text. Error code: " + User32.GetLastError());
                        if (len1 != len2)
                            throw new ApplicationException("Lengths should match (" + len1 + " != " + len2 + ")");

                        // Check if it matches:
                        Regex regexp = new Regex(title);
                        if (regexp.IsMatch(buffer.ToString()))
                            ret.Add(winHandle);

                        return true;
                    }, IntPtr.Zero);

                return ret;
            }

            /// <summary>
            ///     List the titles of visible windows to <c>Console</c>.
            /// </summary>
            /// <remarks>This function is used to write configuration files.</remarks>
            /// <param name="threads">A collection of threads</param>
            static private void listVisibleWindowsTitle(ProcessThreadCollection threads)
            {
                foreach (ProcessThread t in threads)
                    User32.EnumThreadWindows(t.Id, (IntPtr winHandle, IntPtr param) =>
                    {
                        if (!User32.IsWindowVisible(winHandle))
                            return true;

                        // Gets the window title:
                        int len1 = User32.GetWindowTextLength(winHandle);
                        if (len1 == 0)
                            throw new ApplicationException("Could not get window text length. Error code: " + User32.GetLastError());
                        StringBuilder buffer = new StringBuilder(len1 + 1);
                        int len2 = User32.GetWindowText(winHandle, buffer, len1 + 1);
                        if (len2 == 0)
                            throw new ApplicationException("Could not get window text. Error code: " + User32.GetLastError());
                        if (len1 != len2)
                            throw new ApplicationException("Lengths should match (" + len1 + " != " + len2 + ")");

                        Console.WriteLine(buffer);

                        return true;
                    }, IntPtr.Zero);
            }
        }
    }
}
