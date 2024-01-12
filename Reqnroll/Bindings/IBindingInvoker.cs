using System;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.Bindings
{
    [Obsolete("Use async version of the interface (IAsyncBindingInvoker) whenever you can")]
    public interface IBindingInvoker
    {
        [Obsolete("Use async version of the method of IAsyncBindingInvoker instead")]
        object InvokeBinding(IBinding binding, IContextManager contextManager, object[] arguments, ITestTracer testTracer, out TimeSpan duration);
    }
}