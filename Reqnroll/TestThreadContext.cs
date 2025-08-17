using System;
using Reqnroll.BoDi;

namespace Reqnroll;

public class TestThreadContext(IObjectContainer testThreadContainer) 
    : ReqnrollContext, ITestThreadContext
{
    public event Action<TestThreadContext> Disposing;
    public IObjectContainer TestThreadContainer { get; } = testThreadContainer;

    protected override void Dispose()
    {
        Disposing?.Invoke(this);
        base.Dispose();
    }
}