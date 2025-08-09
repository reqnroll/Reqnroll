using System;

namespace Reqnroll.xUnit.ReqnrollPlugin
{
    public class XUnitInconclusiveException : Exception
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
}