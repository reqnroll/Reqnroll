using System;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using System.Globalization;
using Reqnroll.Infrastructure;

namespace Reqnroll.RuntimeTests
{
    public static class TestObjectFactories
    {
        internal static TestRunner CreateTestRunner(out IObjectContainer createThreadContainer, Action<IObjectContainer> registerTestThreadMocks = null, Action<IObjectContainer> registerGlobalMocks = null, IDefaultDependencyProvider defaultDependencyProvider = null)
        {
            createThreadContainer = CreateDefaultTestThreadContainer(registerTestThreadMocks: registerTestThreadMocks, registerGlobalMocks: registerGlobalMocks, defaultDependencyProvider: defaultDependencyProvider);
            return (TestRunner)createThreadContainer.Resolve<ITestRunner>();
        }

        internal static TestRunner CreateTestRunner(Action<IObjectContainer> registerTestThreadMocks = null, Action<IObjectContainer> registerGlobalMocks = null, IDefaultDependencyProvider defaultDependencyProvider = null)
        {
            return CreateTestRunner(out _, registerTestThreadMocks, registerGlobalMocks, defaultDependencyProvider);
        }

        internal static IObjectContainer CreateDefaultGlobalContainer(IRuntimeConfigurationProvider configurationProvider = null, Action<IObjectContainer> registerGlobalMocks = null, IDefaultDependencyProvider defaultDependencyProvider = null)
        {
            return CreateDefaultGlobalContainer(configurationProvider, registerGlobalMocks, new RuntimeTestsContainerBuilder(defaultDependencyProvider));
        }

        internal static IObjectContainer CreateDefaultTestThreadContainer(IRuntimeConfigurationProvider configurationProvider = null, Action<IObjectContainer> registerGlobalMocks = null, Action<IObjectContainer> registerTestThreadMocks = null, IDefaultDependencyProvider defaultDependencyProvider = null)
        {
            return CreateDefaultTestThreadContainer(configurationProvider, registerGlobalMocks, registerTestThreadMocks, new RuntimeTestsContainerBuilder(defaultDependencyProvider));
        }

        internal static IObjectContainer CreateDefaultFeatureContainer(IRuntimeConfigurationProvider configurationHolder, IDefaultDependencyProvider defaultDependencyProvider = null)
        {
            var instance = new RuntimeTestsContainerBuilder(defaultDependencyProvider);
            var testThreadContainer = CreateDefaultTestThreadContainer(configurationHolder, null, null, instance);

            CultureInfo cultureInfo = new CultureInfo("en-US", false);
            return instance.CreateFeatureContainer(testThreadContainer, new FeatureInfo(cultureInfo, "test feature path", "test feature info", "", ProgrammingLanguage.CSharp));
        }

        private static IObjectContainer CreateDefaultGlobalContainer(IRuntimeConfigurationProvider configurationProvider, Action<IObjectContainer> registerGlobalMocks, ContainerBuilder instance)
        {
            var globalContainer = instance.CreateGlobalContainer(typeof(TestObjectFactories).Assembly, configurationProvider);
            registerGlobalMocks?.Invoke(globalContainer);
            return globalContainer;
        }

        private static IObjectContainer CreateDefaultTestThreadContainer(IRuntimeConfigurationProvider configurationProvider, Action<IObjectContainer> registerGlobalMocks, Action<IObjectContainer> registerTestThreadMocks, ContainerBuilder instance)
        {
            var globalContainer = CreateDefaultGlobalContainer(configurationProvider, registerGlobalMocks, instance);
            var testThreadContainer = instance.CreateTestThreadContainer(globalContainer);
            registerTestThreadMocks?.Invoke(testThreadContainer);
            return testThreadContainer;
        }
    }
}
