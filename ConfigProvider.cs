using System;

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
