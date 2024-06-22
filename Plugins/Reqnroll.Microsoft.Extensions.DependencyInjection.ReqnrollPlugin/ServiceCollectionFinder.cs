using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Bindings;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionFinder : IServiceCollectionFinder
    {
        private readonly IBindingRegistry bindingRegistry;
        private (IServiceCollection, ScopeLevelType) _cache;

        public ServiceCollectionFinder(IBindingRegistry bindingRegistry)
        {
            this.bindingRegistry = bindingRegistry;
        }

        public (IServiceCollection, ScopeLevelType) GetServiceCollection()
        {
            if (_cache != default)
            {
                return _cache;
            }

            var assemblies = bindingRegistry.GetBindingAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        var scenarioDependenciesAttribute = (ScenarioDependenciesAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(ScenarioDependenciesAttribute));

                        if (scenarioDependenciesAttribute != null)
                        {
                            var serviceCollection = GetServiceCollection(methodInfo);
                            if (scenarioDependenciesAttribute.AutoRegisterBindings)
                            {
                                AddBindingAttributes(assemblies, serviceCollection);
                            }
                            return _cache = (serviceCollection, scenarioDependenciesAttribute.ScopeLevel);
                        }
                    }
                }
            }
            throw new MissingScenarioDependenciesException();
        }

        private static IServiceCollection GetServiceCollection(MethodBase methodInfo)
        {
            return (IServiceCollection)methodInfo.Invoke(null, null);
        }

        private static void AddBindingAttributes(IEnumerable<Assembly> bindingAssemblies, IServiceCollection serviceCollection)
        {
            foreach (var assembly in bindingAssemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(BindingAttribute))))
                {
                    serviceCollection.AddScoped(type);
                }
            }
        }
    }
}
