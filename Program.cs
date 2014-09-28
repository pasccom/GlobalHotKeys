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
    class Program
    {
        static private ILog log;

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
                processesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Pascom\GlobalHotKeys\globalhotkeys.processes.conf";

            log.Info("Starting GlobalHotKeys with configuration file :" + configPath);
            log.Info("Starting GlobalHotKeys with process list file :" + processesPath);

            Windows.Manager.ProcessessList = new Windows.PlainTextConfig(processesPath);
            Shortcuts.Handler app = Shortcuts.Handler.getInstance(new PlainTextConfig(configPath));
            app.exec();
        }

        private static void setQuiet()
        {
            Hierarchy repo = LogManager.GetRepository() as Hierarchy;
            if (repo == null)
                return;

            repo.Root.RemoveAppender("Console");

            foreach (ILogger iLogger in repo.GetCurrentLoggers()) {
                Logger logger = iLogger as Logger;
                if (logger != null)
                    logger.RemoveAppender("Console");
            }
        }

        private static void removeFileAppenders(Logger logger)
        {
            if (logger == null)
                return;

            AppenderCollection appenders = logger.Appenders;
            for (int i = 0; i < appenders.Count; i++)
                if ((appenders[i].Name != "WindowsEventLog") && (appenders[i].Name != "Console"))
                    logger.RemoveAppender(appenders[i]);
        }

        private static void setRelease()
        {
            Hierarchy repo = LogManager.GetRepository() as Hierarchy;
            if (repo == null)
                return;

            removeFileAppenders(repo.Root);
            foreach (ILogger iLogger in LogManager.GetRepository().GetCurrentLoggers())
                removeFileAppenders(iLogger as Logger);
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

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

            if (quiet)
                setQuiet();
            if (!debuging)
                setRelease();
            log = LogManager.GetLogger(typeof(Program));

            try {
                Start(processed, args);
            } catch (Exception e) {
                log.Fatal("Unhandled exception caught by main handler", e);
            }
        }
    }
}
