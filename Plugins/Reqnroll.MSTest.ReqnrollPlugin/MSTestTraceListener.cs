using Reqnroll.BoDi;
using Reqnroll.Tracing;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    class MSTestTraceListener : AsyncTraceListener
    {
        private readonly IMSTestTestContextProvider _testContextProvider;
        private readonly IMsTestRuntimeAdapter _runtimeAdapter;

        public MSTestTraceListener(ITraceListenerQueue traceListenerQueue, IObjectContainer container, IMSTestTestContextProvider testContextProvider, IMsTestRuntimeAdapter runtimeAdapter) : base(traceListenerQueue, container)
        {
            _testContextProvider = testContextProvider;
            _runtimeAdapter = runtimeAdapter;
        }

        public override void WriteTestOutput(string message)
        {
            _runtimeAdapter.TestContextWriteLine(_testContextProvider.GetTestContext(), message);
        }

        public override void WriteToolOutput(string message)
        {
            _runtimeAdapter.TestContextWriteLine(_testContextProvider.GetTestContext(), "-> " + message);
        }
    }
}