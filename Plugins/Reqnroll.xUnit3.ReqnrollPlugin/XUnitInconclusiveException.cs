using System;
using Xunit.Sdk;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

public class XUnitInconclusiveException : XunitException
{
    public XUnitInconclusiveException() : this("The step is inconclusive")
    {
    }

    public XUnitInconclusiveException(string message) : base(message)
    {
    }

    public XUnitInconclusiveException(string message, Exception inner) : base(message, inner)
    {
    }
}
