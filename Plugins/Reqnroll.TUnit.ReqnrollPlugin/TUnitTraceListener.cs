using System;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.TUnit.ReqnrollPlugin;

public class TUnitTraceListener : AsyncTraceListener
{
    private readonly Lazy<IContextManager> _contextManager;

    public TUnitTraceListener(ITraceListenerQueue traceListenerQueue, IObjectContainer container) : base(traceListenerQueue, container)
    {
        _contextManager = new Lazy<IContextManager>(container.Resolve<IContextManager>);
    }

    public override void WriteTestOutput(string message)
    {
        if (TestContext.Current != null)
        {
            TestContext.Current.OutputWriter.WriteLine(message);
        }
        else
        {
            base.WriteTestOutput(message);
        }
    }

    public override void WriteToolOutput(string message)
    {
        if (TestContext.Current != null)
        {
            TestContext.Current.OutputWriter.WriteLine($"-> {message}");
        }
        else
        {
            base.WriteTestOutput(message);
        }
    }
}
