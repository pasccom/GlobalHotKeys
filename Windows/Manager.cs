﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace GlobalHotKeys
{
    namespace Windows
    {
        class Manager
        {
            static private ProcessesProvider mProcessessList;

            static private readonly ILog log = LogManager.GetLogger(typeof(Manager));
            static public ProcessesProvider ProcessessList
            {
                set
                {
                    if (value != null)
                        value.parseConfig();
                    mProcessessList = value;
                }
            }

            static public List<string> AuthorizedMethods
            {
                get
                {
                    return new List<string>() { "activate", "start", "focus" };
                }
            }

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

            static public void start(List<string> args)
            {
                if (args.Count != 1)
                    throw new Shortcuts.BadArgumentCountException("start(processName) needs 1 argument.", 1, 1);

                string processName = args[0];

                log.InfoFormat("Called Windows.Manager.start(\"{0}\")", processName);

                startProcess(mProcessessList[processName]);
            }

            static private User32.ShowState parseSize(String sizeName)
            {
                bool bad = sizeName.Contains(",");    
                
                try {
                    int.Parse(sizeName);
                    bad = true;
                } catch (Exception) {}

                if (bad)
                    throw new ArgumentException("The size argument should not contain commas or be a number.");

                try {
                    return (User32.ShowState)Enum.Parse(typeof(User32.ShowState), sizeName);
                } catch (ArgumentException) {
                    throw new ArgumentException("The size argument should be either \"Default\", \"Normal\", \"Maximize\" or \"ShowMaximized\"");
                }
            }

            static private bool isMiniMized(IntPtr winHandle)
            {
                User32.Rect rect = new User32.Rect();
                if (!User32.GetWindowRect(winHandle, out rect))
                    throw new ApplicationException("Method failed"); // TODO improve it!

                return ((rect.Top == -32000) && (rect.Left == -32000));
            }

            static private void activate(IntPtr winHandle, User32.ShowState sizeFlag)
            {
                switch(sizeFlag) {
                case User32.ShowState.Restore:
                    if (isMiniMized(winHandle))
                        User32.ShowWindow(winHandle, (int) User32.ShowState.Normal);
                    User32.SetForegroundWindow(winHandle);
                    break;
                case User32.ShowState.Maximize:
                    if (isMiniMized(winHandle))
                        User32.ShowWindow(winHandle, (int) User32.ShowState.ShowMaximized);
                    User32.SetForegroundWindow(winHandle);
                    break;
                default:
                    User32.ShowWindow(winHandle, (int) sizeFlag);
                    User32.SetForegroundWindow(winHandle);
                    break;
                }
            }

            static private void startProcess(ProcessData processData)
            {
                log.Info("Starting process: " + processData.ExePath);

                Process process = new Process();
                process.StartInfo.FileName = processData.ExePath;
                process.StartInfo.Arguments = String.Join<string>(" ", processData.StartArguments);
                process.StartInfo.WorkingDirectory = processData.StartFolder;
                process.StartInfo.UseShellExecute = processData.Shell;

                process.Start();
            }

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
                        StringBuilder buffer = new StringBuilder(len1 + 1);
                        int len2 = User32.GetWindowText(winHandle, buffer, len1 + 1);
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

            static private void listVisibleWindowsTitle(ProcessThreadCollection threads, string title)
            {
                foreach (ProcessThread t in threads)
                    User32.EnumThreadWindows(t.Id, (IntPtr winHandle, IntPtr param) =>
                    {
                        if (!User32.IsWindowVisible(winHandle))
                            return true;

                        // Gets the window title:
                        int len1 = User32.GetWindowTextLength(winHandle);
                        StringBuilder buffer = new StringBuilder(len1 + 1);
                        int len2 = User32.GetWindowText(winHandle, buffer, len1 + 1);
                        if (len1 != len2)
                            throw new ApplicationException("Lengths should match (" + len1 + " != " + len2 + ")"); 

                        Console.WriteLine(buffer);

                        return true;
                    }, IntPtr.Zero);
            }
        }
    }
}
