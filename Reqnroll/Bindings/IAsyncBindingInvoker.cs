using System.Threading.Tasks;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.Bindings;

public interface IAsyncBindingInvoker
{
    Task<object> InvokeBindingAsync(IBinding binding, IContextManager contextManager, object[] arguments, ITestTracer testTracer, DurationHolder durationHolder);
}
