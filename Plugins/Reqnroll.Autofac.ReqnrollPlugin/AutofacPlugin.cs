using System;
using Autofac;
using Reqnroll.BoDi;
using Reqnroll.Autofac;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(AutofacPlugin))]

namespace Reqnroll.Autofac;

public class AutofacPlugin : IRuntimePlugin
{
    private static readonly object _registrationLock = new();

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.CustomizeGlobalDependencies += (_, args) =>
        {
            // temporary fix for CustomizeGlobalDependencies called multiple times
            // see https://github.com/SpecFlowOSS/SpecFlow/issues/948
            if (!args.ObjectContainer.IsRegistered<IContainerBuilderFinder>())
            {
                // an extra lock to ensure that there are not two fast threads re-registering the same stuff
                lock (_registrationLock)
                {
                    if (!args.ObjectContainer.IsRegistered<IContainerBuilderFinder>())
                    {
                        var testRunContainer = args.ObjectContainer;
                        RegisterTestRunPluginTypes(testRunContainer);

                        var containerBuilderFinder = args.ObjectContainer.Resolve<IContainerBuilderFinder>();
                        var configureGlobalContainer = containerBuilderFinder.GetConfigureGlobalContainer();
                        if (configureGlobalContainer != null)
                        {
                            var containerBuilder = configureGlobalContainer(new global::Autofac.ContainerBuilder());
                            args.ObjectContainer.RegisterFactoryAs(() => containerBuilder.Build());
                        }
                    }
                }

                // workaround for parallel execution issue - this should be rather a feature in BoDi?
                args.ObjectContainer.Resolve<IContainerBuilderFinder>();
            }
        };

        runtimePluginEvents.CustomizeTestThreadDependencies += (_, args) =>
        {
            if (args.ObjectContainer.BaseContainer.IsRegistered<IContainer>())
            {
                var container = args.ObjectContainer.BaseContainer.Resolve<IContainer>();
                args.ObjectContainer.RegisterFactoryAs(() => container.BeginLifetimeScope(nameof(TestThreadContext)));
            }
        };

        runtimePluginEvents.CustomizeFeatureDependencies += (_, args) =>
        {
            var containerBuilderFinder = args.ObjectContainer.Resolve<IContainerBuilderFinder>();

            var featureScopeFinder = containerBuilderFinder.GetFeatureLifetimeScope();

            ILifetimeScope featureScope = null;

            if (featureScopeFinder != null)
            {
                // if the user provided a feature scope creation method, we use that
                featureScope = featureScopeFinder();
            }
            else if (args.ObjectContainer.BaseContainer.IsRegistered<ILifetimeScope>())
            {
                // if the TestThread container has a lifetime scope, we make a child scope for the feature
                var testThreadScope = args.ObjectContainer.BaseContainer.Resolve<ILifetimeScope>();
                featureScope = testThreadScope.BeginLifetimeScope(nameof(FeatureContext));
            }

            if (featureScope != null)
            {
                args.ObjectContainer.RegisterInstanceAs(featureScope);
            }
        };

        runtimePluginEvents.CustomizeScenarioDependencies += (_, args) =>
        {
            args.ObjectContainer.RegisterFactoryAs<IComponentContext>(() =>
            {
                var containerBuilderFinder = args.ObjectContainer.Resolve<IContainerBuilderFinder>();

                var featureScope = GetFeatureScope(args.ObjectContainer, containerBuilderFinder);

                if (featureScope != null)
                {
                    return featureScope.BeginLifetimeScope(nameof(ScenarioContext), containerBuilder =>
                    {
                        var configureScenarioContainer = containerBuilderFinder.GetConfigureScenarioContainer();

                        if (configureScenarioContainer != null)
                        {
                            containerBuilder = configureScenarioContainer(containerBuilder);
                        }

                        RegisterReqnrollDependencies(args.ObjectContainer, containerBuilder);
                    });
                }

                var legacyCreateScenarioContainerBuilder = containerBuilderFinder.GetLegacyCreateScenarioContainerBuilder();
                if (legacyCreateScenarioContainerBuilder == null)
                {
                    throw new Exception("Unable to find scenario dependencies! Mark a static method that has a ContainerBuilder parameter and returns void with [ScenarioDependencies]!");
                }

                var containerBuilder = legacyCreateScenarioContainerBuilder(null);
                RegisterReqnrollDependencies(args.ObjectContainer, containerBuilder);
                args.ObjectContainer.RegisterFactoryAs(() => containerBuilder.Build());
                return args.ObjectContainer.Resolve<IContainer>().BeginLifetimeScope(nameof(ScenarioContext));
            });
        };
    }

    protected virtual void RegisterTestRunPluginTypes(ObjectContainer testRunContainer)
    {
        testRunContainer.RegisterTypeAs<AutofacTestObjectResolver, ITestObjectResolver>();
        testRunContainer.RegisterTypeAs<ContainerBuilderFinder, IContainerBuilderFinder>();
        testRunContainer.RegisterTypeAs<ConfigurationMethodsProvider, IConfigurationMethodsProvider>();
    }

    private static ILifetimeScope GetFeatureScope(ObjectContainer objectContainer, IContainerBuilderFinder containerBuilderFinder)
    {
        if (objectContainer.BaseContainer.IsRegistered<ILifetimeScope>())
        {
            return objectContainer.BaseContainer.Resolve<ILifetimeScope>();
        }

        var configureScenarioContainer = containerBuilderFinder.GetConfigureScenarioContainer();

        if (configureScenarioContainer != null)
        {
            var containerBuilder = new global::Autofac.ContainerBuilder();
            objectContainer.RegisterFactoryAs(() => containerBuilder.Build());
            return objectContainer.Resolve<IContainer>();
        }

        return null;
    }


    /// <summary>
    ///     The test objects might be dependent on particular Reqnroll infrastructure, therefore the implemented
    ///     resolution logic should support resolving the following objects (from the provided Reqnroll container):
    ///     <see cref="ScenarioContext" />, <see cref="FeatureContext" />, <see cref="TestThreadContext" /> and
    ///     <see cref="IObjectContainer" /> (to be able to resolve any other Reqnroll infrastructure). So basically
    ///     the resolution of these classes has to be forwarded to the original container.
    /// </summary>
    /// <param name="objectContainer">Reqnroll DI container.</param>
    /// <param name="containerBuilder">Autofac ContainerBuilder.</param>
    private void RegisterReqnrollDependencies(
        IObjectContainer objectContainer,
        global::Autofac.ContainerBuilder containerBuilder)
    {
        containerBuilder.Register(_ => objectContainer).As<IObjectContainer>();
        containerBuilder.Register(
            ctx =>
            {
                var reqnrollContainer = ctx.Resolve<IObjectContainer>();
                var scenarioContext = reqnrollContainer.Resolve<ScenarioContext>();
                return scenarioContext;
            }).As<ScenarioContext>();
        containerBuilder.Register(
            ctx =>
            {
                var reqnrollContainer = ctx.Resolve<IObjectContainer>();
                var scenarioContext = reqnrollContainer.Resolve<FeatureContext>();
                return scenarioContext;
            }).As<FeatureContext>();
        containerBuilder.Register(
            ctx =>
            {
                var reqnrollContainer = ctx.Resolve<IObjectContainer>();
                var scenarioContext = reqnrollContainer.Resolve<TestThreadContext>();
                return scenarioContext;
            }).As<TestThreadContext>();
        containerBuilder.Register(
            ctx =>
            {
                var reqnrollContainer = ctx.Resolve<TestThreadContext>();
                var scenarioContext = reqnrollContainer.TestThreadContainer.Resolve<IReqnrollOutputHelper>();
                return scenarioContext;
            }).As<IReqnrollOutputHelper>();
    }
}