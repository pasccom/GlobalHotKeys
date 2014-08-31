using System;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class InavalidShortcutException : ApplicationException
        {
            public Shortcut InvalidShortcut {get; set;}

            public InavalidShortcutException() :
                base() { }
            public InavalidShortcutException(string msg) :
                base(msg) { }
            public InavalidShortcutException(string msg, Exception e) :
                base(msg, e) { }
            public InavalidShortcutException(Shortcut shortcut) :
                base()
            {
                InvalidShortcut = shortcut;
            }
            public InavalidShortcutException(string msg, Shortcut shortcut) :
                base(msg)
            {
                InvalidShortcut = shortcut;
            }
            public InavalidShortcutException(string msg, Shortcut shortcut, Exception e) :
                base(msg, e)
            {
                InvalidShortcut = shortcut;
            }
        }
    }
}
