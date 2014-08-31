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

            static public void activate(string exePath, string title = null)
            {
                Console.WriteLine("Called Windows.Manager.activate(\"{0}\", \"{1}\")", exePath, title);
            }

            static public void start(string exePath)
            {
                Console.WriteLine("Called Windows.Manager.activate(\"{0}\")", exePath);
            }
        }
    }
}
