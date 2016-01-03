using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace GlobalHotKeys
{
    namespace Power
    {
        /// <summary>
        ///     Defines the methods of Power namespace.
        /// </summary>
        /// <para>
        ///     This is the main class of Power namespace, it defines the methods available from this namespace. They are
        ///     <list type="bullet">
        ///         <item><term><see cref="shutdown"/>: </term><description>Shut computer down 
        ///             <list type="bullet">
        ///                 <item><term>Delay (opt): </term><description>Delay before effective shutdown (1 min, if unspecified)</description></item>
        ///             </list>
        ///         </description></item>
        ///         <item><term><see cref="reboot"/>: </term><description>Reboot the computer
        ///             <list type="bullet">
        ///                 <item><term>Delay (opt): </term><description>Delay before effective shutdown (1 min, if unspecified)</description></item>
        ///             </list>
        ///         </description></item>
        ///         <item><term><see cref="logOff"/>: </term><description>Log off from the current session. It takes no arguments</description></item>
        ///         <item><term><see cref="lockScreen"/>: </term><description>Lock the computer. It takes no arguments</description></item>
        ///     </list>
        /// </para>
        class Manager
        {
            /// <summary>
            ///     States of the computer
            /// </summary>
            private enum State {
                /// <summary>Computer is normally running</summary>
                Running,
                /// <summary>Computer is waiting before shuting down</summary>
                ShuttingDownn,
                /// <summary>Computer is waiting before rebooting</summary>
                Rebooting
            };
            
            /// <summary>
            ///     Logger for GlobalHokKeys.
            /// </summary>
            /// <remarks>See Apache Log4net documentation for the logging interface.</remarks>
            static private readonly ILog log = LogManager.GetLogger(typeof(Manager));
            /// <summary>
            ///     Current state of the computer
            /// </summary>
            static private State mState = State.Running;
            /// <summary>
            ///     List of declared methods.
            /// </summary>
            /// <remarks>Only these methods should be called externally.</remarks>
            static public List<string> AuthorizedMethods
            {
                get
                {
                    return new List<string>() { "shutdown", "reboot", "logOff", "lockScreen" };
                }
            }

            /// <summary>
            ///     Shut the computer down or cancel a previous shutdown, depending on the current <see cref="State"/>.
            /// </summary>
            /// <para>
            ///     Arguments
            ///     <list type="bullet">
            ///         <item><term>Delay (opt): </term><description>Delay before effective shutdown (1 min, if unspecified)</description></item>
            ///     </list>
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            /// <seealso cref="reboot"/>
            static public void shutdown(List<string> args)
            {
                if (args.Count > 1)
                    throw new Shortcuts.BadArgumentCountException("shutdown([when]) admits 1 optional argument", 0, 1);
                
                int when = 60;
                if (args.Count == 1)
                    when = int.Parse(args[0]);

                log.InfoFormat("Called Power.Manager.shutdown({0})", when);

                switch (mState) {
                case State.Running:
                    log.Debug("Shutting down.");
                    Process.Start("shutdown", "/s /t " + when);
                    mState = State.ShuttingDownn;
                    break;
                case State.ShuttingDownn:
                    log.Debug("Cancelling shutdown.");
                    Process.Start("shutdown", "/a");
                    mState = State.Running;
                    break;
                case State.Rebooting:
                    log.Debug("Cancelling rebbot and shutting down.");
                    Process.Start("shutdown", "/a");
                    Process.Start("shutdown", "/s /t " + when);
                    mState = State.ShuttingDownn;
                    break;
                }
            }

            /// <summary>
            ///     Reboot the computer or cancel a previous reboot, depending on the current <see cref="State"/>.
            /// </summary>
            /// <para>
            ///     Arguments
            ///     <list type="bullet">
            ///         <item><term>Delay (opt): </term><description>Delay before effective shutdown (1 min, if unspecified)</description></item>
            ///     </list>
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            /// <seealso cref="shutdown"/>
            static public void reboot(List<string> args)
            {
                if (args.Count > 1)
                    throw new Shortcuts.BadArgumentCountException("reboot([when]) admits 1 optional argument", 0, 1);

                int when = 60;
                if (args.Count == 1)
                    when = int.Parse(args[0]);

                log.InfoFormat("Called Power.Manager.reboot({0})", when);

                Process.Start("shutdown", "/r /t " + when);

                switch (mState) {
                case State.Running:
                    log.Debug("Rebooting.");
                    Process.Start("shutdown", "/r /t " + when);
                    mState = State.Rebooting;
                    break;
                case State.Rebooting:
                    log.Debug("Cancelling reboot.");
                    Process.Start("shutdown", "/a");;
                    mState = State.Running;
                    break;
                case State.ShuttingDownn:
                    log.Debug("Cancelling shutdown and rebooting.");
                    Process.Start("shutdown", "/a");
                    Process.Start("shutdown", "/r /t " + when);;
                    mState = State.Rebooting;
                    break;
                }
            }

            /// <summary>
            ///     Log off from the current session.
            /// </summary>
            /// <para>
            ///     This method does not take any arguments.
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            static public void logOff(List<string> args)
            {
                if (args.Count != 0)
                    throw new Shortcuts.BadArgumentCountException("logOff() admits no argument", 0);

                log.InfoFormat("Called Power.Manager.logOff()");

                Process.Start("shutdown", "/l");
            }

            /// <summary>
            ///     Lock the computer.
            /// </summary>
            /// <para>
            ///     This method does not take any arguments.
            /// </para>
            /// <param name="args">Arguments to the function (see above)</param>
            static public void lockScreen(List<string> args)
            {
                if (args.Count != 0)
                    throw new Shortcuts.BadArgumentCountException("lockScreen() admits no arguments", 0);

                log.InfoFormat("Called Power.Manager.lockScreen()");

                log.Debug("Waiting mutex release before screen lock.");
                Shortcuts.Handler handler = Shortcuts.Handler.getInstance();
                if (handler != null)
                    handler.waitModifiersReleased();
                if (!User32.LockWorkStation())
                    throw new ApplicationException("Failed to lock the screen. Error code: " + User32.GetLastError());
            }
        }
    }
}
