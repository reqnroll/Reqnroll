// the exceptions are part of the public API, keep them in Reqnroll namespace
// ReSharper disable once CheckNamespace
namespace Reqnroll;

public class PendingStepException(string message) : ReqnrollException(message)
{
    internal const string GenericErrorMessage = "The step definition is not implemented.";

    public PendingStepException() : this(GenericErrorMessage)
    {
    }
}
