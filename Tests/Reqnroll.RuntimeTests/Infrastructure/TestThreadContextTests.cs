using Reqnroll.BoDi;
using FluentAssertions;
using Moq;
using Xunit;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    
    public class TestThreadContextTests : StepExecutionTestsBase
    {
        public ContextManager CreateContextManager(IObjectContainer testThreadContainer = null)
        {
            return new ContextManager(new Mock<ITestTracer>().Object, testThreadContainer ?? TestThreadContainer, ContainerBuilderStub);
        }

        [Fact]
        public void Should_ContextManager_initialize_TestThreadContext_when_constructed()
        {
            var contextManager = CreateContextManager();

            TestThreadContext result = contextManager.TestThreadContext;

            result.Should().NotBeNull();
            result.Should().BeSameAs(TestThreadContainer.Resolve<TestThreadContext>());
        }

        [Fact]
        public void Should_expose_the_test_thread_container()
        {
            TestThreadContext result = ContextManagerStub.TestThreadContext;

            result.Should().NotBeNull();
            result.TestThreadContainer.Should().BeSameAs(TestThreadContainer);
        }

        [Fact]
        public async Task Should_disposing_event_fired_when_test_thread_container_disposes()
        {
            bool wasDisposingFired = false;
            ContextManagerStub.TestThreadContext.Should().NotBeNull();
            ContextManagerStub.TestThreadContext.Disposing += context =>
            {
                context.Should().BeSameAs(ContextManagerStub.TestThreadContext);
                wasDisposingFired = true;
            };

            await TestThreadContainer.DisposeAsync();

            wasDisposingFired.Should().BeTrue();
        }

        [Fact]
        public async Task Should_be_able_to_resolve_from_scenario_container()
        {
            // this basically tests the special registration in DefaultDependencyProvider

            var containerBuilder = new RuntimeTestsContainerBuilder();
            var testThreadContainer = containerBuilder.CreateTestThreadContainer(containerBuilder.CreateGlobalContainer(typeof(TestThreadContextTests).Assembly));
            var contextManager = CreateContextManager(testThreadContainer);
            await contextManager.InitializeFeatureContextAsync(new FeatureInfo(FeatureLanguage, "", "test feature", null));
            await contextManager.InitializeScenarioContextAsync(new ScenarioInfo("test scenario", "test_description", null, null), null);

            contextManager.TestThreadContext.Should().NotBeNull();

            var ctxFromFeatureContext = contextManager.FeatureContext.FeatureContainer.Resolve<TestThreadContext>();
            ctxFromFeatureContext.Should().BeSameAs(contextManager.TestThreadContext);

            var ctxFromScenarioContext = contextManager.ScenarioContext.ScenarioContainer.Resolve<TestThreadContext>();
            ctxFromScenarioContext.Should().BeSameAs(contextManager.TestThreadContext);
        }
    }
}
