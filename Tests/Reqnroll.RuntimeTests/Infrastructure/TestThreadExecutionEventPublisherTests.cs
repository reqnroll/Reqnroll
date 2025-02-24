using System;
using System.Threading.Tasks;
using NSubstitute;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Xunit;
//TODO NSub

namespace Reqnroll.RuntimeTests.Infrastructure
{
    public partial class TestExecutionEngineTests
    {
        [Fact]
        public async Task Should_publish_step_started_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<StepStartedEvent>(e => e.ScenarioContext.Equals(scenarioContext) && e.FeatureContext.Equals(featureContainer.Resolve<FeatureContext>()) && e.StepContext.Equals(contextManagerStub.StepContext)));
        }

        [Fact]
        public async Task Should_publish_step_binding_started_event()
        {
            var stepDef = RegisterStepDefinition();
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<StepBindingStartedEvent>(e => e.StepDefinitionBinding.Equals(stepDef)));
        }

        [Fact]
        public async Task Should_publish_step_binding_finished_event()
        {
            var stepDef = RegisterStepDefinition();
            TimeSpan expectedDuration = TimeSpan.FromSeconds(5);
            methodBindingInvokerMock.InvokeBindingAsync(stepDef, contextManagerStub, Arg.Any<object[]>(), testTracerStub, Arg.Any<DurationHolder>())
                                    .Returns(callInfo =>
                                    {
                                        callInfo.Arg<DurationHolder>().Duration = expectedDuration;
                                        return new object();
                                    });
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<StepBindingFinishedEvent>(e => e.StepDefinitionBinding.Equals(stepDef) && e.Duration.Equals(expectedDuration)));
        }

        [Fact]
        public async Task Should_publish_step_finished_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<StepFinishedEvent>(e => e.ScenarioContext.Equals(scenarioContext) && e.FeatureContext.Equals(featureContainer.Resolve<FeatureContext>()) && e.StepContext.Equals(contextManagerStub.StepContext)));
        }

        [Fact]
        public async Task Should_publish_step_skipped_event()
        {
            RegisterStepDefinition();
            var testExecutionEngine = CreateTestExecutionEngine();
            //a step will be skipped if the ScenarioExecutionStatus is not OK
            scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Any<StepSkippedEvent>());
        }

        [Fact]
        public async Task Should_publish_hook_binding_events()
        {
            var hookType = HookType.AfterScenario;
            TimeSpan expectedDuration = TimeSpan.FromSeconds(5);
            var expectedHookBinding = new HookBinding(Substitute.For<IBindingMethod>(), hookType, null, 1);
            methodBindingInvokerMock
                .InvokeBindingAsync(expectedHookBinding, contextManagerStub, Arg.Any<object[]>(), testTracerStub, Arg.Any<DurationHolder>())
                .Returns(callInfo =>
                {
                    callInfo.Arg<DurationHolder>().Duration = expectedDuration;
                    return new object();
                });
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.InvokeHookAsync(methodBindingInvokerMock, expectedHookBinding, hookType);

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<HookBindingStartedEvent>(e => e.HookBinding.Equals(expectedHookBinding)));
            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<HookBindingFinishedEvent>(e => e.HookBinding.Equals(expectedHookBinding) && e.Duration.Equals(expectedDuration)));
        }

        [Fact]
        public async Task Should_publish_scenario_started_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            testExecutionEngine.OnScenarioInitialize(scenarioInfo);
            await testExecutionEngine.OnScenarioStartAsync();

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<ScenarioStartedEvent>(e => e.ScenarioContext.Equals(scenarioContext) && e.FeatureContext.Equals(featureContainer.Resolve<FeatureContext>())));
        }

        [Fact]
        public async Task Should_publish_scenario_finished_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            testExecutionEngine.OnScenarioInitialize(scenarioInfo);
            await testExecutionEngine.OnScenarioStartAsync();
            await testExecutionEngine.OnAfterLastStepAsync();
            await testExecutionEngine.OnScenarioEndAsync();

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<ScenarioFinishedEvent>(e => e.ScenarioContext.Equals(scenarioContext) && e.FeatureContext.Equals(featureContainer.Resolve<FeatureContext>())));
        }

        [Fact]
        public async Task Should_publish_hook_started_finished_events()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            await testExecutionEngine.OnTestRunStartAsync();
            await testExecutionEngine.OnFeatureStartAsync(featureInfo);

            testExecutionEngine.OnScenarioInitialize(scenarioInfo);
            await testExecutionEngine.OnScenarioStartAsync();
            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
            await testExecutionEngine.OnAfterLastStepAsync();
            await testExecutionEngine.OnScenarioEndAsync();

            await testExecutionEngine.OnFeatureEndAsync();
            await testExecutionEngine.OnTestRunEndAsync();

            AssertHookEventsForHookType(HookType.BeforeTestRun);
            AssertHookEventsForHookType(HookType.AfterTestRun);
            AssertHookEventsForHookType(HookType.BeforeFeature);
            AssertHookEventsForHookType(HookType.AfterFeature);
            AssertHookEventsForHookType(HookType.BeforeScenario);
            AssertHookEventsForHookType(HookType.AfterScenario);
            AssertHookEventsForHookType(HookType.BeforeStep);
            AssertHookEventsForHookType(HookType.AfterStep);
        }

        [Fact]
        public async Task Should_publish_scenario_skipped_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            testExecutionEngine.OnScenarioInitialize(scenarioInfo);
            testExecutionEngine.OnScenarioSkipped();
            await testExecutionEngine.OnAfterLastStepAsync();
            await testExecutionEngine.OnScenarioEndAsync();

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<ScenarioStartedEvent>(e => e.ScenarioContext.Equals(scenarioContext) && e.FeatureContext.Equals(featureContainer.Resolve<FeatureContext>())));
            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Any<ScenarioSkippedEvent>());
            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<ScenarioFinishedEvent>(e => e.ScenarioContext.Equals(scenarioContext) && e.FeatureContext.Equals(featureContainer.Resolve<FeatureContext>())));
        }

        [Fact]
        public async Task Should_publish_feature_started_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnFeatureStartAsync(featureInfo);

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<FeatureStartedEvent>(e => e.FeatureContext.Equals(featureContainer.Resolve<FeatureContext>())));
        }

        [Fact]
        public async Task Should_publish_feature_finished_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnFeatureEndAsync();
            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<FeatureFinishedEvent>(e => e.FeatureContext.Equals(featureContainer.Resolve<FeatureContext>())));
        }

        [Fact]
        public async Task Should_publish_testrun_started_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunStartAsync();

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Any<TestRunStartedEvent>());
        }

        [Fact]
        public async Task Should_publish_testrun_finished_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunEndAsync();

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Any<TestRunFinishedEvent>());
        }

        private void AssertHookEventsForHookType(HookType hookType)
        {
            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<HookStartedEvent>(e => e.HookType == hookType));
            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<HookFinishedEvent>(e => e.HookType == hookType));
        }
    }
}
