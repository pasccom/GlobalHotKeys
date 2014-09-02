using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class BadArgumentCountException : ApplicationException
        {
            public uint Max { get; set; }
            public uint Min { get; set; }

            public BadArgumentCountException() :
                base() { }
            public BadArgumentCountException(string msg) :
                base(msg) { }
            public BadArgumentCountException(string msg, Exception e) :
                base(msg, e) { }

            public BadArgumentCountException(uint n) :
                base() 
            { 
                Min = n; 
                Max = n; 
            }
            public BadArgumentCountException(string msg, uint n) :
                base(msg)
            {
                Min = n; 
                Max = n; 
            }
            public BadArgumentCountException(string msg, uint n, Exception e) :
                base(msg, e)
            {
                Min = n; 
                Max = n; 
            }

            public BadArgumentCountException(uint min, uint max) :
                base() 
            { 
                Min = min; 
                Max = max; 
            }
            public BadArgumentCountException(string msg, uint min, uint max) :
                base(msg)
            { 
                Min = min; 
                Max = max; 
            }
            public BadArgumentCountException(string msg, uint min, uint max, Exception e) :
                base(msg, e) 
            {
                Min = min;
                Max = max;
            }
        }
    }
}
