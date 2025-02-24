using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reqnroll.BoDi;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Reqnroll.BindingSkeletons;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Configuration;
using Reqnroll.ErrorHandling;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using FluentAssertions;
using Reqnroll.Analytics;
using Reqnroll.Events;
using Reqnroll.Plugins;
using Reqnroll.TestFramework;

namespace Reqnroll.RuntimeTests.Infrastructure
{

    public partial class TestExecutionEngineTests
    {
        private ScenarioContext scenarioContext;
        private ReqnrollConfiguration reqnrollConfiguration;
        private IBindingRegistry bindingRegistryStub;
        private IErrorProvider errorProviderStub;
        private IContextManager contextManagerStub;
        private ITestTracer testTracerStub;
        private IStepDefinitionMatchService stepDefinitionMatcherStub;
        private IAsyncBindingInvoker methodBindingInvokerMock;
        private IStepDefinitionSkeletonProvider stepDefinitionSkeletonProviderMock;
        private ITestObjectResolver testObjectResolverMock;
        private IObsoleteStepHandler obsoleteTestHandlerMock;
        private FeatureInfo featureInfo;
        private ScenarioInfo scenarioInfo;
        private ObjectContainer globalContainer;
        private ObjectContainer testThreadContainer;
        private ObjectContainer featureContainer;
        private ObjectContainer scenarioContainer;
        private DefaultTestRunContext testRunContext;
        private TestObjectResolver defaultTestObjectResolver = new TestObjectResolver();
        private ITestPendingMessageFactory _testPendingMessageFactory;
        private ITestUndefinedMessageFactory _testUndefinedMessageFactory;
        private IAnalyticsEventProvider _analyticsEventProvider;
        private IAnalyticsTransmitter _analyticsTransmitter;
        private ITestRunnerManager _testRunnerManager;
        private IRuntimePluginTestExecutionLifecycleEventEmitter _runtimePluginTestExecutionLifecycleEventEmitter;
        private ITestThreadExecutionEventPublisher _testThreadExecutionEventPublisher;
        private IStepArgumentTypeConverter _stepArgumentTypeConverterMock;

        private List<IHookBinding> beforeScenarioEvents;
        private List<IHookBinding> afterScenarioEvents;
        private List<IHookBinding> beforeStepEvents;
        private List<IHookBinding> afterStepEvents;
        private List<IHookBinding> beforeFeatureEvents;
        private List<IHookBinding> afterFeatureEvents;
        private List<IHookBinding> beforeTestRunEvents;
        private List<IHookBinding> afterTestRunEvents;
        private List<IHookBinding> beforeScenarioBlockEvents;
        private List<IHookBinding> afterScenarioBlockEvents;

        private List<IStepArgumentTransformationBinding> stepTransformations;

        class DummyClass
        {
            public static DummyClass LastInstance = null;
            public DummyClass()
            {
                LastInstance = this;
            }
        }

        class AnotherDummyClass { }


        public TestExecutionEngineTests()
        {
            reqnrollConfiguration = ConfigurationLoader.GetDefault();

            globalContainer = new ObjectContainer();
            testThreadContainer = new ObjectContainer(globalContainer);
            featureContainer = new ObjectContainer(testThreadContainer);
            scenarioContainer = new ObjectContainer(scenarioContainer);
            testRunContext = new DefaultTestRunContext(globalContainer, Substitute.For<ITestRunSettingsProvider>());

            beforeScenarioEvents = new List<IHookBinding>();
            afterScenarioEvents = new List<IHookBinding>();
            beforeStepEvents = new List<IHookBinding>();
            afterStepEvents = new List<IHookBinding>();
            beforeFeatureEvents = new List<IHookBinding>();
            afterFeatureEvents = new List<IHookBinding>();
            beforeTestRunEvents = new List<IHookBinding>();
            afterTestRunEvents = new List<IHookBinding>();
            beforeScenarioBlockEvents = new List<IHookBinding>();
            afterScenarioBlockEvents = new List<IHookBinding>();

            stepTransformations = new List<IStepArgumentTransformationBinding>();

            stepDefinitionSkeletonProviderMock = Substitute.For<IStepDefinitionSkeletonProvider>();
            testObjectResolverMock = Substitute.For<ITestObjectResolver>();
            testObjectResolverMock.ResolveBindingInstance(Arg.Any<Type>(), Arg.Any<IObjectContainer>())
                .Returns((Type t, IObjectContainer container) => defaultTestObjectResolver.ResolveBindingInstance(t, container));

            var culture = new CultureInfo("en-US", false);
            contextManagerStub = Substitute.For<IContextManager>();
            scenarioInfo = new ScenarioInfo("scenario_title", "scenario_description", null, null);
            scenarioContext = new ScenarioContext(scenarioContainer, scenarioInfo, testObjectResolverMock);
            scenarioContainer.RegisterInstanceAs(scenarioContext);
            contextManagerStub.ScenarioContext.Returns(scenarioContext);
            featureInfo = new FeatureInfo(culture, "feature path", "feature_title", "", ProgrammingLanguage.CSharp);
            var featureContext = new FeatureContext(featureContainer, featureInfo, reqnrollConfiguration);
            featureContainer.RegisterInstanceAs(featureContext);
            contextManagerStub.FeatureContext.Returns(featureContext);
            contextManagerStub.StepContext.Returns(new ScenarioStepContext(new StepInfo(StepDefinitionType.Given, "step_title", null, null)));

            bindingRegistryStub = Substitute.For<IBindingRegistry>();
            bindingRegistryStub.GetHooks(HookType.BeforeStep).Returns(beforeStepEvents);
            bindingRegistryStub.GetHooks(HookType.AfterStep).Returns(afterStepEvents);
            bindingRegistryStub.GetHooks(HookType.BeforeScenarioBlock).Returns(beforeScenarioBlockEvents);
            bindingRegistryStub.GetHooks(HookType.AfterScenarioBlock).Returns(afterScenarioBlockEvents);
            bindingRegistryStub.GetHooks(HookType.BeforeFeature).Returns(beforeFeatureEvents);
            bindingRegistryStub.GetHooks(HookType.AfterFeature).Returns(afterFeatureEvents);
            bindingRegistryStub.GetHooks(HookType.BeforeTestRun).Returns(beforeTestRunEvents);
            bindingRegistryStub.GetHooks(HookType.AfterTestRun).Returns(afterTestRunEvents);
            bindingRegistryStub.GetHooks(HookType.BeforeScenario).Returns(beforeScenarioEvents);
            bindingRegistryStub.GetHooks(HookType.AfterScenario).Returns(afterScenarioEvents);

            bindingRegistryStub.GetStepTransformations().Returns(stepTransformations);
            bindingRegistryStub.IsValid.Returns(true);

            reqnrollConfiguration = ConfigurationLoader.GetDefault();
            errorProviderStub = Substitute.For<IErrorProvider>();
            testTracerStub = Substitute.For<ITestTracer>();
            stepDefinitionMatcherStub = Substitute.For<IStepDefinitionMatchService>();
            methodBindingInvokerMock = Substitute.For<IAsyncBindingInvoker>();

            obsoleteTestHandlerMock = Substitute.For<IObsoleteStepHandler>();

            _testPendingMessageFactory = new TestPendingMessageFactory(errorProviderStub);
            _testUndefinedMessageFactory = new TestUndefinedMessageFactory(stepDefinitionSkeletonProviderMock, errorProviderStub, reqnrollConfiguration);

            _analyticsEventProvider = Substitute.For<IAnalyticsEventProvider>();
            _analyticsTransmitter = Substitute.For<IAnalyticsTransmitter>();
            //TODO NSub check
            //_analyticsTransmitter.TransmitReqnrollProjectRunningEventAsync(Arg.Any<ReqnrollProjectRunningEvent>())
            //    .Callback(() => { });

            _testRunnerManager = Substitute.For<ITestRunnerManager>();
            _testRunnerManager.TestAssembly.Returns(Assembly.GetCallingAssembly());

            _runtimePluginTestExecutionLifecycleEventEmitter = Substitute.For<IRuntimePluginTestExecutionLifecycleEventEmitter>();
            _testThreadExecutionEventPublisher = Substitute.For<ITestThreadExecutionEventPublisher>();

            _stepArgumentTypeConverterMock = Substitute.For<IStepArgumentTypeConverter>();
        }

        private TestExecutionEngine CreateTestExecutionEngine()
        {
            return new TestExecutionEngine(
                Substitute.For<IStepFormatter>(),
                testTracerStub,
                errorProviderStub,
                _stepArgumentTypeConverterMock,
                reqnrollConfiguration,
                bindingRegistryStub,
                Substitute.For<IUnitTestRuntimeProvider>(),
                contextManagerStub,
                stepDefinitionMatcherStub,
                methodBindingInvokerMock,
                obsoleteTestHandlerMock,
                _analyticsEventProvider,
                _analyticsTransmitter,
                _testRunnerManager,
                _runtimePluginTestExecutionLifecycleEventEmitter,
                _testThreadExecutionEventPublisher,
                _testPendingMessageFactory,
                _testUndefinedMessageFactory,
                testObjectResolverMock,
                testRunContext);
        }


        private IStepDefinitionBinding RegisterStepDefinition()
        {
            var methodStub = Substitute.For<IBindingMethod>();
            var stepDefStub = Substitute.For<IStepDefinitionBinding>();
            stepDefStub.Method.Returns(methodStub);

            StepDefinitionAmbiguityReason ambiguityReason;
            List<BindingMatch> candidatingMatches;
            stepDefinitionMatcherStub.GetBestMatch(Arg.Any<StepInstance>(), Arg.Any<CultureInfo>(), out ambiguityReason, out candidatingMatches)
                .Returns(
                    new BindingMatch(stepDefStub, 0, new object[0], new StepContext("bla", "foo", new List<string>(), CultureInfo.InvariantCulture)));

            return stepDefStub;
        }

        private IStepDefinitionBinding RegisterStepDefinitionWithTransformation(IBindingType bindingType)
        {
            var bindingParameterStub = Substitute.For<IBindingParameter>();
            bindingParameterStub.Type.Returns(bindingType);
            var methodStub = Substitute.For<IBindingMethod>();
            methodStub.Parameters.Returns(new[] { bindingParameterStub });
            var stepDefStub = Substitute.For<IStepDefinitionBinding>();
            stepDefStub.Method.Returns(methodStub);

            StepDefinitionAmbiguityReason ambiguityReason;
            List<BindingMatch> candidatingMatches;
            stepDefinitionMatcherStub.GetBestMatch(Arg.Any<StepInstance>(), Arg.Any<CultureInfo>(), out ambiguityReason, out candidatingMatches)
                .Returns(
                    new BindingMatch(stepDefStub, 0, new object[] { "userName" }, new StepContext("bla", "foo", new List<string>(), CultureInfo.InvariantCulture)));

            return stepDefStub;
        }

        private IStepDefinitionBinding RegisterUndefinedStepDefinition()
        {
            var methodStub = Substitute.For<IBindingMethod>();
            var stepDefStub = Substitute.For<IStepDefinitionBinding>();
            stepDefStub.Method.Returns(methodStub);

            StepDefinitionAmbiguityReason ambiguityReason;
            List<BindingMatch> candidatingMatches;
            stepDefinitionMatcherStub.GetBestMatch(Arg.Any<StepInstance>(), Arg.Any<CultureInfo>(), out ambiguityReason, out candidatingMatches)
                .Returns(BindingMatch.NonMatching);

            return stepDefStub;
        }

        private void RegisterFailingStepDefinition(TimeSpan? expectedDuration = null)
        {
            var stepDefStub = RegisterStepDefinition();

            methodBindingInvokerMock.InvokeBindingAsync(stepDefStub, contextManagerStub, Arg.Any<object[]>(), testTracerStub, Arg.Any<DurationHolder>())
                                    .ThrowsAsync(
                                        callInfo =>
                                        {
                                            if (expectedDuration.HasValue)
                                            {
                                                callInfo.Arg<DurationHolder>().Duration = expectedDuration.Value;
                                            }

                                            return new Exception("simulated error");
                                        });
        }

        private IHookBinding CreateHookMock(List<IHookBinding> hookList)
        {
            var mock = Substitute.For<IHookBinding>();
            hookList.Add(mock);
            return mock;
        }

        private IHookBinding CreateParametrizedHookMock(List<IHookBinding> hookList, params Type[] paramTypes)
        {
            var hookMock = CreateHookMock(hookList);
            var bindingMethod = new BindingMethod(new BindingType("AssemblyBT", "BT", "Test.BT"), "X",
                paramTypes.Select((paramType, i) => new BindingParameter(new RuntimeBindingType(paramType), "p" + i)),
                RuntimeBindingType.Void);
            hookMock.Method.Returns(bindingMethod);
            return hookMock;
        }

        private IStepArgumentTransformationBinding CreateStepTransformationBinding(string regexString, IBindingMethod transformMethod)
        {
            return new StepArgumentTransformationBinding(regexString, transformMethod);
        }

        private void AssertHooksWasCalledWithParam(IHookBinding hookMock, object paramObj)
        {
            methodBindingInvokerMock.Received(1)
                                    .InvokeBindingAsync(hookMock, contextManagerStub, Arg.Is((object[] args) => args != null && args.Length > 0 && args.Any(arg => arg == paramObj)), testTracerStub, Arg.Any<DurationHolder>());
        }

        [Fact]
        public async Task Should_execute_before_step()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var hookMock = CreateHookMock(beforeStepEvents);

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            await methodBindingInvokerMock.Received(1).InvokeBindingAsync(hookMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>());  //TODO NSub check
        }

        [Fact]
        public async Task Should_execute_after_step()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var hookMock = CreateHookMock(afterStepEvents);

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            await methodBindingInvokerMock.Received(1).InvokeBindingAsync(hookMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>()); //TODO NSub check
        }

        [Fact]
        public async Task Should_not_execute_step_when_there_was_an_error_earlier()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            var stepDefMock = RegisterStepDefinition();

            scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            await methodBindingInvokerMock.DidNotReceive().InvokeBindingAsync(stepDefMock, Arg.Any<IContextManager>(), Arg.Any<object[]>(), Arg.Any<ITestTracer>(), Arg.Any<DurationHolder>());
        }

        [Fact]
        public async Task Should_not_execute_step_hooks_when_there_was_an_error_earlier()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

            var beforeStepMock = CreateHookMock(beforeStepEvents);
            var afterStepMock = CreateHookMock(afterStepEvents);

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            await methodBindingInvokerMock.DidNotReceive().InvokeBindingAsync(beforeStepMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>());
            await methodBindingInvokerMock.DidNotReceive().InvokeBindingAsync(afterStepMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>());
        }

        [Fact]
        public async Task Should_not_execute_step_argument_transformations_when_there_was_an_error_earlier()
        {
            var testExecutionEngine = CreateTestExecutionEngine();

            var bindingTypeStub = Substitute.For<IBindingType>();
            RegisterStepDefinitionWithTransformation(bindingTypeStub);

            scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

            UserCreator stepTransformationInstance = new UserCreator();
            var transformMethod = new RuntimeBindingMethod(stepTransformationInstance.GetType().GetMethod("Create"));
            var stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);
            stepTransformations.Add(stepTransformationBinding);

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "user bar", null, null);

            await _stepArgumentTypeConverterMock.DidNotReceive().ConvertAsync(Arg.Any<object>(), Arg.Any<IBindingType>(), Arg.Any<CultureInfo>());
        }

        [Fact]
        public async Task Should_execute_after_step_when_step_definition_failed()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterFailingStepDefinition();

            var hookMock = CreateHookMock(afterStepEvents);

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            await methodBindingInvokerMock.Received().InvokeBindingAsync(hookMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>());
        }

        [Fact]
        public async Task Should_cleanup_step_context_after_scenario_block_hook_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var hookMock = CreateHookMock(beforeScenarioBlockEvents);
            methodBindingInvokerMock.InvokeBindingAsync(hookMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>())
                .Throws(new Exception("simulated error"));

            try
            {
                await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

                Assert.Fail("execution of the step should have failed because of the exeption thrown by the before scenario block hook");
            }
            catch (Exception)
            {
            }

            await methodBindingInvokerMock.Received(1).InvokeBindingAsync(hookMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>());
            contextManagerStub.Received().CleanupStepContext();
        }

        [Fact]
        public async Task Should_not_execute_afterstep_when_step_is_undefined()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterUndefinedStepDefinition();

            var afterStepMock = CreateHookMock(afterStepEvents);

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "undefined", null, null);

            await methodBindingInvokerMock.DidNotReceive().InvokeBindingAsync(afterStepMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>());
        }

        [Fact]
        public async Task Should_cleanup_scenario_context_on_scenario_end()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            testExecutionEngine.OnScenarioInitialize(scenarioInfo);
            await testExecutionEngine.OnScenarioStartAsync();
            await testExecutionEngine.OnScenarioEndAsync();

            contextManagerStub.Received(1).CleanupScenarioContext();
        }

        [Fact]
        public async Task Should_cleanup_scenario_context_after_AfterScenario_hook_error()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var afterHook = CreateParametrizedHookMock(afterScenarioEvents, typeof(DummyClass));
            var hookMock = CreateHookMock(afterScenarioEvents);
            methodBindingInvokerMock.InvokeBindingAsync(hookMock, contextManagerStub, null, testTracerStub, Arg.Any<DurationHolder>())
                                    .Throws(new Exception("simulated error"));


            testExecutionEngine.OnScenarioInitialize(scenarioInfo);
            await testExecutionEngine.OnScenarioStartAsync();
            Func<Task> act = async () => await testExecutionEngine.OnScenarioEndAsync();

            await act.Should().ThrowAsync<Exception>().WithMessage("simulated error");
            contextManagerStub.Received(1).CleanupScenarioContext();
        }

        [Fact]
        public async Task Should_resolve_FeatureContext_hook_parameter()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var hookMock = CreateParametrizedHookMock(beforeFeatureEvents, typeof(FeatureContext));

            await testExecutionEngine.OnFeatureStartAsync(featureInfo);
            AssertHooksWasCalledWithParam(hookMock, contextManagerStub.FeatureContext);
        }

        [Fact]
        public async Task Should_resolve_custom_class_hook_parameter()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var hookMock = CreateParametrizedHookMock(beforeFeatureEvents, typeof(DummyClass));

            await testExecutionEngine.OnFeatureStartAsync(featureInfo);
            AssertHooksWasCalledWithParam(hookMock, DummyClass.LastInstance);
        }

        [Fact]
        public async Task Should_resolve_container_hook_parameter()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var hookMock = CreateParametrizedHookMock(beforeTestRunEvents, typeof(IObjectContainer));

            await testExecutionEngine.OnTestRunStartAsync();

            AssertHooksWasCalledWithParam(hookMock, globalContainer);
        }

        [Fact]
        public async Task Should_resolve_multiple_hook_parameter()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var hookMock = CreateParametrizedHookMock(beforeFeatureEvents, typeof(DummyClass), typeof(FeatureContext));

            await testExecutionEngine.OnFeatureStartAsync(featureInfo);
            AssertHooksWasCalledWithParam(hookMock, DummyClass.LastInstance);
            AssertHooksWasCalledWithParam(hookMock, contextManagerStub.FeatureContext);
        }

        [Fact]
        public async Task Should_resolve_BeforeAfterTestRun_hook_parameter_from_test_thread_container()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var beforeHook = CreateParametrizedHookMock(beforeTestRunEvents, typeof(DummyClass));
            var afterHook = CreateParametrizedHookMock(afterTestRunEvents, typeof(DummyClass));

            await testExecutionEngine.OnTestRunStartAsync();
            await testExecutionEngine.OnTestRunEndAsync();

            AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
            AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
            testObjectResolverMock.Received(2).ResolveBindingInstance(typeof(DummyClass), globalContainer);
        }

        [Fact]
        public async Task Should_resolve_BeforeAfterScenario_hook_parameter_from_scenario_container()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var beforeHook = CreateParametrizedHookMock(beforeScenarioEvents, typeof(DummyClass));
            var afterHook = CreateParametrizedHookMock(afterScenarioEvents, typeof(DummyClass));

            testExecutionEngine.OnScenarioInitialize(scenarioInfo);
            await testExecutionEngine.OnScenarioStartAsync();
            await testExecutionEngine.OnScenarioEndAsync();

            AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
            AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
            testObjectResolverMock.Received(2).ResolveBindingInstance(typeof(DummyClass), scenarioContainer);
        }

        [Fact]
        public async Task Should_be_possible_to_register_instance_in_scenario_container_before_firing_scenario_events()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            var instanceToAddBeforeScenarioEventFiring = new AnotherDummyClass();
            var beforeHook = CreateParametrizedHookMock(beforeScenarioEvents, typeof(DummyClass));

            // Setup binding method mock so it attempts to resolve an instance from the scenario container.
            // If this fails, then the instance was not registered before the method was invoked.
            AnotherDummyClass actualInstance = null;
            methodBindingInvokerMock.When(m => m
                .InvokeBindingAsync(Arg.Any<IBinding>(), Arg.Any<IContextManager>(),
                    Arg.Any<object[]>(), Arg.Any<ITestTracer>(), Arg.Any<DurationHolder>()))
                                    .Do(_ => actualInstance = testExecutionEngine.ScenarioContext.ScenarioContainer.Resolve<AnotherDummyClass>());

            testExecutionEngine.OnScenarioInitialize(scenarioInfo);
            testExecutionEngine.ScenarioContext.ScenarioContainer.RegisterInstanceAs(instanceToAddBeforeScenarioEventFiring);
            await testExecutionEngine.OnScenarioStartAsync();
            actualInstance.Should().BeSameAs(instanceToAddBeforeScenarioEventFiring);

            AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
        }

        [Fact]
        public async Task Should_resolve_BeforeAfterScenarioBlock_hook_parameter_from_scenario_container()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var beforeHook = CreateParametrizedHookMock(beforeScenarioBlockEvents, typeof(DummyClass));
            var afterHook = CreateParametrizedHookMock(afterScenarioBlockEvents, typeof(DummyClass));

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
            await testExecutionEngine.OnAfterLastStepAsync();

            AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
            AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
            testObjectResolverMock.Received(2).ResolveBindingInstance(typeof(DummyClass), scenarioContainer);
        }

        [Fact]
        public async Task Should_resolve_BeforeAfterStep_hook_parameter_from_scenario_container()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var beforeHook = CreateParametrizedHookMock(beforeStepEvents, typeof(DummyClass));
            var afterHook = CreateParametrizedHookMock(afterStepEvents, typeof(DummyClass));

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
            AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
            testObjectResolverMock.Received(2).ResolveBindingInstance(typeof(DummyClass), scenarioContainer);
        }

        [Fact]
        public async Task Should_resolve_BeforeAfterFeature_hook_parameter_from_feature_container()
        {
            var testExecutionEngine = CreateTestExecutionEngine();
            RegisterStepDefinition();

            var beforeHook = CreateParametrizedHookMock(beforeFeatureEvents, typeof(DummyClass));
            var afterHook = CreateParametrizedHookMock(afterFeatureEvents, typeof(DummyClass));

            await testExecutionEngine.OnFeatureStartAsync(featureInfo);
            await testExecutionEngine.OnFeatureEndAsync();

            AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
            AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
            testObjectResolverMock.Received(2).ResolveBindingInstance(typeof(DummyClass), featureContainer);
        }

        [Fact]
        public async Task Should_TryToSend_ProjectRunningEvent()
        {
            _analyticsTransmitter.IsEnabled.Returns(true);

            var testExecutionEngine = CreateTestExecutionEngine();

            await testExecutionEngine.OnTestRunStartAsync();

            await _analyticsTransmitter.Received(1).TransmitReqnrollProjectRunningEventAsync(Arg.Any<ReqnrollProjectRunningEvent>());
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(3, 1)]
        public async Task Should_execute_all_ISkippedStepHandlers_for_each_skipped_step(int numberOfHandlers, int numberOfSkippedSteps)
        {
            var sut = CreateTestExecutionEngine();
            scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

            var skippedStepHandlerMocks = new List<ISkippedStepHandler>();
            for (int i = 0; i < numberOfHandlers; i++)
            {
                var mockHandler = Substitute.For<ISkippedStepHandler>();
                mockHandler.Handle(Arg.Any<ScenarioContext>());
                skippedStepHandlerMocks.Add(mockHandler);
                scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler, i.ToString());
            }

            for (int i = 0; i < numberOfSkippedSteps; i++)
            {
                RegisterStepDefinition();
                await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
            }

            foreach (var handler in skippedStepHandlerMocks)
            {
                handler.Received(numberOfSkippedSteps).Handle(Arg.Any<ScenarioContext>());
            }
        }

        [Fact]
        public async Task Should_not_change_ScenarioExecutionStatus_on_dummy_ISkippedStepHandler()
        {
            var sut = CreateTestExecutionEngine();
            scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

            var mockHandler = Substitute.For<ISkippedStepHandler>();
            mockHandler.When(m => m.Handle(Arg.Any<ScenarioContext>()))
                       .Do(_ => Console.WriteLine("ISkippedStepHandler"));
            scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler);

            RegisterStepDefinition();
            await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            scenarioContext.ScenarioExecutionStatus.Should().Be(ScenarioExecutionStatus.TestError);
        }

        [Fact]
        public async Task Should_not_call_ISkippedStepHandler_on_UndefinedStepDefinition()
        {
            var sut = CreateTestExecutionEngine();
            scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

            var mockHandler = Substitute.For<ISkippedStepHandler>();
            mockHandler.Handle(Arg.Any<ScenarioContext>());
            scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler);

            RegisterUndefinedStepDefinition();
            await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            mockHandler.DidNotReceive().Handle(Arg.Any<ScenarioContext>());
        }

        [Fact]
        public async Task Should_not_call_ISkippedStepHandler_on_succesfull_test_run()
        {
            var sut = CreateTestExecutionEngine();

            var mockHandler = Substitute.For<ISkippedStepHandler>();
            mockHandler.Handle(Arg.Any<ScenarioContext>());
            scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler);

            RegisterStepDefinition();
            await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            mockHandler.DidNotReceive().Handle(Arg.Any<ScenarioContext>());
        }

        [Fact]
        public async Task Should_not_call_ISkippedStepHandler_if_only_last_step_is_failing()
        {
            var sut = CreateTestExecutionEngine();

            var mockHandler = Substitute.For<ISkippedStepHandler>();
            mockHandler.When(m => m.Handle(Arg.Any<ScenarioContext>()))
                       .Do(_ => Console.WriteLine("ISkippedStepHandler"));

            scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler);

            RegisterFailingStepDefinition();
            await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            mockHandler.DidNotReceive().Handle(Arg.Any<ScenarioContext>());
        }

        [Fact]
        public async Task Should_set_correct_duration_in_case_of_failed_step()
        {
            TimeSpan executionDuration = TimeSpan.Zero;
            testTracerStub.When(m => m.TraceError(Arg.Any<Exception>(), Arg.Any<TimeSpan>()))
                          .Do(args => executionDuration = args.Arg<TimeSpan>());

            var testExecutionEngine = CreateTestExecutionEngine();

            TimeSpan expectedDuration = TimeSpan.FromSeconds(5);
            RegisterFailingStepDefinition(expectedDuration);

            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            testTracerStub.Received(1).TraceError(Arg.Any<Exception>(), Arg.Any<TimeSpan>());
            executionDuration.Should().Be(expectedDuration);
        }

        [Fact]
        public async Task Should_set_correct_duration_in_case_of_passed_step()
        {
            TimeSpan executionDuration = TimeSpan.Zero;
            testTracerStub.When(m => m.TraceStepDone(Arg.Any<BindingMatch>(), Arg.Any<object[]>(), Arg.Any<TimeSpan>()))
                          .Do(args => executionDuration = args.Arg<TimeSpan>());

            var testExecutionEngine = CreateTestExecutionEngine();

            TimeSpan expectedDuration = TimeSpan.FromSeconds(5);

            var stepDefStub = RegisterStepDefinition();

            methodBindingInvokerMock.InvokeBindingAsync(stepDefStub, contextManagerStub, Arg.Any<object[]>(), testTracerStub, Arg.Any<DurationHolder>())
                                    .Returns(args =>
                                    {
                                        args.Arg<DurationHolder>().Duration = expectedDuration;
                                        return new object();
                                    });


            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

            testTracerStub.Received(1).TraceStepDone(Arg.Any<BindingMatch>(), Arg.Any<object[]>(), Arg.Any<TimeSpan>());
            executionDuration.Should().Be(expectedDuration);
        }
    }
}