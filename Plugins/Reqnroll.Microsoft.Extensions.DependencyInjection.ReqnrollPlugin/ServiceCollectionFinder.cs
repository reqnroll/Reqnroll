using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Infrastructure;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionFinder : IServiceCollectionFinder
    {
        private readonly ITestRunnerManager _testRunnerManager;
        private readonly IRuntimeBindingRegistryBuilder _bindingRegistryBuilder;
        private readonly ITestAssemblyProvider _testAssemblyProvider;
        private (IServiceCollection ServiceCollection, ScenarioDependenciesAttribute Attribute) _cache;

        public ServiceCollectionFinder(ITestRunnerManager testRunnerManager, IRuntimeBindingRegistryBuilder bindingRegistryBuilder, ITestAssemblyProvider testAssemblyProvider)
        {
            _testRunnerManager = testRunnerManager;
            _bindingRegistryBuilder = bindingRegistryBuilder;
            _testAssemblyProvider = testAssemblyProvider;
        }

        public ServiceProviderLifetimeType GetServiceProviderLifetime()
        {
            if (_cache == default)
            {
                PopulateCache();
            }

            return _cache.Attribute.ServiceProviderLifetime;
        }

        public (IServiceCollection, ScopeLevelType) GetServiceCollection()
        {
            if (_cache == default)
            {
                PopulateCache();
            }

            return (_cache.ServiceCollection, _cache.Attribute.ScopeLevel);
        }

        private void PopulateCache()
        {
            if (_cache != default)
            {
                return;
            }

            var assemblies = _testRunnerManager.BindingAssemblies ?? _bindingRegistryBuilder.GetBindingAssemblies(_testAssemblyProvider.TestAssembly);
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

                            // If the ServiceProviderLifetime is Scenario, set the ScopeLevel to Scenario to match.
                            if (scenarioDependenciesAttribute.ServiceProviderLifetime == ServiceProviderLifetimeType.Scenario)
                            {
                                scenarioDependenciesAttribute.ScopeLevel = ScopeLevelType.Scenario;
                            }

                            _cache = (serviceCollection, scenarioDependenciesAttribute);
                            return;
                        }
                    }
                }
            }
            var assemblyNames = assemblies.Select(a => a.GetName().Name).ToList();
            throw new MissingScenarioDependenciesException(assemblyNames);
        }

        private static IServiceCollection GetServiceCollection(MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType == typeof(void))
            {
                throw new InvalidScenarioDependenciesException("the method doesn't return a value.");
            }

            var serviceCollection = methodInfo.Invoke(null, null);

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
