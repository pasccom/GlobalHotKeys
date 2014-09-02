using System;
using System.Collections.Generic;

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
                    return new List<string>() { "activate", "start" };
                }
            }

            static public void activate(string [] args)
            {
                if ((args.Length < 1) || (args.Length > 2))
                    throw new Shortcuts.BadArgumentCountException("activate(when) admits 1 optional argument", 1, 2);

                string exePath = args[0];
                string title = null;
                if (args.Length == 2)
                    title = args[1];

                Console.WriteLine("Called Windows.Manager.activate(\"{0}\", \"{1}\")", exePath, title);
            }

            static public void start(string [] args)
            {
                if (args.Length != 1)
                    throw new Shortcuts.BadArgumentCountException("start(when) needs 1 argument", 1, 1);

                string exePath = args[0];

                Console.WriteLine("Called Windows.Manager.activate(\"{0}\")", exePath);
            }
        }
    }
}
