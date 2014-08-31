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

            static public void shutdown(string when = null)
            {
                Console.WriteLine("Called Power.Manager.shutdown(\"{0}\")", when);
            }

            static public void reboot(string when = null)
            {
                Console.WriteLine("Called Power.Manager.reboot(\"{0}\")", when);
            }

            static public void logOff()
            {
                Console.WriteLine("Called Power.Manager.logOff()");
            }

            static public void lockScreen()
            {
                Console.WriteLine("Called Power.Manager.lockScreen()");
            }
        }
    }
}
