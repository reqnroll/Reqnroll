using System;
using Reqnroll.ErrorHandling;
using Reqnroll.UnitTestProvider;
using TUnit.Core.Exceptions;

namespace Reqnroll.TUnit.ReqnrollPlugin;

public class TUnitRuntimeProvider : IUnitTestRuntimeProvider
{
    public void TestPending(string message)
    {
        throw new PendingScenarioException(message);
    }

    public void TestInconclusive(string message)
    {
        throw new InconclusiveTestException(message, null!);
    }

    public void TestIgnore(string message)
    {
        Skip.Test(message);
    }

    public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => exception switch
    {
        InconclusiveTestException or SkipTestException => ScenarioExecutionStatus.Skipped,
        _ => null
    };
}
