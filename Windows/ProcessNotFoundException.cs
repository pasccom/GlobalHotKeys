using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalHotKeys
{
    namespace Windows
    {
        class ProcessNotFoundException : ApplicationException
        {
            public string ProcessName { get; set; }

            public ProcessNotFoundException() :
            base() { }
            public ProcessNotFoundException(string msg, Exception e) :
                base(msg, e) { }
            public ProcessNotFoundException(string filename) :
                base()
            {
                ProcessName = filename;
            }
            public ProcessNotFoundException(string filename, string msg) :
                base(msg)
            {
                ProcessName = filename;
            }
            public ProcessNotFoundException(string filename, string msg, Exception e) :
                base(msg, e)
            {
                ProcessName = filename;
            }
        }
    }
}
