using System;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

public class XUnit3TraceListener(ITraceListenerQueue traceListenerQueue, IObjectContainer container)
    : AsyncTraceListener(traceListenerQueue, container)
{
}
