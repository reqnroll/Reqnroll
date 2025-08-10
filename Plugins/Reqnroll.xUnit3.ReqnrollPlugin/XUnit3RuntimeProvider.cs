using System;
using Reqnroll.UnitTestProvider;
using Xunit.Sdk;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

public class XUnit3RuntimeProvider : IUnitTestRuntimeProvider
{
    public void TestPending(string message) => throw new XUnitPendingException($"Test pending: {message}");

    public void TestInconclusive(string message) => throw new XUnitInconclusiveException($"Test inconclusive: {message}");

    public void TestIgnore(string message) => Xunit.Assert.Skip(message);

    public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => exception switch
    {
        SkipException => ScenarioExecutionStatus.Skipped,
        _ => null
    };
}

internal class XUnitPendingException(string message) : XunitException(message)
{
}