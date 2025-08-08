using System;
using System.Runtime.Serialization;

// the exceptions are part of the public API, keep them in Reqnroll namespace
namespace Reqnroll
{
    [Serializable]
    public class BindingException : ReqnrollException
    {
        public BindingException() : base("Binding error occurred.")
        {
        }

        public BindingException(string message) : base(message)
        {
        }

        public BindingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BindingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}