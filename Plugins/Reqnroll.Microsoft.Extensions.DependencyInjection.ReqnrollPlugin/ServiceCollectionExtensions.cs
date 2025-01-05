using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reqnroll.Bindings;
using Reqnroll.Infrastructure;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service collection extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Reqnroll binding for classes in the assembly where typeof TAssemblyType resides.
        /// </summary>
        /// <typeparam name="TAssemblyType">Any type in an assembly to search for bindings.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddReqnrollBindings<TAssemblyType>(this IServiceCollection services)
        {
            return services.AddReqnrollBindings(typeof(TAssemblyType));
        }

        /// <summary>
        /// Add Reqnroll binding for classes in the assembly where the type resides.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="type">Any type in an assembly to search for bindings.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddReqnrollBindings(this IServiceCollection services, Type type)
        {
            return services.AddReqnrollBindings(type.Assembly);
        }

        /// <summary>
        /// Add Reqnroll binding for classes in an assembly.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assembly">The assembly to search for bindings.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddReqnrollBindings(this IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(BindingAttribute))))
            {
                services.AddScoped(type);
            }

            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm));
            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext));
            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm.FeatureContext));
            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm.ScenarioContext));
            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<ITestRunner>()));
            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<ITestExecutionEngine>()));
            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<IStepArgumentTypeConverter>()));
            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<IStepDefinitionMatchService>()));
            services.TryAddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<IReqnrollOutputHelper>()));

            return services;
        }
    }
}
