using System;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class UnauthorizedShortcutException : ApplicationException
        {
            public Shortcut UnauthorizedShortcut { get; set; }

            public UnauthorizedShortcutException() :
                base() { }
            public UnauthorizedShortcutException(string msg) :
                base(msg) { }
            public UnauthorizedShortcutException(string msg, Exception e) :
                base(msg, e) { }
            public UnauthorizedShortcutException(Shortcut shortcut) :
                base()
            {
                UnauthorizedShortcut = shortcut;
            }
            public UnauthorizedShortcutException(string msg, Shortcut shortcut) :
                base(msg)
            {
                UnauthorizedShortcut = shortcut;
            }
            public UnauthorizedShortcutException(string msg, Shortcut shortcut, Exception e) :
                base(msg, e)
            {
                UnauthorizedShortcut = shortcut;
            }
        }
    }
}

