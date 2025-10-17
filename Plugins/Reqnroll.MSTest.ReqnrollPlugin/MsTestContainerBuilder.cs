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
        private readonly object _testContext; 

        public MsTestContainerBuilder(object testContext, IContainerBuilder innerContainerBuilder = null)
        {
            _testContext = testContext;
            _innerContainerBuilder = innerContainerBuilder ?? new ContainerBuilder();
        }

        public IObjectContainer CreateGlobalContainer(Assembly testAssembly, IRuntimeConfigurationProvider configurationProvider = null)
        {
            var container = _innerContainerBuilder.CreateGlobalContainer(testAssembly, configurationProvider);
            var testContextClass = GetTestContextType();
            container.RegisterInstanceAs(_testContext, testContextClass);

            return container;
        }

        //TODO: temporary until we have a proper MSTest plugin abstraction
        internal static Type GetTestContextType()
        {
            var a1 = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
                                                                                a.GetName().Name == "Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions" ||
                                                                                a.GetName().Name == "MSTest.TestFramework.Extensions");
            if (a1 == null)
                throw new InvalidOperationException("Could not find MSTest TestContext type in loaded assemblies.");

            return a1.GetType("Microsoft.VisualStudio.TestTools.UnitTesting.TestContext");
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
