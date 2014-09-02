using System;
using System.Collections.Generic;

namespace GlobalHotKeys
{
    namespace Power
    {
        class Manager
        {
            static public List<string> AuthorizedMethods
            {
                get
                {
                    return new List<string>() { "shutdown", "reboot", "logOff", "lockScreen" };
                }
            }

            static public void shutdown(string [] args)
            {
                if (args.Length > 1)
                    throw new Shortcuts.BadArgumentCountException("shutdown(when) admits 1 optional argument", 0, 1);
                
                int? when = null;
                if (args.Length == 1)
                    when = int.Parse(args[0]);

                Console.WriteLine("Called Power.Manager.shutdown(\"{0}\")", when);
            }

            static public void reboot(string[] args)
            {
                if (args.Length > 1)
                    throw new Shortcuts.BadArgumentCountException("reboot(when) admits 1 optional argument", 0, 1);

                int? when = null;
                if (args.Length == 1)
                    when = int.Parse(args[0]);

                Console.WriteLine("Called Power.Manager.reboot(\"{0}\")", when);
            }

            static public void logOff(string[] args)
            {
                if (args.Length > 1)
                    throw new Shortcuts.BadArgumentCountException("logOff() admits no arguments", 0);

                Console.WriteLine("Called Power.Manager.logOff()");
            }

            static public void lockScreen(string[] args)
            {
                if (args.Length > 1)
                    throw new Shortcuts.BadArgumentCountException("lockScreen() admits no arguments", 0);

                Console.WriteLine("Called Power.Manager.lockScreen()");
            }
        }
    }
}
