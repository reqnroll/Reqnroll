using System.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Tracing;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    class MSTestTraceListener : AsyncTraceListener
    {
        private readonly IMSTestTestContextProvider _testContextProvider;
        
        public MSTestTraceListener(ITraceListenerQueue traceListenerQueue, IObjectContainer container, IMSTestTestContextProvider testContextProvider) : base(traceListenerQueue, container)
        {
            _testContextProvider = testContextProvider;
        }

        public override void WriteTestOutput(string message)
        {
            //_testContextProvider.GetTestContext().WriteLine(message);
            object testContext = _testContextProvider.GetTestContext();
            testContext.GetType().GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Instance, null, [ typeof(string )], null)!
                       .Invoke(testContext, [message]);
        }

        public override void WriteToolOutput(string message)
        {
            //_testContextProvider.GetTestContext().WriteLine("-> " + message);
            object testContext = _testContextProvider.GetTestContext();
            testContext.GetType().GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Instance, null, [typeof(string)], null)!
                       .Invoke(testContext, ["-> " + message]);
        }
    }
}