using System.Reflection;
using Reqnroll.BoDi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.Configuration;
using Reqnroll.Infrastructure;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    public class MsTestContainerBuilder : IContainerBuilder
    {
        private readonly IContainerBuilder _innerContainerBuilder;
        private readonly object _testContext;

        public MsTestContainerBuilder(object testContext, IContainerBuilder innerContainerBuilder = null)
        {
            _testContext = testContext;
            _innerContainerBuilder = innerContainerBuilder ?? new ContainerBuilder();
        }

        public IObjectContainer CreateGlobalContainer(Assembly testAssembly, IRuntimeConfigurationProvider configurationProvider = null)
        {
            var container = _innerContainerBuilder.CreateGlobalContainer(testAssembly, configurationProvider);
            container.RegisterInstanceAs(new TestContextWrapper(_testContext));

            return container;
        }

        public IObjectContainer CreateTestThreadContainer(IObjectContainer globalContainer) => _innerContainerBuilder.CreateTestThreadContainer(globalContainer);

        public IObjectContainer CreateScenarioContainer(IObjectContainer testThreadContainer, ScenarioInfo scenarioInfo)
            => _innerContainerBuilder.CreateScenarioContainer(testThreadContainer, scenarioInfo);

        public IObjectContainer CreateFeatureContainer(IObjectContainer testThreadContainer, FeatureInfo featureInfo)
            => _innerContainerBuilder.CreateFeatureContainer(testThreadContainer, featureInfo);
    }

    public class TestContextWrapper(object testContext)
    {
        public object TestContext { get; } = testContext;
    }
}
