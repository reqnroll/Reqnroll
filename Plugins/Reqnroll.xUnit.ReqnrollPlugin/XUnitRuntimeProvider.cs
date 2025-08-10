using System;
using Reqnroll.UnitTestProvider;
using Xunit;

namespace Reqnroll.xUnit.ReqnrollPlugin;

public class XUnitRuntimeProvider : IUnitTestRuntimeProvider
{
    public void TestPending(string message)
    {
        throw new XUnitPendingStepException($"Test pending: {message}");
    }

    public void TestInconclusive(string message)
    {
        throw new XUnitInconclusiveException("Test inconclusive: " + message);
    }

    public void TestIgnore(string message)
    {
        Skip.If(true, message);
    }

    public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => exception switch
    {
        SkipException => ScenarioExecutionStatus.Skipped,
        _ => null
    };
}