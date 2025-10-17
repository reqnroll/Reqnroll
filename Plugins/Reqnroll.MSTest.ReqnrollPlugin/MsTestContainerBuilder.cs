using System;
using System.Linq;
using System.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Infrastructure;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    public class MsTestContainerBuilder : IContainerBuilder
    {
        private readonly IContainerBuilder _innerContainerBuilder;
        private readonly IMsTestRuntimeAdapter _runtimeAdapter;

        public MsTestContainerBuilder(object testContext, IContainerBuilder innerContainerBuilder = null)
        {
            _innerContainerBuilder = innerContainerBuilder ?? new ContainerBuilder();
            _runtimeAdapter = MsTestRuntimeAdapterSelector.GetAdapter(testContext); //TODO: provide adapter via ctor
        }

        public IObjectContainer CreateGlobalContainer(Assembly testAssembly, IRuntimeConfigurationProvider configurationProvider = null)
        {
            var container = _innerContainerBuilder.CreateGlobalContainer(testAssembly, configurationProvider);
            container.RegisterInstanceAs(_runtimeAdapter);
            _runtimeAdapter.RegisterGlobalTestContext(container);
            return container;
        }


        //TODO: temporary until we have a proper MSTest plugin abstraction
        internal static Type GetAssertInconclusiveExceptionType()
        {
            var a1 = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
                                                                                a.GetName().Name == "Microsoft.VisualStudio.TestPlatform.TestFramework" ||
                                                                                a.GetName().Name == "MSTest.TestFramework");
            if (a1 == null)
                throw new InvalidOperationException("Could not find MSTest Assert type in loaded assemblies.");

            return a1.GetType("Microsoft.VisualStudio.TestTools.UnitTesting.AssertInconclusiveException")!;
        }

        public IObjectContainer CreateTestThreadContainer(IObjectContainer globalContainer) => _innerContainerBuilder.CreateTestThreadContainer(globalContainer);

        public IObjectContainer CreateScenarioContainer(IObjectContainer testThreadContainer, ScenarioInfo scenarioInfo)
            => _innerContainerBuilder.CreateScenarioContainer(testThreadContainer, scenarioInfo);

        public IObjectContainer CreateFeatureContainer(IObjectContainer testThreadContainer, FeatureInfo featureInfo)
            => _innerContainerBuilder.CreateFeatureContainer(testThreadContainer, featureInfo);
    }
}
