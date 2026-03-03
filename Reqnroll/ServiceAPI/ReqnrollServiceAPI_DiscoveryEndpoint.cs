using Reqnroll.Bindings;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Bindings.Provider;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Reqnroll.ServiceAPI
{
    public partial class ReqnrollServiceAPIEndpoint
    {
        public static string DiscoverBindings(Assembly testAssembly, string jsonConfiguration)
        {
            if (_globalContainer == null) throw new InvalidOperationException("The service API endpoint is not initialized. Please call the Initialize method before invoking this method.");
            var bindingRegistryBuilder = _globalContainer.Resolve<IRuntimeBindingRegistryBuilder>();
            BindingProviderService.BuildBindingRegistry(testAssembly, bindingRegistryBuilder);
            var bindingRegistry = _globalContainer.Resolve<IBindingRegistry>();
            return BindingProviderService.GetDiscoveredBindingsFromRegistry(bindingRegistry, testAssembly);
        }
    }
}
