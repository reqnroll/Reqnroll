using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.MSTestv4.ReqnrollPlugin;

public class MsTestRuntimeProvider : IUnitTestRuntimeProvider
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
        TestInconclusive(message); // there is no dynamic "Ignore" in MsTest
    }

    public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => exception switch
    {
        AssertInconclusiveException => ScenarioExecutionStatus.Skipped,
        _ => null
    };
}