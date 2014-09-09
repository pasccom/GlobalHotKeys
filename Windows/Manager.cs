﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;

namespace GlobalHotKeys
{
    namespace Windows
    {
        class Manager
        {
            static public List<string> AuthorizedMethods
            {
                get
                {
                    return new List<string>() { "activate", "start", "focus" };
                }
            }

            static public void activate(List<string> args)
            {
                if ((args.Count < 1) || (args.Count > 3))
                    throw new Shortcuts.BadArgumentCountException("activate(exePath, [title, [startPath]]) needs 1 argument at least and admits 2 optional arguments.", 1, 3);

                string exePath = args[0];
                string title = null;
                string startPath = null;
                if (args.Count >= 2)
                    title = args[1];
                if (args.Count >= 3)
                    startPath = args[2];

                Console.WriteLine("Called Windows.Manager.activate(\"{0}\", \"{1}\", \"{2}\")", exePath, title, startPath);

                List<Process> processes = findProcesses(exePath);
                if (processes.Count == 0) {
                    // Start process
                    startProcess(exePath, startPath);
                } else {
                    if (title == null) {
                        // Use the .NET provided MainWindowHandle (not always good...)
                        activate(processes[0].MainWindowHandle);
                    } else {
                        // Look for a visible window whose title matches the provided regexp
                        List<IntPtr> windows = findVisibleWindows(processes[0].Threads, title);
                        if (windows.Count == 0)
                            Console.WriteLine("Warning: No matching windows", windows.Count - 1);
                        else
                            activate(windows[0]);

                        if (windows.Count > 1)
                            Console.WriteLine("Warning: {0} matching windows ignored", windows.Count - 1);
                    }
                }

                if (processes.Count > 1)
                    Console.WriteLine("Warning: {0} matching processes ignored", processes.Count - 1);
            }

            static public void focus(List<string> args)
            {
                if ((args.Count < 1) || (args.Count > 3))
                    throw new Shortcuts.BadArgumentCountException("focus(exePath, [title]) needs 1 argument at least and admits 1 optional argument.", 1, 2);

                string exePath = args[0];
                string title = null;
                if (args.Count == 2)
                    title = args[1];

                Console.WriteLine("Called Windows.Manager.focus(\"{0}\", \"{1}\")", exePath, title);

                List<Process> processes = findProcesses(exePath);
                if (processes.Count == 0) {
                    Console.WriteLine("Warning: No matching process");
                } else {
                    if (title == null) {
                        // Use the .NET provided MainWindowHandle (not always good...)
                        activate(processes[0].MainWindowHandle);
                    } else {
                        // Look for a visible window whose title matches the provided regexp
                        List<IntPtr> windows = findVisibleWindows(processes[0].Threads, title);
                        if (windows.Count == 0)
                            Console.WriteLine("Warning: No matching windows", windows.Count - 1);
                        else
                            activate(windows[0]);

                        if (windows.Count > 1)
                            Console.WriteLine("Warning: {0} matching windows ignored", windows.Count - 1);
                    }
                }

                if (processes.Count > 1)
                    Console.WriteLine("Warning: {0} matching processes ignored", processes.Count - 1);
            }

            static public void start(List<string> args)
            {
                if (args.Count != 1)
                    throw new Shortcuts.BadArgumentCountException("start(exePath, [startPath]) needs 1 argument and admits 1 optional argument.", 1, 2);

                string exePath = args[0];
                string startPath = null;
                if (args.Count >= 2)
                    startPath = args[1];

                Console.WriteLine("Called Windows.Manager.start(\"{0}\")", exePath);

                startProcess(exePath, startPath);
            }

            static private void activate(IntPtr winHandle)
            {
                User32.ShowWindow(winHandle, User32.SW_SHOWDEFAULT);
                User32.SetForegroundWindow(winHandle);
            }

            static private void startProcess(string exePath, string startPath = null)
            {
                Console.WriteLine("Starting " + exePath);

                Process process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.WorkingDirectory = startPath;
                process.StartInfo.UseShellExecute = false;

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
                    } catch (System.ComponentModel.Win32Exception) { }
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