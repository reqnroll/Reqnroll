using FluentAssertions;
using Moq;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using System.Globalization;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure;

public class ContextManagerTests : StepExecutionTestsBase
{
    public ContextManager CreateContextManager(IObjectContainer testThreadContainer = null)
    {
        return new ContextManager(new Mock<ITestTracer>().Object, testThreadContainer ?? TestThreadContainer, ContainerBuilderStub);
    }

    private static void InitializeFeatureContext(ContextManager sut)
    {
        sut.InitializeFeatureContext(new FeatureInfo(new CultureInfo("en-US", false), string.Empty, "F", null));
    }

    private void InitializeScenarioContext(ContextManager sut)
    {
        InitializeFeatureContext(sut);
        sut.InitializeScenarioContext(new ScenarioInfo("the next scenario", "description of the next scenario", null, null), null);
    }

    [Fact]
    public void Should_register_scenario_context_to_scenario_container()
    {
        var sut = CreateContextManager();

        InitializeScenarioContext(sut);

        var scenarioContext = sut.ScenarioContext;
        scenarioContext.Should().NotBeNull();

        var scenarioContainer = scenarioContext.ScenarioContainer;

        scenarioContainer.Resolve<ScenarioContext>().Should().BeSameAs(scenarioContext);
        scenarioContainer.Resolve<IScenarioContext>().Should().BeSameAs(scenarioContext);
    }

    [Fact]
    public void Should_dispose_scenario_context_when_scenario_context_cleaned_up()
    {
        var sut = CreateContextManager();

        InitializeScenarioContext(sut);

        var scenarioContext = sut.ScenarioContext;
        scenarioContext.Should().NotBeNull();

        sut.CleanupScenarioContext();

        scenarioContext.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Should_register_feature_context_to_feature_container()
    {
        var sut = CreateContextManager();

        InitializeFeatureContext(sut);

        var featureContext = sut.FeatureContext;
        featureContext.Should().NotBeNull();

        var featureContainer = featureContext.FeatureContainer;

        featureContainer.Resolve<FeatureContext>().Should().BeSameAs(featureContext);
        featureContainer.Resolve<IFeatureContext>().Should().BeSameAs(featureContext);
    }

    [Fact]
    public void Should_be_able_to_resolve_feature_context_from_scenario_container()
    {
        var sut = CreateContextManager();

        InitializeScenarioContext(sut);

        var featureContext = sut.FeatureContext;
        featureContext.Should().NotBeNull();

        var scenarioContainer = sut.ScenarioContext.ScenarioContainer;
        scenarioContainer.Resolve<FeatureContext>().Should().BeSameAs(featureContext);
        scenarioContainer.Resolve<IFeatureContext>().Should().BeSameAs(featureContext);
    }

    [Fact]
    public void Should_dispose_feature_context_when_feature_context_cleaned_up()
    {
        var sut = CreateContextManager();

        InitializeFeatureContext(sut);

        var featureContext = sut.FeatureContext;
        featureContext.Should().NotBeNull();

        sut.CleanupFeatureContext();

        featureContext.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Should_register_test_thread_context_to_test_thread_container()
    {
        var sut = CreateContextManager();
        // the test thread context is already initialized in the constructor of ContextManager

        var testThreadContext = sut.TestThreadContext;
        testThreadContext.Should().NotBeNull();

        var testThreadContainer = testThreadContext.TestThreadContainer;

        testThreadContainer.Resolve<TestThreadContext>().Should().BeSameAs(testThreadContext);
        testThreadContainer.Resolve<ITestThreadContext>().Should().BeSameAs(testThreadContext);
    }

    [Fact]
    public void Should_be_able_to_resolve_test_thread_context_from_feature_container()
    {
        var sut = CreateContextManager();

        InitializeFeatureContext(sut);

        var testThreadContext = sut.TestThreadContext;
        testThreadContext.Should().NotBeNull();

        var featureContainer = sut.FeatureContext.FeatureContainer;
        featureContainer.Resolve<TestThreadContext>().Should().BeSameAs(testThreadContext);
        featureContainer.Resolve<ITestThreadContext>().Should().BeSameAs(testThreadContext);
    }

    [Fact]
    public void Should_be_able_to_resolve_test_thread_context_from_scenario_container()
    {
        var sut = CreateContextManager();

        InitializeScenarioContext(sut);

        var testThreadContext = sut.TestThreadContext;
        testThreadContext.Should().NotBeNull();

        var scenarioContainer = sut.ScenarioContext.ScenarioContainer;
        scenarioContainer.Resolve<TestThreadContext>().Should().BeSameAs(testThreadContext);
        scenarioContainer.Resolve<ITestThreadContext>().Should().BeSameAs(testThreadContext);
    }

    [Fact]
    public void Should_dispose_test_thread_context_when_test_thread_context_cleaned_up()
    {
        var sut = CreateContextManager();
        // the test thread context is already initialized in the constructor of ContextManager

        var testThreadContext = sut.TestThreadContext;
        testThreadContext.Should().NotBeNull();

        var testThreadContainer = testThreadContext.TestThreadContainer;
        testThreadContainer.Dispose();

        testThreadContext.IsDisposed.Should().BeTrue();
    }
}