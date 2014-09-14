using System;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class UnauthorizedShortcutException : ApplicationException
        {
            public ShortcutData UnauthorizedShortcut { get; set; }

            public UnauthorizedShortcutException() :
                base() { }
            public UnauthorizedShortcutException(string msg) :
                base(msg) { }
            public UnauthorizedShortcutException(string msg, Exception e) :
                base(msg, e) { }
            public UnauthorizedShortcutException(ShortcutData shortcut) :
                base()
            {
                UnauthorizedShortcut = shortcut;
            }
            public UnauthorizedShortcutException(string msg, ShortcutData shortcut) :
                base(msg)
            {
                UnauthorizedShortcut = shortcut;
            }
            public UnauthorizedShortcutException(string msg, ShortcutData shortcut, Exception e) :
                base(msg, e)
            {
                UnauthorizedShortcut = shortcut;
            }
        }
    }
}

