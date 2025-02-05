using Reqnroll.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Autofac;
public class ConfigurationMethodsProvider(ITestAssemblyProvider _testAssemblyProvider) : IConfigurationMethodsProvider
{
    public IEnumerable<MethodInfo> GetConfigurationMethods()
    {
        return _testAssemblyProvider.TestAssembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
    }
}
