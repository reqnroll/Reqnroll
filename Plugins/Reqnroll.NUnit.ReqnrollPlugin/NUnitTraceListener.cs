using System;
using Reqnroll.BoDi;
using NUnit.Framework;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.NUnit.ReqnrollPlugin
{
    public class NUnitTraceListener : AsyncTraceListener
    {
        private readonly Lazy<IContextManager> _contextManager;

        public NUnitTraceListener(ITraceListenerQueue traceListenerQueue, IObjectContainer container) : base(traceListenerQueue, container)
        {
            _contextManager = new Lazy<IContextManager>(container.Resolve<IContextManager>);

        }

        public override void WriteTestOutput(string message)
        {
            TestContext.WriteLine(message);

            // avoid duplicate output entries in NUnit for scenario, forward only if no scenario active 
            if (_contextManager.Value.ScenarioContext == null)
                base.WriteTestOutput(message);
        }

        public override void WriteToolOutput(string message)
        {
            TestContext.WriteLine("-> " + message);

            // avoid duplicate output entries in NUnit for scenario, forward only if no scenario active 
            if (_contextManager.Value.ScenarioContext == null)
                base.WriteToolOutput(message);
        }
    }
}
