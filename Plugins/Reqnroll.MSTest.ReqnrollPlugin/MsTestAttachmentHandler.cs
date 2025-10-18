using System;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    public class MSTestAttachmentHandler : ReqnrollAttachmentHandler
    {
        private readonly IMSTestTestContextProvider _testContextProvider;
        private readonly IMsTestRuntimeAdapter _runtimeAdapter;

        public MSTestAttachmentHandler(ITraceListener traceListener, IMSTestTestContextProvider testContextProvider, IMsTestRuntimeAdapter runtimeAdapter) : base(traceListener)
        {
            _testContextProvider = testContextProvider;
            _runtimeAdapter = runtimeAdapter;
        }

        public override void AddAttachment(string filePath)
        {
            try
            {
                _runtimeAdapter.TestContextAddResultFile(_testContextProvider.GetTestContext(), filePath);
            }
            catch (Exception)
            {
                base.AddAttachment(filePath);
            }
        }
    }
}
