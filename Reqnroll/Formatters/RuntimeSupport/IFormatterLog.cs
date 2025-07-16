using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.RuntimeSupport
{
    public interface IFormatterLog
    {
        public void WriteMessage(string message);
        public void DumpMessages();
    }
}
