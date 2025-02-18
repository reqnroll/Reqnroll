using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Bindings;
using Reqnroll.Microsoft.Extensions.DependencyInjection.Internal;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionFinder : IServiceCollectionFinder
    {
        private readonly IAssemblyTypeResolver assemblyTypeResolver;
        private (IServiceCollection, ScopeLevelType) _cache;

        internal ServiceCollectionFinder(IAssemblyTypeResolver assemblyTypeResolver)
        {
            this.assemblyTypeResolver = assemblyTypeResolver;
        }
        
        public ServiceCollectionFinder(IBindingRegistry bindingRegistry)
        {
            this.assemblyTypeResolver = new AssemblyTypeResolver(bindingRegistry);
        }

        public (IServiceCollection, ScopeLevelType) GetServiceCollection()
        {
            if (_cache != default)
            {
                return _cache;
            }

            foreach (var type in assemblyTypeResolver.GetAssemblyTypes())
            {
                foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var scenarioDependenciesAttribute = (ScenarioDependenciesAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(ScenarioDependenciesAttribute));

                    if (scenarioDependenciesAttribute != null)
                    {
                        var serviceCollection = GetServiceCollection(methodInfo);
                        if (scenarioDependenciesAttribute.AutoRegisterBindings)
                        {
                            AddBindingAttributes(serviceCollection);
                        }
                        return _cache = (serviceCollection, scenarioDependenciesAttribute.ScopeLevel);
                    }
                }
            }
            throw new MissingScenarioDependenciesException();
        }

        private static IServiceCollection GetServiceCollection(MethodBase methodInfo)
        {
            return (IServiceCollection)methodInfo.Invoke(null, null);
        }

        private void AddBindingAttributes(IServiceCollection serviceCollection)
        {
            foreach (var type in assemblyTypeResolver.GetAssemblyTypes().Where(t => Attribute.IsDefined(t, typeof(BindingAttribute))))
            {
                serviceCollection.AddScoped(type);
            }
        }
    }
}
