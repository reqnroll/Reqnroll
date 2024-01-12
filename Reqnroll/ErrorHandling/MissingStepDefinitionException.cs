using System;
using System.Runtime.Serialization;

// the exceptions are part of the public API, keep them in Reqnroll namespace
namespace Reqnroll
{
    [Serializable]
    public class MissingStepDefinitionException : ReqnrollException
    {
        public MissingStepDefinitionException()
            : base("No matching step definition found for one or more steps.")
        {
        }

        protected MissingStepDefinitionException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}