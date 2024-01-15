using System;
using TechTalk.SpecFlow.TestProjectGenerator;

namespace SpecFlow.TestProjectGenerator.Cli
{
    public class OutputWriter : IOutputWriter
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
