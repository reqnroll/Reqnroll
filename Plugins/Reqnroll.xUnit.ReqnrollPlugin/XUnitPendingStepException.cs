using System;
using System.Runtime.Serialization;

// the exceptions are part of the public API, keep them in Reqnroll namespace
// ReSharper disable once CheckNamespace
namespace Reqnroll.xUnit.ReqnrollPlugin
{
    public class XUnitPendingStepException : ReqnrollException
    {
        public XUnitPendingStepException() : this("The step is pending.")
        {
        }

        public XUnitPendingStepException(string message) : base(message)
        {
        }

        public XUnitPendingStepException(string message, Exception inner) : base(message, inner)
        {
        }

        protected XUnitPendingStepException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
