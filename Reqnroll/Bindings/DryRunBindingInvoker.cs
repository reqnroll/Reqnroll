using System;
using System.Threading.Tasks;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.Bindings;

public class DryRunBindingInvoker : IAsyncBindingInvoker
{
    public object InvokeBinding(IBinding binding, IContextManager contextManager, object[] arguments, ITestTracer testTracer, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;
        return null;
    }

    public Task<object> InvokeBindingAsync(IBinding binding, IContextManager contextManager, object[] arguments, ITestTracer testTracer, DurationHolder durationHolder)
    {
        durationHolder.Duration = TimeSpan.Zero;
        return Task.FromResult((object)null);
    }
}
