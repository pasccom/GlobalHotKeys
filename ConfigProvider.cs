using System;

namespace GlobalHotKeys
{
    /// <summary>
    ///     Abstract base for classes providing a configuration for GlobalHotKeys
    /// </summary>
    /// <para>
    ///     It defines a new event <see cref="NewShortcutEvent"/>, which is triggered 
    ///     when a new shortcut is read, and the associated delegate <see cref="ShortcutHandler"/>.
    /// </para>
    abstract class ConfigProvider
    {
        /// <summary>
        ///     Delegate for new shortcut handling.
        /// </summary>
        /// <para>
        ///     Register you handler here. It will be triggered when a new shortcut is read.
        /// </para>
        /// <param name="shortcut">The shortcut which trigered the event.</param>
        /// <seealso cref="NewShortcutEvent"/>
        public delegate void ShortcutHandler(ShortcutData shortcut);

        /// <summary>
        ///     Event triggered when a new shortcut is read.
        /// </summary>
        /// <para>
        ///     When this event is triggered, the handlers registered in the delegates are called.
        /// </para>
        /// <seealso cref="ShortcutHandler"/>
        public event ShortcutHandler NewShortcutEvent;

        /// \internal
        /// <summary>
        ///     This function should be called when a new shortcut is read.
        /// </summary>
        /// <remarks>It just triggers the <see cref="NewShortcutEvent"/>.</remarks>
        /// <param name="shortcut">The new shortcut</param>
        protected void newShortcut(ShortcutData shortcut)
        {
            NewShortcutEvent(shortcut);
        }

        /// <summary>
        ///     Config file parsing.
        /// </summary>
        /// <para>This function is in charge of the parsing of the config file.</para>
        public abstract void parseConfig();
    }
}
