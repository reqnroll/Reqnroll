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
        private readonly IBindingRegistry _bindingRegistry;
        private (ServicesEntryPoint, ScopeLevelType) _cache;

        public ServiceCollectionFinder(IBindingRegistry bindingRegistry)
        {
            _bindingRegistry = bindingRegistry;
        }

        public (ServicesEntryPoint, ScopeLevelType) GetServiceEntryPoint()
        {
            if (_cache != default)
            {
                return _cache;
            }

            var assemblies = _bindingRegistry.GetBindingAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        var scenarioDependenciesAttribute = (ScenarioDependenciesAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(ScenarioDependenciesAttribute));

                        if (scenarioDependenciesAttribute != null)
                        {
                            var entryPoint = GetServicesEntryPoint(methodInfo);

                            if (entryPoint == null)
                            {
                                throw new MissingScenarioDependenciesException();
                            }

                            if (entryPoint.ServiceCollection != null && scenarioDependenciesAttribute.AutoRegisterBindings)
                            {
                                AddBindingAttributes(assemblies, entryPoint.ServiceCollection);
                            }

                            return _cache = (entryPoint, scenarioDependenciesAttribute.ScopeLevel);
                        }
                    }
                }
            }
            throw new MissingScenarioDependenciesException();
        }

        private static ServicesEntryPoint GetServicesEntryPoint(MethodBase methodInfo)
        {
            var entry = methodInfo.Invoke(null, null);

            if (entry is IServiceCollection serviceCollection)
            {
                return new ServicesEntryPoint
                {
                    ServiceCollection = serviceCollection
                };
            }
            
            if (entry is IServiceProvider serviceProvider)
            {
                return new ServicesEntryPoint
                {
                    ServiceProvider = serviceProvider
                };
            }

            return null;
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
