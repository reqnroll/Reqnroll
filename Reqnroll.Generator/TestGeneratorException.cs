using System;
using System.Runtime.Serialization;

namespace Reqnroll.Generator
{
    [Serializable]
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

        protected TestGeneratorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
