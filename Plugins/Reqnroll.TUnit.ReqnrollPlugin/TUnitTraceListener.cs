using System;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.TUnit.ReqnrollPlugin;

/// <summary>
/// Trace listener for TUnit test framework that writes output to the TUnit test context.
/// </summary>
public class TUnitTraceListener : AsyncTraceListener
{
    private readonly Lazy<IContextManager> _contextManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TUnitTraceListener"/> class.
    /// </summary>
    /// <param name="traceListenerQueue">The trace listener queue.</param>
    /// <param name="container">The object container for dependency resolution.</param>
    public TUnitTraceListener(ITraceListenerQueue traceListenerQueue, IObjectContainer container) : base(traceListenerQueue, container)
    {
        _contextManager = new Lazy<IContextManager>(container.Resolve<IContextManager>);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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
