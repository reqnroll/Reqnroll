using System;
using System.Runtime.Serialization;

// the exceptions are part of the public API, keep them in Reqnroll namespace
// ReSharper disable once CheckNamespace
namespace Reqnroll
{
    [Serializable]
    public class ReqnrollException : Exception
    {
        public ReqnrollException()
        {
        }

        public ReqnrollException(string message) : base(message)
        {
        }

        public ReqnrollException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ReqnrollException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}