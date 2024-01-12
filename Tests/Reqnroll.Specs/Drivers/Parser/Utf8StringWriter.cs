using System.IO;
using System.Text;

namespace Reqnroll.Specs.Drivers.Parser
{
    internal class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}