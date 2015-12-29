using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalHotKeys
{
    namespace Windows
    {
        /// <summary>
        ///     Exception thrown when a process does not exist.
        /// </summary>
        /// <para>
        ///     It stores the name of the unknown process in <see cref="ProcessName"/>
        /// </para>
        class ProcessNotFoundException : ApplicationException
        {
            /// <summary>
            ///     The name of the unknown process.
            /// </summary>
            public string ProcessName { get; private set; }

            /// <summary>
            ///     Default constructor.
            /// </summary>
            /// <remarks>The name of the unknown process is not set</remarks>
            public ProcessNotFoundException() :
                base() { }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <remarks>The name of the unknown process is not set</remarks>
            /// <param name="msg">Exception message</param>
            /// <param name="e">Parent exception</param>
            public ProcessNotFoundException(string msg, Exception e) :
                base(msg, e) { }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="filename">Name of the unknown process</param>
            public ProcessNotFoundException(string filename) :
                base()
            {
                ProcessName = filename;
            }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="filename">Name of the unknown process</param>
            /// <param name="msg">Exception message</param>
            public ProcessNotFoundException(string filename, string msg) :
                base(msg)
            {
                ProcessName = filename;
            }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="filename">Name of the unknown process</param>
            /// <param name="msg">Exception message</param>
            /// <param name="e">Parent exception</param>
            public ProcessNotFoundException(string filename, string msg, Exception e) :
                base(msg, e)
            {
                ProcessName = filename;
            }
        }
    }
}
