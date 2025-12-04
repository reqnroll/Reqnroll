using System;
using Reqnroll.ErrorHandling;
using Reqnroll.UnitTestProvider;
using TUnit.Core.Exceptions;

namespace Reqnroll.TUnit.ReqnrollPlugin;

/// <summary>
/// Unit test runtime provider for TUnit test framework.
/// </summary>
public class TUnitRuntimeProvider : IUnitTestRuntimeProvider
{
    /// <inheritdoc />
    public void TestPending(string message)
    {
        throw new PendingScenarioException(message);
    }

    /// <inheritdoc />
    public void TestInconclusive(string message)
    {
        throw new InconclusiveTestException(message, null!);
    }

    /// <inheritdoc />
    public void TestIgnore(string message)
    {
        Skip.Test(message);
    }

    /// <inheritdoc />
    public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => exception switch
    {
        InconclusiveTestException or SkipTestException => ScenarioExecutionStatus.Skipped,
        _ => null
    };
}
