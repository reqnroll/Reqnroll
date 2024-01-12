using System;
using BoDi;

namespace Reqnroll
{
    public interface ITestThreadContext : IReqnrollContext
    {
        IObjectContainer TestThreadContainer { get; }
    }

    public class TestThreadContext : ReqnrollContext, ITestThreadContext
    {
        public event Action<TestThreadContext> Disposing;
        public IObjectContainer TestThreadContainer { get; }

        public TestThreadContext(IObjectContainer testThreadContainer)
        {
            TestThreadContainer = testThreadContainer;
        }

        protected override void Dispose()
        {
            Disposing?.Invoke(this);
            base.Dispose();
        }
    }
}
