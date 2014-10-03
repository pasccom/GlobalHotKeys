using System;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class InvalidShortcutException : ApplicationException
        {
            public ShortcutData InvalidShortcut {get; set;}

            public InvalidShortcutException() :
                base() { }
            public InvalidShortcutException(string msg) :
                base(msg) { }
            public InvalidShortcutException(string msg, Exception e) :
                base(msg, e) { }
            public InvalidShortcutException(ShortcutData shortcut) :
                base()
            {
                InvalidShortcut = shortcut;
            }
            public InvalidShortcutException(string msg, ShortcutData shortcut) :
                base(msg)
            {
                InvalidShortcut = shortcut;
            }
            public InvalidShortcutException(string msg, ShortcutData shortcut, Exception e) :
                base(msg, e)
            {
                InvalidShortcut = shortcut;
            }
        }
    }
}
