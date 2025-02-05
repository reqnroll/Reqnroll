using System.IO;

namespace Reqnroll.Tracing
{
    public class TextWriterTraceListener : ITraceListener
    {
        private readonly TextWriter textWriter;
        private readonly string toolMessagePrefix;

        public TextWriterTraceListener(TextWriter textWriter) : this(textWriter, "")
        {
        }

        public TextWriterTraceListener(TextWriter textWriter, string toolMessagePrefix)
        {
            this.textWriter = textWriter;
            this.toolMessagePrefix = toolMessagePrefix;
        }

        public void WriteTestOutput(string message)
        {
            textWriter.WriteLine(message);
        }

        public void WriteToolOutput(string message)
        {
            textWriter.WriteLine(toolMessagePrefix + message);
        }
    }
}
