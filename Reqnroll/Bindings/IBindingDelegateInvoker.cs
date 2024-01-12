using System;
using System.Threading.Tasks;

namespace Reqnroll.Bindings
{
    public interface IBindingDelegateInvoker
    {
        Task<object> InvokeDelegateAsync(Delegate bindingDelegate, object[] invokeArgs, ExecutionContextHolder executionContext);
    }
}