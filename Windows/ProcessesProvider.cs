using System;
using System.Collections.Generic;
using log4net;

namespace GlobalHotKeys
{
    namespace Windows
    {
        abstract class ProcessesProvider
        {
            static private readonly ILog log = LogManager.GetLogger(typeof(ProcessesProvider));

            private Dictionary<string, ProcessData> mProcesses;

            public ProcessesProvider()
            {
                mProcesses = new Dictionary<string, ProcessData>();
            }

            protected void newProcess(string name, ProcessData process)
            {
                log.Info("Added new process: " + name);
                log.Debug("\t-Executable path: " + process.ExePath);
                log.Debug("\t-Start folder: " + process.StartFolder);
                log.Debug("\t-New shell: " + (process.Shell ? "Yes" : "No"));
                log.Debug("\t-Real executable path: " + process.StartPath);
                log.Debug("\t-Arguments: " + String.Join<string>(", ", process.StartArguments));

                try {
                    mProcesses.Add(name, process);
                } catch (ArgumentException) {
                    log.WarnFormat("A key with the same name (\"{0}\") already exists. It will be overwritten.", name);
                    mProcesses.Remove(name);
                    mProcesses.Add(name, process);
                }
            }

            public ProcessData this[string name] 
            { 
                get 
                { 
                    return getProcess(name); 
                } 
            }

            public ProcessData getProcess(string name)
            {
                try {
                    return mProcesses[name];
                } catch (KeyNotFoundException e) {
                    throw new ProcessNotFoundException(name, "The process \"" + name + "\" was not found in known process list.", e);
                }
            }

            public abstract void parseConfig();
        }
    }
}
