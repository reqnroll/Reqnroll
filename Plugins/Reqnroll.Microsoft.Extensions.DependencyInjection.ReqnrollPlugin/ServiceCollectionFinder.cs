using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionFinder : IServiceCollectionFinder
    {
        private readonly ITestRunnerManager testRunnerManager;
        private (IServiceCollection, ScopeLevelType) _cache;

        public ServiceCollectionFinder(ITestRunnerManager testRunnerManager)
        {
            this.testRunnerManager = testRunnerManager;
        }

        public (IServiceCollection, ScopeLevelType) GetServiceCollection()
        {
            if (_cache != default)
            {
                return _cache;
            }

            var assemblies = testRunnerManager.BindingAssemblies;
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
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
            var assemblyNames = assemblies.Select(a => a.GetName().Name).ToList();
            throw new MissingScenarioDependenciesException(assemblyNames);
        }

        private static IServiceCollection GetServiceCollection(MethodInfo methodInfo)
        {
            var serviceCollection = methodInfo.Invoke(null, null);
            if(methodInfo.ReturnType == typeof(void))
            {
                throw new InvalidScenarioDependenciesException("the method doesn't return a value.");
            }

            if (serviceCollection == null)
            {
                throw new InvalidScenarioDependenciesException("returned null.");
            }

            if (serviceCollection is not IServiceCollection collection)
            {
                throw new InvalidScenarioDependenciesException($"returned {serviceCollection.GetType()}.");
            }
            return collection;
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
