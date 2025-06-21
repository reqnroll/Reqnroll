using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.RuntimeSupport
{
    public class FormatterLog : IFormatterLog
    {
        public void WriteMessage(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
