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
            StartFolder = null;
            StartArguments = new List<string>();
        }

        public ProcessData(string exePath, string startFolder)
        {
            ExePath = exePath;
            StartFolder = startFolder;
            StartArguments = new List<string>();
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
