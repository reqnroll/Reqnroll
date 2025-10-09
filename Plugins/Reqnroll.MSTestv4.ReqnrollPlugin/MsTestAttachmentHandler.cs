using System;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.MSTestv4.ReqnrollPlugin
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
                _testContextProvider.GetTestContext().AddResultFile(filePath);
            }
            catch (Exception)
            {
                base.AddAttachment(filePath);
            }
        }
    }
}
