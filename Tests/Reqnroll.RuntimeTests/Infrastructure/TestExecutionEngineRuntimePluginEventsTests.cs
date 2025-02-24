using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reqnroll.BoDi;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
            methodBindingInvokerMock.InvokeBindingAsync(hookMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>())
                                    .ThrowsAsync(new Exception(SimulatedErrorMessage));
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforetestrun()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunStartAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.BeforeTestRun, Arg.Any<IObjectContainer>());
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforetestrun_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(beforeTestRunEvents);
            Func<Task> act = async () => await testExecutionEngine.OnTestRunStartAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.BeforeTestRun, Arg.Any<IObjectContainer>());
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_aftertestrun()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunEndAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.AfterTestRun, Arg.Any<IObjectContainer>());
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_aftertestrun_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(afterTestRunEvents);
            Func<Task> act = async () => await testExecutionEngine.OnTestRunEndAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.AfterTestRun, Arg.Any<IObjectContainer>());
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforefeature()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnFeatureStartAsync(featureInfo);

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.BeforeFeature, Arg.Any<IObjectContainer>());
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforefeature_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(beforeFeatureEvents);
            Func<Task> act = async () => await testExecutionEngine.OnFeatureStartAsync(featureInfo);

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.BeforeFeature, Arg.Any<IObjectContainer>());
        }


        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterfeature()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnFeatureEndAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.AfterFeature, Arg.Any<IObjectContainer>());
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterfeature_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(afterFeatureEvents);
            Func<Task> act = async () => await testExecutionEngine.OnFeatureEndAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.AfterFeature, Arg.Any<IObjectContainer>());
        }


        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforescenario()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnScenarioStartAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.BeforeScenario, Arg.Any<IObjectContainer>());
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforescenario_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(beforeScenarioEvents);
            Func<Task> act = async () =>
            {
                //NOTE: the exception will be re-thrown in the OnAfterLastStep
                await testExecutionEngine.OnScenarioStartAsync();
                await testExecutionEngine.OnAfterLastStepAsync(); 
            };

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.BeforeScenario, Arg.Any<IObjectContainer>());
        }


        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterscenario()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnScenarioEndAsync();

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.AfterScenario, Arg.Any<IObjectContainer>());
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterscenario_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            RegisterFailingHook(afterScenarioEvents);
            Func<Task> act = async () => await testExecutionEngine.OnScenarioEndAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.AfterScenario, Arg.Any<IObjectContainer>());
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforestep()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.BeforeStep, Arg.Any<IObjectContainer>());
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_beforestep_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();
            RegisterFailingHook(beforeStepEvents);

            Func<Task> act = async () =>
            {
                //NOTE: the exception will be re-thrown in the OnAfterLastStep
                await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
                await testExecutionEngine.OnAfterLastStepAsync();
            };

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.BeforeStep, Arg.Any<IObjectContainer>());
        }

        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterstep()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.AfterStep, Arg.Any<IObjectContainer>());
        }
        
        [Fact]
        public async Task Should_emit_runtime_plugin_test_execution_lifecycle_event_afterstep_after_hook_error_and_throw_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();
            RegisterFailingHook(afterStepEvents);

            Func<Task> act = async () =>
            {
                //NOTE: the exception will be re-thrown in the OnAfterLastStep
                await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
                await testExecutionEngine.OnAfterLastStepAsync();
            };

            await act.Should().ThrowAsync<Exception>().WithMessage(SimulatedErrorMessage);
            _runtimePluginTestExecutionLifecycleEventEmitter.Received().RaiseExecutionLifecycleEvent(HookType.AfterStep, Arg.Any<IObjectContainer>());
        }

    }
}
