using Reqnroll.BoDi;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Reqnroll.Windsor;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(WindsorPlugin))]

namespace Reqnroll.Windsor
{
    public class WindsorPlugin : IRuntimePlugin
    {
        private static object _registrationLock = new object();

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                // temporary fix for CustomizeGlobalDependencies called multiple times
                // see https://github.com/reqnroll/Reqnroll/issues/948
                if (!args.ObjectContainer.IsRegistered<IContainerFinder>())
                {
                    // an extra lock to ensure that there are not two super fast threads re-registering the same stuff
                    lock (_registrationLock)
                    {
                        if (!args.ObjectContainer.IsRegistered<IContainerFinder>())
                        {
                            args.ObjectContainer.RegisterTypeAs<WindsorTestObjectResolver, ITestObjectResolver>();
                            args.ObjectContainer.RegisterTypeAs<ContainerFinder, IContainerFinder>();
                        }
                    }

                    // workaround for parallel execution issue - this should be rather a feature in BoDi?
                    args.ObjectContainer.Resolve<IContainerFinder>();
                }
            };

            runtimePluginEvents.CustomizeScenarioDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterFactoryAs(() =>
                {
                    var finder = args.ObjectContainer.Resolve<IContainerFinder>();
                    var containerBuilder = finder.GetCreateScenarioContainer();

                    var container = containerBuilder();

                    RegisterReqnrollDependecies(args.ObjectContainer, container);

                    return container;
                });
            };
        }

        public void RegisterReqnrollDependecies(IObjectContainer objectContainer, IWindsorContainer container)
        {
            container.Register(Component.For<IObjectContainer>().Instance(objectContainer).LifestyleScoped());

            RegisterContext<ScenarioContext>(objectContainer, container);
            RegisterContext<FeatureContext>(objectContainer, container);
            RegisterContext<TestThreadContext>(objectContainer, container);
        }

        private void RegisterContext<T>(IObjectContainer objectContainer, IWindsorContainer container)
            where T : class
        {
            container.Register(Component.For<T>().UsingFactoryMethod(objectContainer.Resolve<T>, true));
        }
    }
}
