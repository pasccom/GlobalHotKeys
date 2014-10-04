using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalHotKeys.Windows
{
    class ProcessData
    {
        private string mStartPath;

        public string ExePath { get; set; }
        public bool Shell { get; private set; }
        public string StartFolder { get; set; }
        public string StartPath
        {
            get
            {
                if (mStartPath == null)
                    return ExePath;
                else
                    return mStartPath;
            }

            set
            {
                mStartPath = value;
            }
        }

        public List<string> StartArguments { get; private set; }

        public ProcessData()
        {
            ExePath = null;
            StartPath = null;
            Shell = false;
            StartFolder = null;
            StartArguments = new List<string>();
        }

        public ProcessData(string exePath, string startFolder)
        {
            ExePath = exePath;
            StartPath = null;
            Shell = false;
            StartFolder = startFolder;
            StartArguments = new List<string>();
        }

        public bool parseShell(string arg)
        {
            switch (arg) {
            case "X":
                Shell = true;
                return true;
            case "O":
                Shell = false;
                return true;
            default:
                return false;
            }
        }

        public void addArgument(string arg) 
        {
            StartArguments.Add(arg);
        }

        public void resetArguments()
        {
            StartArguments.Clear();
        }
    }
}
