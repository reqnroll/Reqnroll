using Reqnroll.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.RuntimeSupport
{
#pragma warning disable CS9113 // Parameter is unread.
    public class FormatterLog(ITraceListener tl) : IFormatterLog
#pragma warning restore CS9113 // Parameter is unread.
    {
        public List<string> entries = new();
        private bool hasDumped = false;
        public void WriteMessage(string message)
        {
            entries.Add($"{DateTime.Now.ToString("HH:mm:ss.fff")}: {message}");
            //tl.WriteToolOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")}: {message}");
            //Console.WriteLine(message);
            //Debug.WriteLine(message);
        }

        public void DumpMessages()
        {
            if (!hasDumped)
                foreach (var msg in entries)
                {
                    tl.WriteToolOutput(msg);
                }
            hasDumped = true;
        }
    }
}
