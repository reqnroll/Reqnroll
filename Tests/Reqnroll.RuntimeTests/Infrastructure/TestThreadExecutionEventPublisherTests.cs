using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.BoDi;
using Reqnroll.ErrorHandling;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    public partial class TestExecutionEngineTests
    {
        [Fact]
        public async Task Should_publish_step_started_event()
        {
            var stepDefinition = RegisterStepDefinition().Object;
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _testThreadExecutionEventPublisher.Verify(te =>
                    te.PublishEventAsync(It.Is<StepStartedEvent>(e => e.ScenarioContext.Equals(_scenarioContext) &&
                                                                 e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()) &&
                                                                 e.StepContext.Equals(_contextManagerStub.Object.StepContext))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_step_binding_started_event()
        {
            var stepDef = RegisterStepDefinition().Object;
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<StepBindingStartedEvent>(e => 
                                                                   e.StepDefinitionBinding.Equals(stepDef))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_step_binding_finished_event()
        {
            var stepDef = RegisterStepDefinition().Object;
            TimeSpan expectedDuration = TimeSpan.FromSeconds(5);
            _methodBindingInvokerMock
                .Setup(i => i.InvokeBindingAsync(stepDef, _contextManagerStub.Object, It.IsAny<object[]>(), _testTracerStub.Object, It.IsAny<DurationHolder>()))
                .Callback((IBinding _, IContextManager _, object[] arguments, ITestTracer _, DurationHolder durationHolder) => durationHolder.Duration = expectedDuration)
                .ReturnsAsync(new object());
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<StepBindingFinishedEvent>(e =>
                                                                    e.StepDefinitionBinding.Equals(stepDef) && 
                                                                    e.Duration.Equals(expectedDuration))),
                                                      Times.Once);
        }
        
        [Fact]
        public async Task Should_publish_step_finished_event()
        {
            var stepDefinition = RegisterStepDefinition().Object;

            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
            
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<StepFinishedEvent>(e => 
                                                             e.ScenarioContext.Equals(_scenarioContext) &&
                                                             e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()) &&
                                                             e.StepContext.Equals(_contextManagerStub.Object.StepContext))),
                                                      Times.Once);
        }
        
        [Fact]
        public async Task Should_publish_step_skipped_event()
        {
            RegisterStepDefinition();
            var testExecutionEngine = CreateTestExecutionEngine();
            //a step will be skipped if the ScenarioExecutionStatus is not OK
            _scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
            
            _testThreadExecutionEventPublisher.Verify(te =>
                                                          te.PublishEventAsync(It.IsAny<StepSkippedEvent>()), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_publish_hook_binding_events(bool isDryRun)
        {
            _testObjectResolverMock.Reset();
            if (isDryRun)
            {
                _environmentOptions.Setup(e => e.IsDryRun).Returns(true);
                _testObjectResolverMock.Setup(r => r.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()))
                                       .Throws(new Exception("ResolveBindingInstance should not be called in dry-run mode"));
            }
            else
            {
                _testObjectResolverMock.Setup(r => r.ResolveBindingInstance(typeof(string), It.IsAny<IObjectContainer>()))
                                       .Returns(string.Empty);
            }

            var hookType = HookType.AfterScenario;
            TimeSpan expectedDuration = TimeSpan.FromSeconds(5);
            var bindingMethod = new Mock<IBindingMethod>();
            var bindingParameter = new Mock<IBindingParameter>();
            bindingParameter.SetupGet(p => p.Type).Returns(new RuntimeBindingType(typeof(string)));
            bindingMethod.SetupGet(m => m.Parameters).Returns([bindingParameter.Object]);

            var expectedHookBinding = new HookBinding(bindingMethod.Object, hookType, null, 1);
            _methodBindingInvokerMock
                .Setup(i => i.InvokeBindingAsync(expectedHookBinding, _contextManagerStub.Object, It.IsAny<object[]>(), _testTracerStub.Object, It.IsAny<DurationHolder>()))
                .Callback((IBinding _, IContextManager _, object[] arguments, ITestTracer _, DurationHolder durationHolder) => durationHolder.Duration = expectedDuration)
                .ReturnsAsync(new object());
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.InvokeHookAsync(_methodBindingInvokerMock.Object, expectedHookBinding, hookType);

            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<HookBindingStartedEvent>(e =>
                                                                   e.HookBinding.Equals(expectedHookBinding))),
                                                      Times.Once);
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<HookBindingFinishedEvent>(e =>
                                                                    e.HookBinding.Equals(expectedHookBinding) &&
                                                                    e.Duration.Equals(expectedDuration))),
                                                      Times.Once);

            var times = isDryRun ? Times.Never() : Times.AtLeastOnce();
            _testObjectResolverMock.Verify(r => r.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()), times);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_publish_hook_binding_finished_event_with_OK_status_when_hook_succeeds(bool isDryRun)
        {
            // Arrange
            _testObjectResolverMock.Reset();
            if (isDryRun)
            {
                _environmentOptions.Setup(e => e.IsDryRun).Returns(true);
                _testObjectResolverMock.Setup(r => r.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()))
                                       .Throws(new Exception("ResolveBindingInstance should not be called in dry-run mode"));
            }
            else
            {
                _testObjectResolverMock.Setup(r => r.ResolveBindingInstance(typeof(string), It.IsAny<IObjectContainer>()))
                                       .Returns(string.Empty);
            }

            var hookType = HookType.AfterScenario;
            TimeSpan expectedDuration = TimeSpan.FromSeconds(5);
            var bindingMethod = new Mock<IBindingMethod>();
            var bindingParameter = new Mock<IBindingParameter>();
            bindingParameter.SetupGet(p => p.Type).Returns(new RuntimeBindingType(typeof(string)));
            bindingMethod.SetupGet(m => m.Parameters).Returns([bindingParameter.Object]);

            var expectedHookBinding = new HookBinding(bindingMethod.Object, hookType, null, 1);
            _methodBindingInvokerMock
                .Setup(i => i.InvokeBindingAsync(expectedHookBinding, _contextManagerStub.Object, It.IsAny<object[]>(), _testTracerStub.Object, It.IsAny<DurationHolder>()))
                .Callback((IBinding _, IContextManager _, object[] arguments, ITestTracer _, DurationHolder durationHolder) => durationHolder.Duration = expectedDuration)
                .ReturnsAsync(new object());
            var testExecutionEngine = CreateTestExecutionEngine();

            // Act
            await testExecutionEngine.InvokeHookAsync(_methodBindingInvokerMock.Object, expectedHookBinding, hookType);

            // Assert
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<HookBindingFinishedEvent>(e =>
                                                                    e.HookBinding.Equals(expectedHookBinding) &&
                                                                    e.Duration.Equals(expectedDuration) &&
                                                                    e.HookStatus == ScenarioExecutionStatus.OK &&
                                                                    e.HookException == null)),
                                                      Times.Once);

            var times = isDryRun ? Times.Never() : Times.AtLeastOnce();
            _testObjectResolverMock.Verify(r => r.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()), times);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_publish_hook_binding_finished_event_with_unit_test_provider_detected_status(bool isDryRun)
        {
            // Arrange
            _testObjectResolverMock.Reset();
            if (isDryRun)
            {
                _environmentOptions.Setup(e => e.IsDryRun).Returns(true);
                _testObjectResolverMock.Setup(r => r.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()))
                                       .Throws(new Exception("ResolveBindingInstance should not be called in dry-run mode"));
            }
            else
            {
                _testObjectResolverMock.Setup(r => r.ResolveBindingInstance(typeof(string), It.IsAny<IObjectContainer>()))
                                       .Returns(string.Empty);
            }

            var hookType = HookType.AfterScenario;
            TimeSpan expectedDuration = TimeSpan.FromSeconds(3);
            var bindingMethod = new Mock<IBindingMethod>();
            var bindingParameter = new Mock<IBindingParameter>();
            bindingParameter.SetupGet(p => p.Type).Returns(new RuntimeBindingType(typeof(string)));
            bindingMethod.SetupGet(m => m.Parameters).Returns([bindingParameter.Object]);

            var expectedHookBinding = new HookBinding(bindingMethod.Object, hookType, null, 1);
            var expectedException = new Exception("Custom test framework exception");

            // Setup the unit test provider to return a specific status for this exception type
            _unitTestRuntimeProviderStub.Setup(p => p.DetectExecutionStatus(expectedException))
                                       .Returns(ScenarioExecutionStatus.Skipped);

            _methodBindingInvokerMock
                .Setup(i => i.InvokeBindingAsync(expectedHookBinding, _contextManagerStub.Object, It.IsAny<object[]>(), _testTracerStub.Object, It.IsAny<DurationHolder>()))
                .Callback((IBinding _, IContextManager _, object[] arguments, ITestTracer _, DurationHolder durationHolder) => durationHolder.Duration = expectedDuration)
                .ThrowsAsync(expectedException);
            var testExecutionEngine = CreateTestExecutionEngine();

            // Act & Assert
            await FluentActions.Awaiting(() => testExecutionEngine.InvokeHookAsync(_methodBindingInvokerMock.Object, expectedHookBinding, hookType))
                               .Should().ThrowAsync<Exception>();

            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<HookBindingFinishedEvent>(e =>
                                                                    e.HookBinding.Equals(expectedHookBinding) &&
                                                                    e.Duration.Equals(expectedDuration) &&
                                                                    e.HookStatus == ScenarioExecutionStatus.Skipped &&
                                                                    e.HookException == expectedException)),
                                                      Times.Once);

            var times = isDryRun ? Times.Never() : Times.AtLeastOnce();
            _testObjectResolverMock.Verify(r => r.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()), times);
        }

        public static TheoryData<Type, ScenarioExecutionStatus, bool> Arguments__Should_publish_hook_binding_finished_event_with_correct_status_for_exception_types()
        {
            var data = new TheoryData<Type, ScenarioExecutionStatus, bool>();
            (Type, ScenarioExecutionStatus)[] options = 
                [
                    (typeof(NotImplementedException), ScenarioExecutionStatus.StepDefinitionPending),
                    (typeof(PendingScenarioException), ScenarioExecutionStatus.StepDefinitionPending),
                    (typeof(InvalidOperationException), ScenarioExecutionStatus.TestError),
                    (typeof(ArgumentException), ScenarioExecutionStatus.TestError)
                ];
            foreach (var option in options)
            {
                data.Add(option.Item1, option.Item2, true);
                data.Add(option.Item1, option.Item2, false);
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(Arguments__Should_publish_hook_binding_finished_event_with_correct_status_for_exception_types))]
        public async Task Should_publish_hook_binding_finished_event_with_correct_status_for_exception_types(Type exceptionType, ScenarioExecutionStatus expectedStatus, bool isDryRun)
        {
            // Arrange
            _testObjectResolverMock.Reset();
            if (isDryRun)
            {
                _environmentOptions.Setup(e => e.IsDryRun).Returns(true);
                _testObjectResolverMock.Setup(r => r.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()))
                                       .Throws(new Exception("ResolveBindingInstance should not be called in dry-run mode"));
            }
            else
            {
                _testObjectResolverMock.Setup(r => r.ResolveBindingInstance(typeof(string), It.IsAny<IObjectContainer>()))
                                       .Returns(string.Empty);
            }

            var hookType = HookType.BeforeStep;
            TimeSpan expectedDuration = TimeSpan.FromSeconds(1);
            var bindingMethod = new Mock<IBindingMethod>();
            var bindingParameter = new Mock<IBindingParameter>();
            bindingParameter.SetupGet(p => p.Type).Returns(new RuntimeBindingType(typeof(string)));
            bindingMethod.SetupGet(m => m.Parameters).Returns([bindingParameter.Object]);
            var expectedHookBinding = new HookBinding(bindingMethod.Object, hookType, null, 1);
            var expectedException = (Exception)Activator.CreateInstance(exceptionType, "Test exception");

            _methodBindingInvokerMock
                .Setup(i => i.InvokeBindingAsync(expectedHookBinding, _contextManagerStub.Object, It.IsAny<object[]>(), _testTracerStub.Object, It.IsAny<DurationHolder>()))
                .Callback((IBinding _, IContextManager _, object[] arguments, ITestTracer _, DurationHolder durationHolder) => durationHolder.Duration = expectedDuration)
                .ThrowsAsync(expectedException);
            var testExecutionEngine = CreateTestExecutionEngine();

            // Act & Assert
            await FluentActions.Awaiting(() => testExecutionEngine.InvokeHookAsync(_methodBindingInvokerMock.Object, expectedHookBinding, hookType))
                               .Should().ThrowAsync<Exception>();

            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<HookBindingFinishedEvent>(e =>
                                                                    e.HookBinding.Equals(expectedHookBinding) &&
                                                                    e.Duration.Equals(expectedDuration) &&
                                                                    e.HookStatus == expectedStatus &&
                                                                    e.HookException == expectedException)),
                                                      Times.Once);

            var times = isDryRun ? Times.Never() : Times.AtLeastOnce();
            _testObjectResolverMock.Verify(r => r.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()), times);
        }

        [Fact]
        public async Task Should_publish_scenario_started_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
            await testExecutionEngine.OnScenarioStartAsync();

            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<ScenarioStartedEvent>(e =>
                                                                e.ScenarioContext.Equals(_scenarioContext) &&
                                                                e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_scenario_finished_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
            await testExecutionEngine.OnScenarioStartAsync();
            await testExecutionEngine.OnAfterLastStepAsync();
            await testExecutionEngine.OnScenarioEndAsync();
            
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<ScenarioFinishedEvent>(e =>
                                                                 e.ScenarioContext.Equals(_scenarioContext) &&
                                                                 e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_scenario_finished_event_even_if_the_after_scenario_hook_fails()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            var hookMock = CreateHookMock(_afterScenarioEvents);
            _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                    .Throws(new Exception("simulated hook error"));

            testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
            await testExecutionEngine.OnScenarioStartAsync();
            await testExecutionEngine.OnAfterLastStepAsync();
            await FluentActions.Awaiting(testExecutionEngine.OnScenarioEndAsync)
                         .Should().ThrowAsync<Exception>();
            
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<ScenarioFinishedEvent>(e =>
                                                                 e.ScenarioContext.Equals(_scenarioContext) &&
                                                                 e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_hook_started_finished_events()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            await testExecutionEngine.OnTestRunStartAsync();
            await testExecutionEngine.OnFeatureStartAsync(_featureInfo);

            testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
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

            testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
            await testExecutionEngine.OnScenarioSkippedAsync();
            await testExecutionEngine.OnAfterLastStepAsync();
            await testExecutionEngine.OnScenarioEndAsync();
            
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<ScenarioStartedEvent>(e =>
                                                                e.ScenarioContext.Equals(_scenarioContext) &&
                                                                e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()))),
                                                      Times.Once);
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.IsAny<ScenarioSkippedEvent>()), Times.Once);
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<ScenarioFinishedEvent>(e =>
                                                                 e.ScenarioContext.Equals(_scenarioContext) &&
                                                                 e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_feature_started_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnFeatureStartAsync(_featureInfo);
            
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<FeatureStartedEvent>(e =>
                                                               e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_feature_finished_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnFeatureEndAsync();
            
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<FeatureFinishedEvent>(e => 
                                                                e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_feature_finished_event_even_if_the_after_feature_hook_fails()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            var hookMock = CreateHookMock(_afterFeatureEvents);
            _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                    .Throws(new Exception("simulated hook error"));

            await FluentActions.Awaiting(testExecutionEngine.OnFeatureEndAsync)
                               .Should().ThrowAsync<Exception>();
            
            _testThreadExecutionEventPublisher.Verify(te =>
                te.PublishEventAsync(It.Is<FeatureFinishedEvent>(e => 
                                                                e.FeatureContext.Equals(_featureContainer.Resolve<FeatureContext>()))),
                                                      Times.Once);
        }

        [Fact]
        public async Task Should_publish_test_run_started_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunStartAsync();
            
            _testThreadExecutionEventPublisher.Verify(te =>
                                                          te.PublishEventAsync(It.IsAny<TestRunStartedEvent>()), Times.Once);
        }

        [Fact]
        public async Task Should_publish_test_run_finished_event()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunEndAsync();
            
            _testThreadExecutionEventPublisher.Verify(te =>
                                                          te.PublishEventAsync(It.IsAny<TestRunFinishedEvent>()), Times.Once);
        }

        [Fact]
        public async Task Should_publish_test_run_finished_event_even_if_the_after_test_run_hook_fails()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            var hookMock = CreateHookMock(_afterTestRunEvents);
            _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                    .Throws(new Exception("simulated hook error"));

            await FluentActions.Awaiting(testExecutionEngine.OnTestRunEndAsync)
                               .Should().ThrowAsync<Exception>();
            
            _testThreadExecutionEventPublisher.Verify(te =>
                                                          te.PublishEventAsync(It.IsAny<TestRunFinishedEvent>()), Times.Once);
        }

        private void AssertHookEventsForHookType(HookType hookType)
        {
            _testThreadExecutionEventPublisher.Verify(
                te =>
                    te.PublishEventAsync(It.Is<HookStartedEvent>(e => e.HookType == hookType)),
                Times.Once);
            _testThreadExecutionEventPublisher.Verify(
                te =>
                    te.PublishEventAsync(It.Is<HookFinishedEvent>(e => e.HookType == hookType)),
                Times.Once);
        }
    }
}
