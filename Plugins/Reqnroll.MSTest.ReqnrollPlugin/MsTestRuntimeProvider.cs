using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.MSTest.ReqnrollPlugin;

public class MsTestRuntimeProvider : IUnitTestRuntimeProvider
{
    public void TestPending(string message)
    {
        TestInconclusive(message);
    }

    public void TestInconclusive(string message)
    {
        //Assert.Inconclusive(message);
        throw (Exception)Activator.CreateInstance(MsTestContainerBuilder.GetAssertInconclusiveExceptionType(), message)!;
    }

    public void TestIgnore(string message)
    {
        TestInconclusive(message); // there is no dynamic "Ignore" in MsTest
    }

    public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => exception switch
    {
        var e when e.GetType().Name == "AssertInconclusiveException" => ScenarioExecutionStatus.Skipped,
        _ => null
    };
}