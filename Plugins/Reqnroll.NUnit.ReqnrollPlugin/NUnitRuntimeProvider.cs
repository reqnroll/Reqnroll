using System;
using NUnit.Framework;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.NUnit.ReqnrollPlugin;

public class NUnitRuntimeProvider : IUnitTestRuntimeProvider
{
    public void TestPending(string message)
    {
        TestInconclusive(message);
    }

    public void TestInconclusive(string message)
    {
        Assert.Inconclusive(message);
    }

    public void TestIgnore(string message)
    {
        Assert.Ignore(message);
    }

    public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => exception switch
    {
        InconclusiveException or IgnoreException => ScenarioExecutionStatus.Skipped,
        _ => null
    };
}