using System;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class InavalidShortcutException : ApplicationException
        {
            public ShortcutData InvalidShortcut {get; set;}

            public InavalidShortcutException() :
                base() { }
            public InavalidShortcutException(string msg) :
                base(msg) { }
            public InavalidShortcutException(string msg, Exception e) :
                base(msg, e) { }
            public InavalidShortcutException(ShortcutData shortcut) :
                base()
            {
                InvalidShortcut = shortcut;
            }
            public InavalidShortcutException(string msg, ShortcutData shortcut) :
                base(msg)
            {
                InvalidShortcut = shortcut;
            }
            public InavalidShortcutException(string msg, ShortcutData shortcut, Exception e) :
                base(msg, e)
            {
                InvalidShortcut = shortcut;
            }
        }
    }
}
