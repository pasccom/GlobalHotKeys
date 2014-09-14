using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace GlobalHotKeys
{
    class Program
    {
        static private readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Start(string[] args)
        {
            string configPath;
            string processesPath;

            if (args.Length > 1)
                configPath = args[0];
            else
                configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Pascom\GlobalHotKeys\globalhotkeys.conf";

            if (args.Length > 2)
                processesPath = args[1];
            else
                processesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Pascom\GlobalHotKeys\globalhotkeys.processes.conf";

            log.Info("Starting GlobalHotKeys with configuration file :" + configPath);
            log.Info("Starting GlobalHotKeys with process list file :" + processesPath);

            Windows.Manager.ProcessessList = new Windows.PlainTextConfig(processesPath);
            Shortcuts.Handler app = Shortcuts.Handler.getInstance(new PlainTextConfig(configPath));
            app.exec();
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            try {
                Start(args);
            } catch (Exception e) {
                log.Fatal("Unhandled exception caught by main handler", e);
            }
        }
    }
}
