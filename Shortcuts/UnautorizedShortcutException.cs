using System;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        /// <summary>
        ///     Exception thrown when an unauthorized <see cref="ShortcutData"/> is used.
        /// </summary>
        /// <para>
        ///     It stores the <see cref="UnauthorizedShortcut"/> in a property.
        /// </para>
        class UnauthorizedShortcutException : ApplicationException
        {
            /// <summary>
            ///     The unauthorized shortcut.
            /// </summary>
            public ShortcutData UnauthorizedShortcut { get; set; }

            /// <summary>
            ///     Default constructor.
            /// </summary>
            /// <remarks>The <see cref="UnauthorizedShortcut"/> is not set.</remarks>
            public UnauthorizedShortcutException() :
                base() { }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <remarks>The <see cref="UnauthorizedShortcut"/> is not set.</remarks>
            public UnauthorizedShortcutException(string msg) :
                base(msg) { }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="e">Parent exception</param>
            /// <remarks>The <see cref="UnauthorizedShortcut"/> is not set.</remarks>
            public UnauthorizedShortcutException(string msg, Exception e) :
                base(msg, e) { }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="shortcut">The <see cref="UnauthorizedShortcut"/></param>
            public UnauthorizedShortcutException(ShortcutData shortcut) :
                base()
            {
                UnauthorizedShortcut = shortcut;
            }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="shortcut">The <see cref="UnauthorizedShortcut"/></param>
            public UnauthorizedShortcutException(string msg, ShortcutData shortcut) :
                base(msg)
            {
                UnauthorizedShortcut = shortcut;
            }
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="shortcut">The <see cref="UnauthorizedShortcut"/></param>
            /// <param name="e">Parent exception</param>
            public UnauthorizedShortcutException(string msg, ShortcutData shortcut, Exception e) :
                base(msg, e)
            {
                UnauthorizedShortcut = shortcut;
            }
        }
    }
}

