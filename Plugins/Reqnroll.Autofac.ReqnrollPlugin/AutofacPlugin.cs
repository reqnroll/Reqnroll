using System;
using Autofac;
using Reqnroll.Autofac;
using Reqnroll;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(AutofacPlugin))]

namespace Reqnroll.Autofac
{
    using BoDi;

    using Reqnroll;

    public class AutofacPlugin : IRuntimePlugin
    {
        private static readonly Object _registrationLock = new Object();

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                // temporary fix for CustomizeGlobalDependencies called multiple times
                // see https://github.com/reqnroll/Reqnroll/issues/948
                if (!args.ObjectContainer.IsRegistered<IContainerBuilderFinder>())
                {
                    // an extra lock to ensure that there are not two super fast threads re-registering the same stuff
                    lock (_registrationLock)
                    {
                        if (!args.ObjectContainer.IsRegistered<IContainerBuilderFinder>())
                        {
                            args.ObjectContainer.RegisterTypeAs<AutofacTestObjectResolver, ITestObjectResolver>();
                            args.ObjectContainer.RegisterTypeAs<ContainerBuilderFinder, IContainerBuilderFinder>();

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

            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                if (args.ObjectContainer.BaseContainer.IsRegistered<IContainer>())
                {
                    var container = args.ObjectContainer.BaseContainer.Resolve<IContainer>();
                    args.ObjectContainer.RegisterFactoryAs(() => container.BeginLifetimeScope(nameof(TestThreadContext)));
                }
            };

            runtimePluginEvents.CustomizeFeatureDependencies += (sender, args) =>
            {
                var containerBuilderFinder = args.ObjectContainer.Resolve<IContainerBuilderFinder>();

                var featureScopeFinder = containerBuilderFinder.GetFeatureLifetimeScope();

                ILifetimeScope featureScope = null;

                if (featureScopeFinder != null)
                {
                    featureScope = featureScopeFinder();
                }
                else if (args.ObjectContainer.BaseContainer.IsRegistered<ILifetimeScope>())
                {
                    var testThreadScope = args.ObjectContainer.BaseContainer.Resolve<ILifetimeScope>();

                    featureScope = testThreadScope.BeginLifetimeScope(nameof(FeatureContext));
                }

                if (featureScope != null)
                {
                    args.ObjectContainer.RegisterInstanceAs(featureScope);
                }
            };

            runtimePluginEvents.CustomizeScenarioDependencies += (sender, args) =>
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

                            RegisterReqnrollDependecies(args.ObjectContainer, containerBuilder);
                        });
                    }

                    var createScenarioContainerBuilder = containerBuilderFinder.GetCreateScenarioContainerBuilder();
                    if (createScenarioContainerBuilder == null)
                    {
                        throw new Exception("Unable to find scenario dependencies! Mark a static method that has a ContainerBuilder parameter and returns void with [ScenarioDependencies]!");
                    }

                    var containerBuilder = createScenarioContainerBuilder(null);
                    RegisterReqnrollDependecies(args.ObjectContainer, containerBuilder);
                    args.ObjectContainer.RegisterFactoryAs(() => containerBuilder.Build());
                    return args.ObjectContainer.Resolve<IContainer>().BeginLifetimeScope(nameof(ScenarioContext));
                });
            };
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
        ///     Fix for https://github.com/gasparnagy/Reqnroll.Autofac/issues/11 Cannot resolve ScenarioInfo
        ///     Extracted from
        ///     https://github.com/reqnroll/Reqnroll/blob/master/Reqnroll/Infrastructure/ITestObjectResolver.cs
        ///     The test objects might be dependent on particular Reqnroll infrastructure, therefore the implemented
        ///     resolution logic should support resolving the following objects (from the provided Reqnroll container):
        ///     <see cref="ScenarioContext" />, <see cref="FeatureContext" />, <see cref="TestThreadContext" /> and
        ///     <see cref="IObjectContainer" /> (to be able to resolve any other Reqnroll infrastucture). So basically
        ///     the resolution of these classes has to be forwarded to the original container.
        /// </summary>
        /// <param name="objectContainer">Reqnroll DI container.</param>
        /// <param name="containerBuilder">Autofac ContainerBuilder.</param>
        private void RegisterReqnrollDependecies(
            IObjectContainer objectContainer,
            global::Autofac.ContainerBuilder containerBuilder)
        {
            containerBuilder.Register(ctx => objectContainer).As<IObjectContainer>();
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
        }
    }
}
