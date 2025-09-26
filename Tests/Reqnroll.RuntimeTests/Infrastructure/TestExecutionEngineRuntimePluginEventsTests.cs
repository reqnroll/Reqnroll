using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reqnroll.BoDi;
using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    public partial class TestExecutionEngineTests
    {
        private const string SimulatedErrorMessage = "simulated error";

        private void RegisterFailingHook(List<IHookBinding> hooks)
        {
            var hookMock = CreateHookMock(hooks);
            _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                    .ThrowsAsync(new Exception(SimulatedErrorMessage));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforetestrun()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunStartAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeTestRun, It.IsAny<IObjectContainer>()));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforetestrun_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(_beforeTestRunEvents);
            Func<Task> act = async () => await testExecutionEngine.OnTestRunStartAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeTestRun, It.IsAny<IObjectContainer>()));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_aftertestrun()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunEndAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.AfterTestRun, It.IsAny<IObjectContainer>()));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_aftertestrun_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(_afterTestRunEvents);
            Func<Task> act = async () => await testExecutionEngine.OnTestRunEndAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.AfterTestRun, It.IsAny<IObjectContainer>()));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforefeature()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnFeatureStartAsync(_featureInfo);

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeFeature, It.IsAny<IObjectContainer>()));
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforefeature_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(_beforeFeatureEvents);
            Func<Task> act = async () => await testExecutionEngine.OnFeatureStartAsync(_featureInfo);

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeFeature, It.IsAny<IObjectContainer>()));
        }


        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterfeature()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnFeatureEndAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.AfterFeature, It.IsAny<IObjectContainer>()));
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterfeature_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(_afterFeatureEvents);
            Func<Task> act = async () => await testExecutionEngine.OnFeatureEndAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.AfterFeature, It.IsAny<IObjectContainer>()));
        }


        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforescenario()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnScenarioStartAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeScenario, It.IsAny<IObjectContainer>()));
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforescenario_after_hook_error_and_throw_error(bool stopAtFirstError)
        {
            _reqnrollConfiguration.StopAtFirstError = stopAtFirstError;
            var testExecutionEngine = CreateTestExecutionEngine();
            var handledInOnAfterLastStep = false;

            RegisterFailingHook(_beforeScenarioEvents);
            Func<Task> act = async () =>
            {
                //NOTE: the exception will be re-thrown in the OnAfterLastStep
                await testExecutionEngine.OnScenarioStartAsync();
                handledInOnAfterLastStep = true;
                await testExecutionEngine.OnAfterLastStepAsync(); 
            };

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeScenario, It.IsAny<IObjectContainer>()));
            handledInOnAfterLastStep.Should().Be(!stopAtFirstError); // in case of stopAtFirstError, the error should come from OnScenarioStartAsync
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforescenario_after_plugin_hook_error_and_throw_error(bool stopAtFirstError)
        {
            // this test is similar to the previous one, but it simulates an error from the plugin hook infrastructure, not a user hook
            // "normally" the plugin hooks should not throw exceptions, but still, we should ensure that the error is somehow visible

            _reqnrollConfiguration.StopAtFirstError = stopAtFirstError;
            var testExecutionEngine = CreateTestExecutionEngine();
            var handledInOnAfterLastStep = false;

            _runtimePluginTestExecutionLifecycleEventEmitter.Setup(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeScenario, It.IsAny<IObjectContainer>()))
                                                            .ThrowsAsync(new Exception(SimulatedErrorMessage));

            Func<Task> act = async () =>
            {
                //NOTE: the exception will be re-thrown in the OnAfterLastStep
                await testExecutionEngine.OnScenarioStartAsync();
                handledInOnAfterLastStep = true;
                await testExecutionEngine.OnAfterLastStepAsync(); 
            };

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeScenario, It.IsAny<IObjectContainer>()));
            handledInOnAfterLastStep.Should().Be(!stopAtFirstError); // in case of stopAtFirstError, the error should come from OnScenarioStartAsync
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_keep_user_hook_error_when_both_user_and_plugin_beforescenario_hook_fails(bool stopAtFirstError)
        {
            _reqnrollConfiguration.StopAtFirstError = stopAtFirstError;
            var testExecutionEngine = CreateTestExecutionEngine();

            // user hook will fail with SimulatedErrorMessage
            RegisterFailingHook(_beforeScenarioEvents);

            // plugin hook will also fail
            _runtimePluginTestExecutionLifecycleEventEmitter.Setup(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeScenario, It.IsAny<IObjectContainer>()))
                                                            .ThrowsAsync(new Exception("plugin-hook-error"));

            Func<Task> act = async () =>
            {
                //NOTE: the exception will be re-thrown in the OnAfterLastStep
                await testExecutionEngine.OnScenarioStartAsync();
                await testExecutionEngine.OnAfterLastStepAsync(); 
            };

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterscenario()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnScenarioEndAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.AfterScenario, It.IsAny<IObjectContainer>()));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterscenario_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(_afterScenarioEvents);
            Func<Task> act = async () => await testExecutionEngine.OnScenarioEndAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.AfterScenario, It.IsAny<IObjectContainer>()));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforestep()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeStep, It.IsAny<IObjectContainer>()));
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforestep_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();
            RegisterFailingHook(_beforeStepEvents);

            Func<Task> act = async () =>
            {
                //NOTE: the exception will be re-thrown in the OnAfterLastStep
                await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
                await testExecutionEngine.OnAfterLastStepAsync();
            };

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.BeforeStep, It.IsAny<IObjectContainer>()));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterstep()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.AfterStep, It.IsAny<IObjectContainer>()));
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterstep_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();
            RegisterFailingHook(_afterStepEvents);

            Func<Task> act = async () =>
            {
                //NOTE: the exception will be re-thrown in the OnAfterLastStep
                await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
                await testExecutionEngine.OnAfterLastStepAsync();
            };

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Verify(e => e.RaiseExecutionLifecycleEventAsync(HookType.AfterStep, It.IsAny<IObjectContainer>()));
        }
    }
}
