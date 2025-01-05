using System;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Discovery;
using Reqnroll.BindingSkeletons;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.ErrorHandling;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(Reqnroll.Microsoft.Extensions.DependencyInjection.DependencyInjectionPlugin))]

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public class DependencyInjectionPlugin : IRuntimePlugin
    {
        private readonly object _registrationLock = new object();

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += CustomizeGlobalDependencies;
            runtimePluginEvents.CustomizeFeatureDependencies += CustomizeFeatureDependenciesEventHandler;
            runtimePluginEvents.CustomizeScenarioDependencies += CustomizeScenarioDependenciesEventHandler;
        }

        private void CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs args)
        {
            if (!args.ObjectContainer.IsRegistered<IServiceCollectionFinder>())
            {
                lock (_registrationLock)
                {
                    if (!args.ObjectContainer.IsRegistered<IServiceCollectionFinder>())
                    {
                        args.ObjectContainer.RegisterTypeAs<DependencyInjectionTestObjectResolver, ITestObjectResolver>();
                        args.ObjectContainer.RegisterTypeAs<ServiceCollectionFinder, IServiceCollectionFinder>();
                    }

                    // We store the (MS) service provider in the global (BoDi) container, we create it only once.
                    // It must be lazy (hence factory) because at this point we still don't have the bindings mapped.
                    args.ObjectContainer.RegisterFactoryAs<RootServiceProviderContainer>(() =>
                    {
                        var serviceCollectionFinder = args.ObjectContainer.Resolve<IServiceCollectionFinder>();
                        var (services, scoping) = serviceCollectionFinder.GetServiceEntryPoint();

                        if (services.ServiceCollection != null)
                        {
                            RegisterProxyBindings(args.ObjectContainer, services.ServiceCollection);
                        }

                        return services.ServiceProvider != null 
                            ? new RootServiceProviderContainer(services.ServiceProvider, scoping) 
                            : new RootServiceProviderContainer(services.ServiceCollection.BuildServiceProvider(), scoping);
                    });

                    args.ObjectContainer.RegisterFactoryAs<IServiceProvider>(() =>
                    {
                        return args.ObjectContainer.Resolve<RootServiceProviderContainer>().ServiceProvider;
                    });

                    // Will make sure DI scope is disposed.
                    var lcEvents = args.ObjectContainer.Resolve<RuntimePluginTestExecutionLifecycleEvents>();
                    lcEvents.AfterScenario += AfterScenarioPluginLifecycleEventHandler;
                    lcEvents.AfterFeature += AfterFeaturePluginLifecycleEventHandler;
                }
                args.ObjectContainer.Resolve<IServiceCollectionFinder>();
            }
        }

        private static void CustomizeFeatureDependenciesEventHandler(object sender, CustomizeFeatureDependenciesEventArgs args)
        {
            // At this point we have the bindings, we can resolve the service provider, which will build it if it's the first time.
            var spContainer = args.ObjectContainer.Resolve<RootServiceProviderContainer>();

            if (spContainer.Scoping == ScopeLevelType.Feature)
            {
                var serviceProvider = spContainer.ServiceProvider;

                // Now we can register a new scoped service provider
                args.ObjectContainer.RegisterFactoryAs<IServiceProvider>(() =>
                {
                    var scope = serviceProvider.CreateScope();
                    Context.BindMappings.TryAdd(scope.ServiceProvider, args.ObjectContainer.Resolve<IContextManager>());
                    Context.ActiveServiceScopes.TryAdd(args.ObjectContainer.Resolve<FeatureContext>(), scope);
                    return scope.ServiceProvider;
                });
            }
        }

        private static void AfterFeaturePluginLifecycleEventHandler(object sender, RuntimePluginAfterFeatureEventArgs eventArgs)
        {
            if (Context.ActiveServiceScopes.TryRemove(eventArgs.ObjectContainer.Resolve<FeatureContext>(), out var serviceScope))
            {
                Context.BindMappings.TryRemove(serviceScope.ServiceProvider, out _);
                serviceScope.Dispose();
            }
        }

        private static void CustomizeScenarioDependenciesEventHandler(object sender, CustomizeScenarioDependenciesEventArgs args)
        {
            // At this point we have the bindings, we can resolve the service provider, which will build it if it's the first time.
            var spContainer = args.ObjectContainer.Resolve<RootServiceProviderContainer>();

            if (spContainer.Scoping == ScopeLevelType.Scenario)
            {
                var serviceProvider = spContainer.ServiceProvider;
                // Now we can register a new scoped service provider
                args.ObjectContainer.RegisterFactoryAs<IServiceProvider>(() =>
                {
                    var scope = serviceProvider.CreateScope();
                    Context.BindMappings.TryAdd(scope.ServiceProvider, args.ObjectContainer.Resolve<IContextManager>());
                    Context.ActiveServiceScopes.TryAdd(args.ObjectContainer.Resolve<ScenarioContext>(), scope);
                    return scope.ServiceProvider;
                });
            }
        }

        private static void AfterScenarioPluginLifecycleEventHandler(object sender, RuntimePluginAfterScenarioEventArgs eventArgs)
        {
            if (Context.ActiveServiceScopes.TryRemove(eventArgs.ObjectContainer.Resolve<ScenarioContext>(), out var serviceScope))
            {
                Context.BindMappings.TryRemove(serviceScope.ServiceProvider, out _);
                serviceScope.Dispose();
            }
        }

        private static void RegisterProxyBindings(IObjectContainer objectContainer, IServiceCollection services)
        {
            // Required for DI of binding classes that want container injections
            // While they can (and should) use the method params for injection, we can support it.
            // Note that in Feature mode, one can't inject "ScenarioContext", this can only be done from method params.

            // Bases on this: https://docs.specflow.org/projects/specflow/en/latest/Extend/Available-Containers-%26-Registrations.html
            // Might need to add more...

            services.AddSingleton<IObjectContainer>(objectContainer);
            services.AddSingleton(sp => objectContainer.Resolve<IRuntimeConfigurationProvider>());
            services.AddSingleton(sp => objectContainer.Resolve<ITestRunnerManager>());
            services.AddSingleton(sp => objectContainer.Resolve<IStepFormatter>());
            services.AddSingleton(sp => objectContainer.Resolve<ITestTracer>());
            services.AddSingleton(sp => objectContainer.Resolve<ITraceListener>());
            services.AddSingleton(sp => objectContainer.Resolve<ITraceListenerQueue>());
            services.AddSingleton(sp => objectContainer.Resolve<IErrorProvider>());
            services.AddSingleton(sp => objectContainer.Resolve<IRuntimeBindingSourceProcessor>());
            services.AddSingleton(sp => objectContainer.Resolve<IBindingRegistry>());
            services.AddSingleton(sp => objectContainer.Resolve<IBindingFactory>());
            services.AddSingleton(sp => objectContainer.Resolve<IStepDefinitionRegexCalculator>());
            // TODO: Use async version of the interface (IAsyncBindingInvoker)
            //services.AddSingleton(sp => objectContainer.Resolve<IBindingInvoker>());
            services.AddSingleton(sp => objectContainer.Resolve<IStepDefinitionSkeletonProvider>());
            services.AddSingleton(sp => objectContainer.Resolve<ISkeletonTemplateProvider>());
            services.AddSingleton(sp => objectContainer.Resolve<IStepTextAnalyzer>());
            services.AddSingleton(sp => objectContainer.Resolve<IRuntimePluginLoader>());
            services.AddSingleton(sp => objectContainer.Resolve<IBindingAssemblyLoader>());
            services.AddSingleton(sp => objectContainer.Resolve<IUnitTestRuntimeProvider>());

            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm));
            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext));
            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm.FeatureContext));
            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm.ScenarioContext));
            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<ITestRunner>()));
            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<ITestExecutionEngine>()));
            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<IStepArgumentTypeConverter>()));
            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<IStepDefinitionMatchService>()));
            services.AddTransient(sp => sp.GetTestThreadDependency(cm => cm.TestThreadContext.TestThreadContainer.Resolve<IReqnrollOutputHelper>()));
        }

        private class RootServiceProviderContainer
        {
            public IServiceProvider ServiceProvider { get; }
            public ScopeLevelType Scoping { get; }

            public RootServiceProviderContainer(IServiceProvider sp, ScopeLevelType scoping)
            {
                ServiceProvider = sp;
                Scoping = scoping;
            }
        }
    }
}
