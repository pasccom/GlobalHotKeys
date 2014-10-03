using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace GlobalHotKeys
{
    namespace Power
    {
        class Manager
        {
            private enum State {Running, ShuttingDownn, Rebooting};

            static private readonly ILog log = LogManager.GetLogger(typeof(Manager));

            static private State mState = State.Running;

            static public List<string> AuthorizedMethods
            {
                get
                {
                    return new List<string>() { "shutdown", "reboot", "logOff", "lockScreen" };
                }
            }

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

            static public void logOff(List<string> args)
            {
                if (args.Count != 0)
                    throw new Shortcuts.BadArgumentCountException("logOff() admits no argument", 0);

                log.InfoFormat("Called Power.Manager.logOff()");

                Process.Start("shutdown", "/l");
            }

            static public void lockScreen(List<string> args)
            {
                if (args.Count != 0)
                    throw new Shortcuts.BadArgumentCountException("lockScreen() admits no arguments", 0);

                log.InfoFormat("Called Power.Manager.lockScreen()");

                User32.LockWorkStation();
            }
        }
    }
}
