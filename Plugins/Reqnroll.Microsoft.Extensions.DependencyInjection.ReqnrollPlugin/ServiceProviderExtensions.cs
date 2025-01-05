using Reqnroll.Infrastructure;
using System;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    internal static class ServiceProviderExtensions
    {
        public static TResult GetTestThreadDependency<TResult>(this IServiceProvider sp, Func<IContextManager, TResult> selector) where TResult : class
        {
            string GetErrorMessage()
                => $"Unable to access test execution dependent service '{typeof(TResult).FullName}' with the Reqnroll.Microsoft.Extensions.DependencyInjection plugin. This service is only available once test execution has been started and cannot be used in '[BeforeTestRun]' hook. See https://go.reqnroll.net/doc-migrate-specflow-testrun-hooks for details.";

            if (!Context.BindMappings.TryGetValue(sp, out var contextManager))
            {
                throw new ReqnrollException(GetErrorMessage());
            }

            TResult result;
            try
            {
                result = selector(contextManager);
            }
            catch (Exception ex)
            {
                throw new ReqnrollException(GetErrorMessage(), ex);
            }

            if (result == null)
                throw new ReqnrollException(GetErrorMessage());

            return result;
        }
    }
}
