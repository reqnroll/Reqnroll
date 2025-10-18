using System;
using Reqnroll.BoDi;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.MSTest.ReqnrollPlugin;

public class MsTestRuntimeProvider(IObjectContainer container) : IUnitTestRuntimeProvider
{
    private readonly Lazy<IMsTestRuntimeAdapter> _runtimeAdapter = new(container.Resolve<IMsTestRuntimeAdapter>);

    public void TestPending(string message)
    {
        TestInconclusive(message);
    }

    public void TestInconclusive(string message)
    {
        _runtimeAdapter.Value.ThrowAssertInconclusiveException(message);
    }

    public void TestIgnore(string message)
    {
        TestInconclusive(message); // there is no dynamic "Ignore" in MsTest
    }

    public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => exception switch
    {
        var e when _runtimeAdapter.Value.IsInconclusiveException(e) => ScenarioExecutionStatus.Skipped,
        _ => null
    };
}