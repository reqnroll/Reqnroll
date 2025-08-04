using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core.Registration;
using FluentAssertions;
using Moq;
using Reqnroll.Autofac;
using Reqnroll.Autofac.ReqnrollPlugin;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using Xunit;

namespace Reqnroll.PluginTests.Autofac;
public class AutofacPluginTests
{
    class TestableAutofacPlugin : AutofacPlugin
    {
        private readonly IConfigurationMethodsProvider _configurationMethodsProvider;

        public TestableAutofacPlugin(Type configType, params string[] configMethodNames)
        {
            var configMethods = configType.GetMethods().AsEnumerable();
            if (configMethodNames.Any())
            {
                configMethods = configMethods.Where(m => configMethodNames.Contains(m.Name));
            }
            var configurationMethodsProviderMock = new Mock<IConfigurationMethodsProvider>();
            configurationMethodsProviderMock.Setup(m => m.GetConfigurationMethods()).Returns(configMethods.ToArray());
            _configurationMethodsProvider = configurationMethodsProviderMock.Object;
        }

        protected override void RegisterTestRunPluginTypes(ObjectContainer testRunContainer)
        {
            base.RegisterTestRunPluginTypes(testRunContainer);
            testRunContainer.RegisterInstanceAs(_configurationMethodsProvider);
        }
    }

    interface IScenarioDependency1;
    class ScenarioDependency1 : IScenarioDependency1;
    class ScenarioDependency2(IGlobalDependency1 globalDependency1) : IScenarioDependency1
    {
        public IGlobalDependency1 CurrentGlobalDependency1 { get; } = globalDependency1;
    }

    interface IGlobalDependency1;
    class GlobalDependency1 : IGlobalDependency1;

    public class ContainerSetup1
    {
        [GlobalDependencies]
        public static void SetupGlobalContainer(global::Autofac.ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<GlobalDependency1>()
                .As<IGlobalDependency1>()
                .SingleInstance();
        }

        [ScenarioDependencies]
        public static void SetupScenarioContainer(global::Autofac.ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<ScenarioDependency1>()
                .As<IScenarioDependency1>()
                .SingleInstance();
        }

        [ScenarioDependencies]
        public static void SetupScenarioContainerWithGlobalDep(global::Autofac.ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<ScenarioDependency2>()
                .As<IScenarioDependency1>()
                .SingleInstance();
        }
    }

    public class ContainerSetup2
    {
        private static readonly ILifetimeScope _existingGlobalScope;

        static ContainerSetup2()
        {
            var containerBuilder = new global::Autofac.ContainerBuilder();
            ContainerSetup1.SetupGlobalContainer(containerBuilder);
            _existingGlobalScope = containerBuilder.Build();
        }

        [FeatureDependencies]
        public static ILifetimeScope GetFeatureLifetimeScope()
        {
            return _existingGlobalScope.BeginLifetimeScope();
        }

        [ScenarioDependencies]
        public static void SetupScenarioContainer(global::Autofac.ContainerBuilder containerBuilder)
        {
            ContainerSetup1.SetupScenarioContainer(containerBuilder);
        }
    }

    private readonly RuntimePluginEvents _runtimePluginEvents;
    private readonly ObjectContainer _testRunContainer;
    private readonly ObjectContainer _testThreadContainer;
    private readonly ObjectContainer _featureContainer;

    public AutofacPluginTests()
    {
        _runtimePluginEvents = new RuntimePluginEvents();
        _testRunContainer = new ObjectContainer();
        var testAssemblyProviderMock = new Mock<ITestAssemblyProvider>();
        testAssemblyProviderMock.SetupGet(tap => tap.TestAssembly).Returns(Assembly.GetExecutingAssembly());
        _testRunContainer.RegisterInstanceAs(testAssemblyProviderMock.Object);

        _testThreadContainer = new ObjectContainer(_testRunContainer);
        _featureContainer = new ObjectContainer(_testThreadContainer);
    }

    [Fact]
    public void Should_register_AutofacTestObjectResolver()
    {
        // Arrange
        var sut = new AutofacPlugin();
        
        // Act
        sut.Initialize(_runtimePluginEvents, new RuntimePluginParameters(), new UnitTestProviderConfiguration());
        var reqnrollConfiguration = ConfigurationLoader.GetDefault();
        _runtimePluginEvents.RaiseCustomizeGlobalDependencies(_testRunContainer, reqnrollConfiguration);

        // Assert
        _testRunContainer.IsRegistered<ITestObjectResolver>().Should().BeTrue();
        _testRunContainer.Resolve<ITestObjectResolver>().Should().BeOfType<AutofacTestObjectResolver>();
    }

    [Fact]
    public void Should_allow_global_registrations_to_TestThreadContainer()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup1), nameof(ContainerSetup1.SetupGlobalContainer));
        
        // Act
        sut.Initialize(_runtimePluginEvents, new RuntimePluginParameters(), new UnitTestProviderConfiguration());
        var reqnrollConfiguration = ConfigurationLoader.GetDefault();
        _runtimePluginEvents.RaiseCustomizeGlobalDependencies(_testRunContainer, reqnrollConfiguration);

        var testThreadContainer = new ObjectContainer(_testRunContainer);
        _runtimePluginEvents.RaiseCustomizeTestThreadDependencies(testThreadContainer);

        // Assert
        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();
        var globalDep1 = resolver.ResolveBindingInstance(typeof(IGlobalDependency1), testThreadContainer);
        globalDep1.Should().NotBeNull();
    }

    [Fact] public void Should_allow_global_registrations_to_GlobalContainer()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup1), nameof(ContainerSetup1.SetupGlobalContainer));
        
        // Act
        sut.Initialize(_runtimePluginEvents, new RuntimePluginParameters(), new UnitTestProviderConfiguration());
        var reqnrollConfiguration = ConfigurationLoader.GetDefault();
        _runtimePluginEvents.RaiseCustomizeGlobalDependencies(_testRunContainer, reqnrollConfiguration);

        // Assert
        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();
        var globalDep1 = resolver.ResolveBindingInstance(typeof(IGlobalDependency1), _testRunContainer);
        globalDep1.Should().NotBeNull();
    }

    private ObjectContainer InitializeToScenarioContainer(TestableAutofacPlugin sut)
    {
        sut.Initialize(_runtimePluginEvents, new RuntimePluginParameters(), new UnitTestProviderConfiguration());
        var reqnrollConfiguration = ConfigurationLoader.GetDefault();
        _runtimePluginEvents.RaiseCustomizeGlobalDependencies(_testRunContainer, reqnrollConfiguration);
        _runtimePluginEvents.RaiseCustomizeTestThreadDependencies(_testThreadContainer);
        _runtimePluginEvents.RaiseCustomizeFeatureDependencies(_featureContainer);

        var scenarioContainer = new ObjectContainer(_featureContainer);
        _runtimePluginEvents.RaiseCustomizeScenarioDependencies(scenarioContainer);

        return scenarioContainer;
    }

    [Fact]
    public void Should_allow_scenario_specific_registrations()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup1), 
            nameof(ContainerSetup1.SetupGlobalContainer),
            nameof(ContainerSetup1.SetupScenarioContainer));

        // Act
        var scenarioContainer = InitializeToScenarioContainer(sut);

        // Assert
        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();
        var scenarioDep1 = resolver.ResolveBindingInstance(typeof(IScenarioDependency1), scenarioContainer);
        scenarioDep1.Should().NotBeNull();

        var globalDep1 = resolver.ResolveBindingInstance(typeof(IGlobalDependency1), scenarioContainer);
        globalDep1.Should().NotBeNull();
    }

    [Fact]
    public void Should_allow_scenario_specific_registrations_without_global_registrations()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup1), 
            nameof(ContainerSetup1.SetupScenarioContainer));

        // Act
        var scenarioContainer = InitializeToScenarioContainer(sut);

        // Assert
        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();
        var scenarioDep1 = resolver.ResolveBindingInstance(typeof(IScenarioDependency1), scenarioContainer);
        scenarioDep1.Should().NotBeNull();

        FluentActions.Invoking(() => resolver.ResolveBindingInstance(typeof(IGlobalDependency1), scenarioContainer))
                     .Should()
                     .Throw<ComponentNotRegisteredException>();
    }


    [Fact]
    public void Should_allow_resolving_common_reqnroll_objects_from_scenario_container()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup1),
                                            nameof(ContainerSetup1.SetupGlobalContainer),
                                            nameof(ContainerSetup1.SetupScenarioContainer));

        // Act
        var scenarioContainer = InitializeToScenarioContainer(sut);
        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();
        var scenarioContext = new ScenarioContext(scenarioContainer, new ScenarioInfo("", "", Array.Empty<string>(), new OrderedDictionary()), null, resolver);
        scenarioContainer.RegisterInstanceAs(scenarioContext);
        var featureContext = new FeatureContext(scenarioContainer, new FeatureInfo(CultureInfo.CurrentCulture, "", "", ""), ConfigurationLoader.GetDefault());
        _featureContainer.RegisterInstanceAs(featureContext);
        var testThreadContext = new TestThreadContext(_testThreadContainer);
        _testThreadContainer.RegisterInstanceAs(testThreadContext);

        // Assert
        var resolvedContainer = resolver.ResolveBindingInstance(typeof(IObjectContainer), scenarioContainer);
        resolvedContainer.Should().BeSameAs(scenarioContainer);

        var resolvedScenarioContext = resolver.ResolveBindingInstance(typeof(ScenarioContext), scenarioContainer);
        resolvedScenarioContext.Should().BeSameAs(scenarioContext);

        var resolvedFeatureContext = resolver.ResolveBindingInstance(typeof(FeatureContext), scenarioContainer);
        resolvedFeatureContext.Should().BeSameAs(featureContext);

        var resolvedTestThreadContext = resolver.ResolveBindingInstance(typeof(TestThreadContext), scenarioContainer);
        resolvedTestThreadContext.Should().BeSameAs(testThreadContext);
    }

    [Fact]
    public void Should_allow_resolving_common_reqnroll_objects_from_feature_container()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup1),
                                            nameof(ContainerSetup1.SetupGlobalContainer),
                                            nameof(ContainerSetup1.SetupScenarioContainer));

        // Act
        InitializeToScenarioContainer(sut);
        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();
        var featureContext = new FeatureContext(_featureContainer, new FeatureInfo(CultureInfo.CurrentCulture, "", "", ""), ConfigurationLoader.GetDefault());
        _featureContainer.RegisterInstanceAs(featureContext);
        var testThreadContext = new TestThreadContext(_testThreadContainer);
        _testThreadContainer.RegisterInstanceAs(testThreadContext);

        // Assert
        var resolvedContainer = resolver.ResolveBindingInstance(typeof(IObjectContainer), _featureContainer);
        resolvedContainer.Should().BeSameAs(_featureContainer);

        var resolvedFeatureContext = resolver.ResolveBindingInstance(typeof(FeatureContext), _featureContainer);
        resolvedFeatureContext.Should().BeSameAs(featureContext);

        var resolvedTestThreadContext = resolver.ResolveBindingInstance(typeof(TestThreadContext), _featureContainer);
        resolvedTestThreadContext.Should().BeSameAs(testThreadContext);
    }


    [Fact]
    public void Should_allow_having_scenario_dependency_that_depends_on_a_global()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup1),
                                            nameof(ContainerSetup1.SetupGlobalContainer),
                                            nameof(ContainerSetup1.SetupScenarioContainerWithGlobalDep));

        // Act
        var scenarioContainer = InitializeToScenarioContainer(sut);

        // Assert


        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();
        var scenarioDep1 = resolver.ResolveBindingInstance(typeof(IScenarioDependency1), scenarioContainer);
        var dep2 = scenarioDep1.Should().BeOfType<ScenarioDependency2>().Subject;

        var globalDep1 = resolver.ResolveBindingInstance(typeof(IGlobalDependency1), scenarioContainer);
        globalDep1.Should().NotBeNull();

        dep2.CurrentGlobalDependency1.Should().BeSameAs(globalDep1);
    }

    [Fact]
    public void Should_allow_using_existing_global_scope()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup2));

        // Act
        var scenarioContainer = InitializeToScenarioContainer(sut);

        // Assert
        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();
        var scenarioDep1 = resolver.ResolveBindingInstance(typeof(IScenarioDependency1), scenarioContainer);
        scenarioDep1.Should().NotBeNull();

        var globalDep1 = resolver.ResolveBindingInstance(typeof(IGlobalDependency1), scenarioContainer);
        globalDep1.Should().NotBeNull();
    }

    [Fact]
    public void Should_register_IReqnrollOutputHelper()
    {
        // Arrange
        var sut = new TestableAutofacPlugin(typeof(ContainerSetup1),
                                            nameof(ContainerSetup1.SetupGlobalContainer),
                                            nameof(ContainerSetup1.SetupScenarioContainer));

        // Act
        var scenarioContainer = InitializeToScenarioContainer(sut);

        var resolver = _testRunContainer.Resolve<ITestObjectResolver>();

        var testThreadContext =
            new TestThreadContext(_testThreadContainer);

        _testThreadContainer.RegisterInstanceAs(testThreadContext);

        var traceListenerMock =
            new Mock<ITraceListener>();
        var attachmentHandlerMock =
            new Mock<IReqnrollAttachmentHandler>();
        var threadExecutionMock =
            new Mock<ITestThreadExecutionEventPublisher>();
        var contextManagerMock =
            new Mock<IContextManager>();

        var defaultDependencyProvider = new DefaultDependencyProvider();
        defaultDependencyProvider
            .RegisterTestThreadContainerDefaults(_testThreadContainer);

        _testThreadContainer
            .RegisterInstanceAs(traceListenerMock.Object);
        _testThreadContainer
            .RegisterInstanceAs(attachmentHandlerMock.Object);
        _testThreadContainer
            .RegisterInstanceAs(threadExecutionMock.Object);
        _testThreadContainer
            .RegisterInstanceAs(contextManagerMock.Object);

        // Assert
        var resolvedOutputHelper = resolver
            .ResolveBindingInstance(typeof(IReqnrollOutputHelper), scenarioContainer);

        resolvedOutputHelper
            .Should()
            .NotBeNull();
    }
}
