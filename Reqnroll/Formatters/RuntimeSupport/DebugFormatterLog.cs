using Reqnroll.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.RuntimeSupport
{
    public class DebugFormatterLog() : IFormatterLog
    {
        public List<string> entries = new();
        private bool hasDumped = false;
        public void WriteMessage(string message)
        {
            entries.Add($"{DateTime.Now.ToString("HH:mm:ss.fff")}: {message}");
        }

        public void DumpMessages()
        {
            if (!hasDumped)
                foreach (var msg in entries)
                {
                    Debug.WriteLine(msg);
                }
            hasDumped = true;
        }
    }
}
