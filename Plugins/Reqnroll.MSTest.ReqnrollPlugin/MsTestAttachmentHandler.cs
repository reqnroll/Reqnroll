using System;
using System.Reflection;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    public class MSTestAttachmentHandler : ReqnrollAttachmentHandler
    {
        private readonly IMSTestTestContextProvider _testContextProvider;

        public MSTestAttachmentHandler(ITraceListener traceListener, IMSTestTestContextProvider testContextProvider) : base(traceListener)
        {
            _testContextProvider = testContextProvider;
        }

        public override void AddAttachment(string filePath)
        {
            try
            {
                object testContext = _testContextProvider.GetTestContext();
                //testContext.AddResultFile(filePath);
                testContext.GetType().GetMethod("AddResultFile")!.Invoke(testContext, [filePath]);
            }
            catch (Exception)
            {
                base.AddAttachment(filePath);
            }
        }
    }
}
