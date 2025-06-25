using Reqnroll.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.RuntimeSupport
{
    public class FormatterLog(ITraceListener tl) : IFormatterLog
    {
        public void WriteMessage(string message)
        {
            tl.WriteToolOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")}: {message}");
            //Console.WriteLine(message);
            //Debug.WriteLine(message);
        }
    }
}
