using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Infrastructure;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    internal static class Context
    {
        public static readonly ConcurrentDictionary<IServiceProvider, IContextManager> BindMappings =
            new ConcurrentDictionary<IServiceProvider, IContextManager>();

        public static readonly ConcurrentDictionary<IReqnrollContext, IServiceScope> ActiveServiceScopes =
            new ConcurrentDictionary<IReqnrollContext, IServiceScope>();
    }
}
