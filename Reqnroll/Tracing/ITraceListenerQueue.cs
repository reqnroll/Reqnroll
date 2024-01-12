using System;

namespace Reqnroll.Tracing
{
    public interface ITraceListenerQueue : IDisposable
    {
        void Start();
        void EnqueueMessage(ITestRunner sourceTestRunner, string message, bool isToolMessgae);
    }
}