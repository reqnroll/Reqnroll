using System;
using System.Runtime.Serialization;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

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
