using System;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

public class XUnit3TraceListener(ITraceListenerQueue traceListenerQueue, IObjectContainer container) : AsyncTraceListener(traceListenerQueue, container)
{
    private readonly Lazy<IContextManager> _contextManager = new(container.Resolve<IContextManager>);

    public override void WriteTestOutput(string message)
    {
        var testOutputHelper = GetTestOutputHelper();
        if (testOutputHelper != null)
            testOutputHelper.WriteLine(message);
        else
            base.WriteTestOutput(message);
    }

    public override void WriteToolOutput(string message)
    {
        var testOutputHelper = GetTestOutputHelper();
        if (testOutputHelper != null)
            testOutputHelper.WriteLine("-> " + message);
        else
            base.WriteToolOutput(message);
    }
    private ITestOutputHelper GetTestOutputHelper()
    {
        var scenarioContext = _contextManager.Value.ScenarioContext;
        if (scenarioContext == null) return null;

        return !scenarioContext.ScenarioContainer.IsRegistered<ITestOutputHelper>()
            ? null
            : scenarioContext.ScenarioContainer.Resolve<ITestOutputHelper>();
    }
}
