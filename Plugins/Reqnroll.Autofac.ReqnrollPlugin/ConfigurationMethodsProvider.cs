using Reqnroll.Bindings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Autofac;
public class ConfigurationMethodsProvider(IBindingRegistry _bindingRegistry) : IConfigurationMethodsProvider
{
    public IEnumerable<MethodInfo> GetConfigurationMethods()
    {
        return _bindingRegistry.GetBindingAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
    }
}
