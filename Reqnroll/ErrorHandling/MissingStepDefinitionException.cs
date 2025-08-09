using System;

// the exceptions are part of the public API, keep them in Reqnroll namespace
namespace Reqnroll
{
    public class MissingStepDefinitionException : ReqnrollException
    {
        public MissingStepDefinitionException()
            : base("No matching step definition found for one or more steps.")
        {
        }
    }
}