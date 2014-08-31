using System;

namespace GlobalHotKeys
{
    class BadConfigException : ApplicationException
    {
        private string p1;
        private string FileName1;
        private uint l;
        private int p2;
        private long p3;

        public string FileName { get; set; }
        public uint? Line { get; set; }
        public uint? Column { get; set; }

        public BadConfigException() :
            base() { }
        public BadConfigException(string msg) :
            base(msg) { }
        public BadConfigException(string msg, Exception e) :
            base(msg, e) { }
        public BadConfigException(string filename, uint line, uint column) :
            base()
        {
            FileName = filename;
            Line = line;
            Column = column;
        }
        public BadConfigException(string msg, string filename, uint line, uint column) :
            base(msg)
        {
            FileName = filename;
            Line = line;
            Column = column;
        }
        public BadConfigException(string msg, string filename, uint line, uint column, Exception e) :
            base(msg, e)
        {
            FileName = filename;
            Line = line;
            Column = column;
        }
    }
}
