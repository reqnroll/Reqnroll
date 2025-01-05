using System;
using Microsoft.Extensions.DependencyInjection;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public class ServicesEntryPoint
    {
        public IServiceCollection ServiceCollection { get; set; }

        public IServiceProvider ServiceProvider { get; set; }
    }
}
