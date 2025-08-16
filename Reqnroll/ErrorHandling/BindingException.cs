using System;

// the exceptions are part of the public API, keep them in Reqnroll namespace
namespace Reqnroll
{
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
    }
}