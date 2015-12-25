using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace GlobalHotKeys
{
    /// <summary>
    ///     Main class of GlobalHotKeys
    /// </summary>
    /// <para>Handles argument parsing and initialisation of the program.</para>
    /// <para>
    ///     GlobalHotKeys invocation:
    ///     <code>GlobalHotKeys.exe [(--quiet|--debug)] [configPath] [processesPath]</code>
    /// </para>
    /// <list>
    ///     <item><term><c>--quiet</c>: </term><description>Quietly run (no file logging)</description></item>
    ///     <item><term><c>--debug</c>: </term><description>Run ind debug mode (log level set to debug)</description></item>
    ///     <item><term>configPath: </term><description>The path to the file where the shortcuts are defined.</description></item>
    ///     <item><term>processesPath: </term><description>The path to the file where process info is listed. Only these processes can be started by GlobalHotKeys. This file should not be writable by normal users.</description></item>
    /// </list>
    /// <para>
    ///     Actually, only plain text configuration is supported by GlobalHotKeys.
    ///     To see the format of such files <see cref="PlainTextConfig"/> and look at the example files provided in the project root directory.
    ///     I have in project to develop registry based configuration as well as XML configuration files.
    /// </para>
    class Program
    {
        /// <summary>
        ///     Logger for GlobalHokKeys.
        /// </summary>
        /// <remarks>See Apache Log4net documentation for the logging interface.</remarks>
        static private ILog log;

        /// <summary>
        ///     Effectively start the program
        /// </summary>
        /// <para>
        ///     This function handles the command line arguments containing the config path and the processes path.
        ///     It then initialies the component with the suitable configuration.
        /// </para>
        /// <param name="argStart">Index pointing after the last argument read by <see cref="Main"/>.</param>
        /// <param name="args">Command line arguments.</param>
        static void Start(int argStart, string[] args)
        {
            string configPath;
            string processesPath;

            if (args.Length > argStart)
                configPath = args[argStart];
            else
                configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Pascom\GlobalHotKeys\globalhotkeys.conf";

            if (args.Length > argStart + 1)
                processesPath = args[argStart + 1];
            else
                processesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Pascom\GlobalHotKeys\globalhotkeys.processes.conf";

            log.Info("Starting GlobalHotKeys with configuration file :" + configPath);
            log.Info("Starting GlobalHotKeys with process list file :" + processesPath);

            Windows.Manager.ProcessessList = new Windows.PlainTextConfig(processesPath);
            Shortcuts.Handler app = Shortcuts.Handler.getInstance(new PlainTextConfig(configPath));
            app.exec();
        }

        /// <summary>
        ///     Set log level to Debug
        /// </summary>
        /// <para>
        ///     This function handles the command line argument <c>--debug</c>.
        ///     It sets the log level of all loggers to debug.
        /// </para>
        private static void setDebug()
        {
            Hierarchy repo = LogManager.GetRepository() as Hierarchy;
            if (repo == null)
                return;

            repo.Root.Level = Level.Debug;

            foreach (ILogger iLogger in repo.GetCurrentLoggers()) {
                Logger logger = iLogger as Logger;
                if (logger != null)
                    logger.Level = Level.Debug;
            }
        }

        /// <summary>
        ///     Removes all appenders but Windows Event Log
        /// </summary>
        /// <param name="logger">The logger from which appenders will be removed</param>
        /// <seealso cref="setQuiet"/>
        private static void removeFileAppenders(Logger logger)
        {
            if (logger == null)
                return;

            AppenderCollection appenders = logger.Appenders;
            for (int i = 0; i < appenders.Count; i++)
                if (appenders[i].Name != "WindowsEventLog")
                    logger.RemoveAppender(appenders[i]);
        }

        /// <summary>
        ///     Removes file logging
        /// </summary>
        /// <para>
        ///     This function handles the command line argument <c>--quiet</c>.
        ///     It removes all file logging.
        /// </para>
        private static void setQuiet()
        {
            Hierarchy repo = LogManager.GetRepository() as Hierarchy;
            if (repo == null)
                return;

            removeFileAppenders(repo.Root);
            foreach (ILogger iLogger in LogManager.GetRepository().GetCurrentLoggers())
                removeFileAppenders(iLogger as Logger);
        }

        /// <summary>
        ///     Program main function
        /// </summary>
        /// <para>
        ///     Handles the processiong of <c>--debug</c> and <c>--quiet</c> arguments.
        ///     Also wrap the core of the program in an exception handler.
        /// </para>
        /// <param name="args">Command line arguments.</param>
        /// <seealso cref="Start"/>
        static void Main(string[] args)
        {
            int processed = 0;
            bool debuging = false;
            bool quiet = false;

            while ((processed < args.Length) && args[processed].StartsWith("--")) {
                switch (args[processed]) {
                case "--quiet":
                    quiet = true;
                    break;
                case "--debug":
                    debuging = true;
                    break;
                default:
                    log.Warn("Unknown argument: " + args[processed]);
                    break;
                }
                processed++;
            }

#if DEBUG
            log4net.GlobalContext.Properties["LogFilePath"] = System.IO.Path.GetTempPath() + @"\Pascom\GlobalHotKeys\Debug";
            debuging = true;
#else
            log4net.GlobalContext.Properties["LogFilePath"] = System.IO.Path.GetTempPath() + @"\Pascom\GlobalHotKeys\Release";
#endif
            log4net.Config.XmlConfigurator.Configure();

            if (quiet)
                setQuiet();
            if (debuging)
                setDebug();
            log = LogManager.GetLogger(typeof(Program));

            try {
                Start(processed, args);
            } catch (Exception e) {
                log.Fatal("Unhandled exception caught by main handler", e);
            }
        }
    }
}
