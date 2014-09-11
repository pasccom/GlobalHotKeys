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
            string path;
            if (args.Length > 1)
                path = args[0];
            else
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Pascom\GlobalHotKeys\globalhotkeys.conf";

            log.Info("Starting GlobalHotKeys with configuration file :" + path);

            Shortcuts.Handler app = Shortcuts.Handler.getInstance(new PlainTextConfig(path));
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
