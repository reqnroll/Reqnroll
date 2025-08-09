using System;

namespace Reqnroll.Generator
{
    public class TestGeneratorException : Exception
    {
        public TestGeneratorException()
        {
        }

        public TestGeneratorException(string message) : base(message)
        {
        }

        public TestGeneratorException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
