using System;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.Bindings
{
    [Obsolete("The synchronous code invocation API has been deprecated. Please use IAsyncBindingInvoker instead.", true)]
    public interface IBindingInvoker
    {
        object InvokeBinding(IBinding binding, IContextManager contextManager, object[] arguments, ITestTracer testTracer, out TimeSpan duration);
    }
}