using System;
using System.Runtime.Serialization;
using Xunit.Sdk;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

[Serializable]
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
