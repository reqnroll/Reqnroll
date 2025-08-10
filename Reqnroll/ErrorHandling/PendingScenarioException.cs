using System;

namespace Reqnroll.ErrorHandling;

/// <summary>
/// This exception is thrown by Reqnroll test execution framework plugins to indicate a pending scenario if there is no dedicated support for pending errors in the test execution framework for that.
/// </summary>
public class PendingScenarioException : ReqnrollException
{
    internal const string GenericErrorMessage = "One or more step definitions are not implemented yet.";

    public PendingScenarioException() : base(GenericErrorMessage)
    {
    }

    public PendingScenarioException(string message) : base(message)
    {
    }

    public PendingScenarioException(string message, Exception inner) : base(message, inner)
    {
    }
}
