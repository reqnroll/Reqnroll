using Xunit.Abstractions;

namespace Reqnroll.TestProjectGenerator.Tests.Stubs
{
    public class OutputWriterStub : IOutputWriter
    {
        private readonly ITestOutputHelper output;

        public OutputWriterStub(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void WriteLine(string message)
        {
            output.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            output.WriteLine(format, args);
        }
    }
}
