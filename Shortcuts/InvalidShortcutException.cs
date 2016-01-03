using System;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        /// <summary>
        ///     Exception thrown when an invalid <see cref="ShortcutData"/> is used.
        /// </summary>
        /// <para>
        ///     It stores the <see cref="InvalidShortcut"/> in a property.
        /// </para>
        class InvalidShortcutException : ApplicationException
        {
            /// <summary>
            ///     The invalid shortcut.
            /// </summary>
            public ShortcutData InvalidShortcut {get; set;}

            /// <summary>
            ///     Default constructor
            /// </summary>
            /// <remarks>The <see cref="InvalidShortcut"/> is not set.</remarks>
            public InvalidShortcutException() :
                base() { }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <remarks>The <see cref="InvalidShortcut"/> is not set.</remarks>
            public InvalidShortcutException(string msg) :
                base(msg) { }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="e">Parent exception</param>
            /// <remarks>The <see cref="InvalidShortcut"/> is not set.</remarks>
            public InvalidShortcutException(string msg, Exception e) :
                base(msg, e) { }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="shortcut">The <see cref="InvalidShortcut"/></param>
            public InvalidShortcutException(ShortcutData shortcut) :
                base()
            {
                InvalidShortcut = shortcut;
            }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="shortcut">The <see cref="InvalidShortcut"/></param>
            public InvalidShortcutException(string msg, ShortcutData shortcut) :
                base(msg)
            {
                InvalidShortcut = shortcut;
            }
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="msg">Exception message</param>
            /// <param name="shortcut">The <see cref="InvalidShortcut"/></param>
            /// <param name="e">Parent exception</param>
            public InvalidShortcutException(string msg, ShortcutData shortcut, Exception e) :
                base(msg, e)
            {
                InvalidShortcut = shortcut;
            }
        }
    }
}
