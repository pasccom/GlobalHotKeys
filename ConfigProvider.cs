using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalHotKeys
{
    abstract class ConfigProvider
    {
        public delegate void ShortcutHandler(Shortcut shortcut);
        public event ShortcutHandler NewShortcutEvent;

        protected void newShortcut(Shortcut shortcut)
        {
            NewShortcutEvent(shortcut);
        }

        public abstract void parseConfig();
    }
}
