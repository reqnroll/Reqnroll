using System;
using System.Runtime.Serialization;

// the exceptions are part of the public API, keep them in Reqnroll namespace
// ReSharper disable once CheckNamespace
namespace Reqnroll
{
    [Serializable]
    public class PendingStepException : ReqnrollException
    {
        public PendingStepException()
            : base("One or more step definitions are not implemented yet.")
        {
        }

        public PendingStepException(string message) : base(message)
        {
        }

        protected PendingStepException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}