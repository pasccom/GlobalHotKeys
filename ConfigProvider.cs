using System;

namespace GlobalHotKeys
{
    abstract class ConfigProvider
    {
        public delegate void ShortcutHandler(ShortcutData shortcut);
        public event ShortcutHandler NewShortcutEvent;

        protected void newShortcut(ShortcutData shortcut)
        {
            NewShortcutEvent(shortcut);
        }

        public abstract void parseConfig();
    }
}
