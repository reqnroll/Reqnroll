using FluentAssertions;
using Moq;
using Reqnroll.Analytics;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.BindingSkeletons;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.ErrorHandling;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.TestFramework;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure;

public partial class TestExecutionEngineTests
{
    private readonly ScenarioContext _scenarioContext;
    private readonly ReqnrollConfiguration _reqnrollConfiguration;
    private readonly Mock<IEnvironmentOptions> _environmentOptions;
    private readonly Mock<IBindingRegistry> _bindingRegistryStub;
    private readonly Mock<IErrorProvider> _errorProviderStub;
    private readonly Mock<IContextManager> _contextManagerStub;
    private readonly Mock<ITestTracer> _testTracerStub;
    private readonly Mock<IStepDefinitionMatchService> _stepDefinitionMatcherStub;
    private readonly Mock<IAsyncBindingInvoker> _methodBindingInvokerMock;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Mock<IStepDefinitionSkeletonProvider> _stepDefinitionSkeletonProviderMock;
    private readonly Mock<ITestObjectResolver> _testObjectResolverMock;
    private readonly Mock<IObsoleteStepHandler> _obsoleteTestHandlerMock;
    private readonly FeatureInfo _featureInfo;
    private readonly ScenarioInfo _scenarioInfo;
    private readonly RuleInfo _ruleInfo;
    private readonly ObjectContainer _globalContainer;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ObjectContainer _testThreadContainer;
    private readonly ObjectContainer _featureContainer;
    private readonly ObjectContainer _scenarioContainer;
    private readonly DefaultTestRunContext _testRunContext;
    private readonly TestObjectResolver _defaultTestObjectResolver = new();
    private readonly ITestPendingMessageFactory _testPendingMessageFactory;
    private readonly ITestUndefinedMessageFactory _testUndefinedMessageFactory;
    private readonly Mock<IAnalyticsRuntimeTelemetryService> _telemetryService;
    private readonly Mock<IRuntimePluginTestExecutionLifecycleEventEmitter> _runtimePluginTestExecutionLifecycleEventEmitter;
    private readonly Mock<ITestThreadExecutionEventPublisher> _testThreadExecutionEventPublisher;
    private readonly Mock<IStepArgumentTypeConverter> _stepArgumentTypeConverterMock;
    private readonly Mock<IUnitTestRuntimeProvider> _unitTestRuntimeProviderStub;

    private readonly List<IHookBinding> _beforeScenarioEvents;
    private readonly List<IHookBinding> _afterScenarioEvents;
    private readonly List<IHookBinding> _beforeStepEvents;
    private readonly List<IHookBinding> _afterStepEvents;
    private readonly List<IHookBinding> _beforeFeatureEvents;
    private readonly List<IHookBinding> _afterFeatureEvents;
    private readonly List<IHookBinding> _beforeTestRunEvents;
    private readonly List<IHookBinding> _afterTestRunEvents;
    private readonly List<IHookBinding> _beforeScenarioBlockEvents;
    private readonly List<IHookBinding> _afterScenarioBlockEvents;
    private readonly List<IStepArgumentTransformationBinding> _stepTransformations;

    class DummyClass
    {
        public static DummyClass LastInstance = null;
        public DummyClass()
        {
            LastInstance = this;
        }
    }

    class AnotherDummyClass;


    public TestExecutionEngineTests()
    {
        _reqnrollConfiguration = ConfigurationLoader.GetDefault();

        _environmentOptions = new Mock<IEnvironmentOptions>();

        _globalContainer = new ObjectContainer();
        _testThreadContainer = new ObjectContainer(_globalContainer);
        _featureContainer = new ObjectContainer(_testThreadContainer);
        _scenarioContainer = new ObjectContainer(_featureContainer);
        _testRunContext = new DefaultTestRunContext(_globalContainer, new Mock<ITestRunSettingsProvider>().Object);

        _beforeScenarioEvents = new List<IHookBinding>();
        _afterScenarioEvents = new List<IHookBinding>();
        _beforeStepEvents = new List<IHookBinding>();
        _afterStepEvents = new List<IHookBinding>();
        _beforeFeatureEvents = new List<IHookBinding>();
        _afterFeatureEvents = new List<IHookBinding>();
        _beforeTestRunEvents = new List<IHookBinding>();
        _afterTestRunEvents = new List<IHookBinding>();
        _beforeScenarioBlockEvents = new List<IHookBinding>();
        _afterScenarioBlockEvents = new List<IHookBinding>();

        _stepTransformations = new List<IStepArgumentTransformationBinding>();

        _stepDefinitionSkeletonProviderMock = new Mock<IStepDefinitionSkeletonProvider>();
        _testObjectResolverMock = new Mock<ITestObjectResolver>();
        _testObjectResolverMock.Setup(bir => bir.ResolveBindingInstance(It.IsAny<Type>(), It.IsAny<IObjectContainer>()))
                              .Returns((Type t, IObjectContainer container) => _defaultTestObjectResolver.ResolveBindingInstance(t, container));

        var culture = new CultureInfo("en-US", false);
        _contextManagerStub = new Mock<IContextManager>();
        _scenarioInfo = new ScenarioInfo("scenario_title", "scenario_description", null, null);
        _ruleInfo = new RuleInfo("rule_title", "rule_description", null);
        _scenarioContext = new ScenarioContext(_scenarioContainer, _scenarioInfo, _ruleInfo, _testObjectResolverMock.Object);
        _scenarioContainer.RegisterInstanceAs(_scenarioContext);
        _contextManagerStub.Setup(cm => cm.ScenarioContext).Returns(_scenarioContext);
        _featureInfo = new FeatureInfo(culture, "feature path", "feature_title", "", ProgrammingLanguage.CSharp);
        var featureContext = new FeatureContext(_featureContainer, _featureInfo, _reqnrollConfiguration);
        _featureContainer.RegisterInstanceAs(featureContext);
        _contextManagerStub.Setup(cm => cm.FeatureContext).Returns(featureContext);
        _contextManagerStub.Setup(cm => cm.StepContext).Returns(new ScenarioStepContext(new StepInfo(StepDefinitionType.Given, "step_title", null, null)));

        _bindingRegistryStub = new Mock<IBindingRegistry>();
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.BeforeStep)).Returns(_beforeStepEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.AfterStep)).Returns(_afterStepEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.BeforeScenarioBlock)).Returns(_beforeScenarioBlockEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.AfterScenarioBlock)).Returns(_afterScenarioBlockEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.BeforeFeature)).Returns(_beforeFeatureEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.AfterFeature)).Returns(_afterFeatureEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.BeforeTestRun)).Returns(_beforeTestRunEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.AfterTestRun)).Returns(_afterTestRunEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.BeforeScenario)).Returns(_beforeScenarioEvents);
        _bindingRegistryStub.Setup(br => br.GetHooks(HookType.AfterScenario)).Returns(_afterScenarioEvents);
            
        _bindingRegistryStub.Setup(br => br.GetStepTransformations()).Returns(_stepTransformations);
        _bindingRegistryStub.Setup(br => br.IsValid).Returns(true);

        _errorProviderStub = new Mock<IErrorProvider>();
        _testTracerStub = new Mock<ITestTracer>();
        _stepDefinitionMatcherStub = new Mock<IStepDefinitionMatchService>();
        _methodBindingInvokerMock = new Mock<IAsyncBindingInvoker>();

        _obsoleteTestHandlerMock = new Mock<IObsoleteStepHandler>();

        _testPendingMessageFactory = new TestPendingMessageFactory();
        _testUndefinedMessageFactory = new TestUndefinedMessageFactory(_stepDefinitionSkeletonProviderMock.Object, _errorProviderStub.Object, _reqnrollConfiguration);

        _telemetryService = new Mock<IAnalyticsRuntimeTelemetryService>();

        var testRunnerManager = new Mock<ITestRunnerManager>();
        testRunnerManager.Setup(trm => trm.TestAssembly).Returns(Assembly.GetCallingAssembly);

        _runtimePluginTestExecutionLifecycleEventEmitter = new Mock<IRuntimePluginTestExecutionLifecycleEventEmitter>();
        _testThreadExecutionEventPublisher = new Mock<ITestThreadExecutionEventPublisher>();

        _unitTestRuntimeProviderStub = new Mock<IUnitTestRuntimeProvider>();
        _stepArgumentTypeConverterMock = new Mock<IStepArgumentTypeConverter>();
    }

    private TestExecutionEngine CreateTestExecutionEngine()
    {
        return new TestExecutionEngine(
            new Mock<IStepFormatter>().Object,
            _testTracerStub.Object,
            _errorProviderStub.Object,
            _stepArgumentTypeConverterMock.Object,
            _reqnrollConfiguration,
            _environmentOptions.Object,
            _bindingRegistryStub.Object,
            _unitTestRuntimeProviderStub.Object,
            _contextManagerStub.Object,
            _stepDefinitionMatcherStub.Object,
            _methodBindingInvokerMock.Object,
            _obsoleteTestHandlerMock.Object,
            _telemetryService.Object,
            _runtimePluginTestExecutionLifecycleEventEmitter.Object,
            _testThreadExecutionEventPublisher.Object,
            _testPendingMessageFactory,
            _testUndefinedMessageFactory,
            _testObjectResolverMock.Object,
            _testRunContext);
    }

    private Mock<IStepDefinitionBinding> RegisterStepDefinition()
    {
        var methodStub = new Mock<IBindingMethod>();
        var stepDefStub = new Mock<IStepDefinitionBinding>();
        stepDefStub.Setup(sd => sd.Method).Returns(methodStub.Object);

        StepDefinitionAmbiguityReason ambiguityReason;
        List<BindingMatch> candidatingMatches;
        _stepDefinitionMatcherStub.Setup(sdm => sdm.GetBestMatch(It.IsAny<StepInstance>(), It.IsAny<CultureInfo>(), out ambiguityReason, out candidatingMatches))
                                 .Returns(
                                     new BindingMatch(stepDefStub.Object, 0, [], new StepContext("bla", "foo", new List<string>(), CultureInfo.InvariantCulture)));

        return stepDefStub;
    }

    private void RegisterStepDefinitionWithParameter()
    {
        var methodStub = new Mock<IBindingMethod>();
        methodStub.Setup(m => m.Parameters).Returns([new BindingParameter(new RuntimeBindingType(typeof(int)), "p1")]);
        var stepDefStub = new Mock<IStepDefinitionBinding>();
        stepDefStub.Setup(sd => sd.Method).Returns(methodStub.Object);

        StepDefinitionAmbiguityReason ambiguityReason;
        List<BindingMatch> candidatingMatches;
        _stepDefinitionMatcherStub.Setup(sdm => sdm.GetBestMatch(It.IsAny<StepInstance>(), It.IsAny<CultureInfo>(), out ambiguityReason, out candidatingMatches))
                                 .Returns(
                                     new BindingMatch(stepDefStub.Object, 0, [new MatchArgument(42)], new StepContext("bla", "foo", new List<string>(), CultureInfo.InvariantCulture)));
    }

    private void RegisterStepDefinitionWithWrongArgumentCount()
    {
        var methodStub = new Mock<IBindingMethod>();
        var stepDefStub = new Mock<IStepDefinitionBinding>();
        stepDefStub.Setup(sd => sd.Method).Returns(methodStub.Object);

        StepDefinitionAmbiguityReason ambiguityReason;
        List<BindingMatch> candidatingMatches;
        _stepDefinitionMatcherStub.Setup(sdm => sdm.GetBestMatch(It.IsAny<StepInstance>(), It.IsAny<CultureInfo>(), out ambiguityReason, out candidatingMatches))
                                 .Returns(
                                     new BindingMatch(stepDefStub.Object, 0, [new MatchArgument(42)], new StepContext("bla", "foo", new List<string>(), CultureInfo.InvariantCulture)));
    }

    private void RegisterStepDefinitionWithTransformation(IBindingType bindingType)
    {            
        var bindingParameterStub = new Mock<IBindingParameter>();
        bindingParameterStub.Setup(bp => bp.Type).Returns(bindingType);
        var methodStub = new Mock<IBindingMethod>();
        methodStub.Setup(m => m.Parameters).Returns([bindingParameterStub.Object]);
        var stepDefStub = new Mock<IStepDefinitionBinding>();
        stepDefStub.Setup(sd => sd.Method).Returns(methodStub.Object);

        StepDefinitionAmbiguityReason ambiguityReason;
        List<BindingMatch> candidatingMatches;
        _stepDefinitionMatcherStub.Setup(sdm => sdm.GetBestMatch(It.IsAny<StepInstance>(), It.IsAny<CultureInfo>(), out ambiguityReason, out candidatingMatches))
                                 .Returns(
                                     new BindingMatch(stepDefStub.Object, 0, [new MatchArgument("username", 1)], new StepContext("bla", "foo", new List<string>(), CultureInfo.InvariantCulture)));
    }

    private void RegisterUndefinedStepDefinition()
    {
        var methodStub = new Mock<IBindingMethod>();
        var stepDefStub = new Mock<IStepDefinitionBinding>();
        stepDefStub.Setup(sd => sd.Method).Returns(methodStub.Object);

        StepDefinitionAmbiguityReason ambiguityReason = StepDefinitionAmbiguityReason.None;
        List<BindingMatch> candidatingMatches = new();
        _stepDefinitionMatcherStub.Setup(sdm => sdm.GetBestMatch(It.IsAny<StepInstance>(), It.IsAny<CultureInfo>(), out ambiguityReason, out candidatingMatches))
                                 .Returns(BindingMatch.NonMatching);
    }

    private void RegisterAmbiguousStepDefinition()
    {
        var methodStub1 = new Mock<IBindingMethod>();
        var stepDefStub1 = new Mock<IStepDefinitionBinding>();
        stepDefStub1.Setup(sd => sd.Method).Returns(methodStub1.Object);

        var methodStub2 = new Mock<IBindingMethod>();
        var stepDefStub2 = new Mock<IStepDefinitionBinding>();
        stepDefStub2.Setup(sd => sd.Method).Returns(methodStub2.Object);

        var stepContext = new StepContext("bla", "foo", new List<string>(), CultureInfo.InvariantCulture);

        StepDefinitionAmbiguityReason ambiguityReason = StepDefinitionAmbiguityReason.AmbiguousSteps;
        List<BindingMatch> candidatingMatches = [
            new(stepDefStub1.Object, 0, [], stepContext),
            new(stepDefStub2.Object, 0, [], stepContext)
        ];
        _stepDefinitionMatcherStub.Setup(sdm => sdm.GetBestMatch(It.IsAny<StepInstance>(), It.IsAny<CultureInfo>(), out ambiguityReason, out candidatingMatches))
                                 .Returns(new BindingMatch(null, 0, [], stepContext));
    }

    private void RegisterFailingStepDefinition(TimeSpan? expectedDuration = null, Exception exceptionToThrow = null)
    {
        var stepDefStub = RegisterStepDefinition();

        _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(stepDefStub.Object, _contextManagerStub.Object, It.IsAny<object[]>(), _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                .Callback((IBinding _, IContextManager _, object[] _, ITestTracer _, DurationHolder durationHolder) =>
                                {
                                    if (expectedDuration.HasValue)
                                    {
                                        durationHolder.Duration = expectedDuration.Value;
                                    }
                                })
                                .ThrowsAsync(exceptionToThrow ?? new InvalidOperationException("simulated error"));
    }

    private void RegisterPendingStepDefinition()
    {
        RegisterFailingStepDefinition(exceptionToThrow: new PendingStepException());
    }

    private Mock<IHookBinding> CreateHookMock(List<IHookBinding> hookList)
    {
        var mock = new Mock<IHookBinding>();
        hookList.Add(mock.Object);
        return mock;
    }

    private Mock<IHookBinding> CreateParametrizedHookMock(List<IHookBinding> hookList, params Type[] paramTypes)
    {
        var hookMock = CreateHookMock(hookList);
        var bindingMethod = new BindingMethod(new BindingType("AssemblyBT", "BT", "Test.BT"), "X",
                                              paramTypes.Select((paramType, i) => new BindingParameter(new RuntimeBindingType(paramType), "p" + i)),
                                              RuntimeBindingType.Void);
        hookMock.Setup(h => h.Method).Returns(bindingMethod);
        return hookMock;
    }

    private IStepArgumentTransformationBinding CreateStepTransformationBinding(string regexString, IBindingMethod transformMethod)
    {
        return new StepArgumentTransformationBinding(regexString, transformMethod);
    }

    private void AssertHooksWasCalledWithParam(Mock<IHookBinding> hookMock, object paramObj)
    {
        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object,
                                                                  It.Is((object[] args) => args != null && args.Length > 0 && args.Any(arg => arg == paramObj)),
                                                                  _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Once());
    }

    public enum StepExecutionUseCase
    {
        PassingStep,
        FailingStep,
        SkippedBecauseOfPreviousError,
        UndefinedStep,
        PendingStepDefinition,
        DynamicallyPendingStepDefinition, // e.g. throws a not implemented exception
        DynamicallySkippedStepDefinition, // e.g. Assert.Ignore called
        ObsoleteStepDefinitionAsError,
        ObsoleteStepDefinitionAsPending,
        AmbiguousStep,
        InvalidStepDefinitionArguments, // e.g. wrong argument count
        ArgumentConversionError,
        ArgumentExceptionInStepDefinition,
        InvalidBindingRegistry,
        BeforeStepHookError,
        AfterStepHookError,
        StepBlockHookError,
    }

    [Theory]                        //UseCase                        StopAtFirst                   ExpectedStatus           ExpectedError                        customErrorStatus                 Hooks?  Events? Skipped? Throws?
    // This first set of Use Cases with StopAtFirstError set to false (the default)
    [InlineData(StepExecutionUseCase.PassingStep,                       false, ScenarioExecutionStatus.OK,                    null,                                null,                            true,  true,  false, false)]
    [InlineData(StepExecutionUseCase.FailingStep,                       false, ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   null,                            true,  true,  false, false)]
    [InlineData(StepExecutionUseCase.SkippedBecauseOfPreviousError,     false, ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   ScenarioExecutionStatus.Skipped, false, true,  true,  false)]
    [InlineData(StepExecutionUseCase.UndefinedStep,                     false, ScenarioExecutionStatus.UndefinedStep,         null,                                null,                            false, true,  false, false)]
    [InlineData(StepExecutionUseCase.PendingStepDefinition,             false, ScenarioExecutionStatus.StepDefinitionPending, typeof(PendingStepException),        null,                            true,  true,  false, false)]
    [InlineData(StepExecutionUseCase.DynamicallyPendingStepDefinition,  false, ScenarioExecutionStatus.StepDefinitionPending, typeof(NotImplementedException),     null,                            true,  true,  false, false)]
    [InlineData(StepExecutionUseCase.DynamicallySkippedStepDefinition,  false, ScenarioExecutionStatus.Skipped,               typeof(OperationCanceledException),  null,                            true,  true,  false, false)]
    [InlineData(StepExecutionUseCase.ObsoleteStepDefinitionAsError,     false, ScenarioExecutionStatus.BindingError,          typeof(BindingException),            null,                            false, true,  false, false)]
    [InlineData(StepExecutionUseCase.ObsoleteStepDefinitionAsPending,   false, ScenarioExecutionStatus.StepDefinitionPending, typeof(PendingStepException),        null,                            false, true,  false, false)]
    [InlineData(StepExecutionUseCase.AmbiguousStep,                     false, ScenarioExecutionStatus.BindingError,          typeof(AmbiguousBindingException),   null,                            false, true,  false, false)]
    [InlineData(StepExecutionUseCase.InvalidStepDefinitionArguments,    false, ScenarioExecutionStatus.BindingError,          typeof(BindingException),            null,                            false, true,  false, false)]
    [InlineData(StepExecutionUseCase.ArgumentConversionError,           false, ScenarioExecutionStatus.TestError,             typeof(FormatException),             null,                            false, true,  false, false)]
    [InlineData(StepExecutionUseCase.ArgumentExceptionInStepDefinition, false, ScenarioExecutionStatus.BindingError,          typeof(BindingException),            null,                            true,  true,  false, false)]
    [InlineData(StepExecutionUseCase.InvalidBindingRegistry,            false, ScenarioExecutionStatus.BindingError,          typeof(BindingException),            null,                            false, true,  false, false)]
    [InlineData(StepExecutionUseCase.BeforeStepHookError,               false, ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   null,                            true,  true,  false, false)]
    [InlineData(StepExecutionUseCase.AfterStepHookError,                false, ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   ScenarioExecutionStatus.OK,      true,  true,  false, true)]
    [InlineData(StepExecutionUseCase.StepBlockHookError,                false, ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   ScenarioExecutionStatus.OK,      false, false, false, true)]
    // Repeat the Use Cases with StopAtFirstError set to true
    [InlineData(StepExecutionUseCase.PassingStep,                       true,  ScenarioExecutionStatus.OK,                    null,                                null,                            true,  true,  false, false)]
    [InlineData(StepExecutionUseCase.FailingStep,                       true,  ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   null,                            true,  true,  false, true)]
    [InlineData(StepExecutionUseCase.SkippedBecauseOfPreviousError,     true,  ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   ScenarioExecutionStatus.Skipped, false, true,  true,  false)]
    [InlineData(StepExecutionUseCase.UndefinedStep,                     true,  ScenarioExecutionStatus.UndefinedStep,         null,                                null,                            false, true,  false, false)]
    [InlineData(StepExecutionUseCase.PendingStepDefinition,             true,  ScenarioExecutionStatus.StepDefinitionPending, typeof(PendingStepException),        null,                            true,  true,  false, true)]
    [InlineData(StepExecutionUseCase.DynamicallyPendingStepDefinition,  true,  ScenarioExecutionStatus.StepDefinitionPending, typeof(NotImplementedException),     null,                            true,  true,  false, true)]
    [InlineData(StepExecutionUseCase.DynamicallySkippedStepDefinition,  true,  ScenarioExecutionStatus.Skipped,               typeof(OperationCanceledException),  null,                            true,  true,  false, true)]
    [InlineData(StepExecutionUseCase.ObsoleteStepDefinitionAsError,     true,  ScenarioExecutionStatus.BindingError,          typeof(BindingException),            null,                            false, true,  false, true)]
    [InlineData(StepExecutionUseCase.ObsoleteStepDefinitionAsPending,   true,  ScenarioExecutionStatus.StepDefinitionPending, typeof(PendingStepException),        null,                            false, true,  false, true)]
    [InlineData(StepExecutionUseCase.AmbiguousStep,                     true,  ScenarioExecutionStatus.BindingError,          typeof(AmbiguousBindingException),   null,                            false, true,  false, true)]
    [InlineData(StepExecutionUseCase.InvalidStepDefinitionArguments,    true,  ScenarioExecutionStatus.BindingError,          typeof(BindingException),            null,                            false, true,  false, true)]
    [InlineData(StepExecutionUseCase.ArgumentConversionError,           true,  ScenarioExecutionStatus.TestError,             typeof(FormatException),             null,                            false, true,  false, true)]
    [InlineData(StepExecutionUseCase.ArgumentExceptionInStepDefinition, true,  ScenarioExecutionStatus.BindingError,          typeof(BindingException),            null,                            true,  true,  false, true)]
    [InlineData(StepExecutionUseCase.InvalidBindingRegistry,            true,  ScenarioExecutionStatus.BindingError,          typeof(BindingException),            null,                            false, true,  false, true)]
    [InlineData(StepExecutionUseCase.BeforeStepHookError,               true,  ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   null,                            true,  true,  false, true)]
    [InlineData(StepExecutionUseCase.AfterStepHookError,                true,  ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   ScenarioExecutionStatus.OK,      true,  true,  false, true)]
    [InlineData(StepExecutionUseCase.StepBlockHookError,                true,  ScenarioExecutionStatus.TestError,             typeof(InvalidOperationException),   ScenarioExecutionStatus.OK,      false, false, false, true)]
    public async Task Should_execute_step(StepExecutionUseCase useCase, bool configuredStopAtFirstError, ScenarioExecutionStatus expectedStatus, Type expectedErrorType, ScenarioExecutionStatus? customExpectedStepStatus, 
        bool shouldRunStepHooks, bool shouldPublishStepStartedFinishedEvent, bool shouldPublishStepSkippedEvent, bool shouldThrow)
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        _reqnrollConfiguration.StopAtFirstError = configuredStopAtFirstError;

        var beforeStepHookMock = CreateHookMock(_beforeStepEvents);
        var afterStepHookMock = CreateHookMock(_afterStepEvents);

        switch (useCase)
        {
            case StepExecutionUseCase.FailingStep:
                RegisterFailingStepDefinition();
                break;
            case StepExecutionUseCase.PendingStepDefinition:
                RegisterPendingStepDefinition();
                break;
            case StepExecutionUseCase.DynamicallyPendingStepDefinition:
                RegisterFailingStepDefinition(exceptionToThrow: new NotImplementedException("simulated"));
                break;
            case StepExecutionUseCase.DynamicallySkippedStepDefinition:
                _unitTestRuntimeProviderStub.Setup(utp => utp.DetectExecutionStatus(It.Is<Exception>(e => e is OperationCanceledException)))
                                            .Returns(ScenarioExecutionStatus.Skipped);
                RegisterFailingStepDefinition(exceptionToThrow: new OperationCanceledException("simulated"));
                break;
            case StepExecutionUseCase.ArgumentExceptionInStepDefinition:
                // BindingInvoker wraps ArgumentException to BindingException
                RegisterFailingStepDefinition(exceptionToThrow: new BindingException("call error", new ArgumentException("simulated")));
                break;
            case StepExecutionUseCase.InvalidStepDefinitionArguments:
                _errorProviderStub.Setup(ep => ep.GetParameterCountError(It.IsAny<BindingMatch>(), It.IsAny<int>()))
                                 .Returns(new BindingException("invalid parameter count"));
                RegisterStepDefinitionWithWrongArgumentCount();
                break;
            case StepExecutionUseCase.ArgumentConversionError:
                _stepArgumentTypeConverterMock.Setup(sac => sac.ConvertAsync(It.IsAny<object>(), It.IsAny<IBindingType>(), It.IsAny<CultureInfo>()))
                                              .ThrowsAsync(new FormatException("conversion error"));
                RegisterStepDefinitionWithParameter();
                break;
            case StepExecutionUseCase.SkippedBecauseOfPreviousError:
                _scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;
                _scenarioContext.TestError = new InvalidOperationException("earlier error");
                goto default;
            case StepExecutionUseCase.ObsoleteStepDefinitionAsError:
                _obsoleteTestHandlerMock.Setup(oth => oth.Handle(It.IsAny<BindingMatch>()))
                                       .Throws(new BindingException("obsolete"));
                goto default;
            case StepExecutionUseCase.ObsoleteStepDefinitionAsPending:
                _obsoleteTestHandlerMock.Setup(oth => oth.Handle(It.IsAny<BindingMatch>()))
                                       .Throws(new PendingStepException("obsolete"));
                goto default;
            case StepExecutionUseCase.InvalidBindingRegistry:
                _errorProviderStub.Setup(ep => ep.GetInvalidBindingRegistryError(It.IsAny<IEnumerable<BindingError>>()))
                                 .Returns(new BindingException("invalid binding registry"));
                _bindingRegistryStub.Setup(r => r.IsValid).Returns(false);
                goto default;
            case StepExecutionUseCase.UndefinedStep:
                _errorProviderStub.Setup(ep => ep.GetMissingStepDefinitionError()).Returns(new MissingStepDefinitionException());
                RegisterUndefinedStepDefinition();
                break;
            case StepExecutionUseCase.AmbiguousStep:
                _errorProviderStub.Setup(ep => ep.GetAmbiguousMatchError(It.IsAny<List<BindingMatch>>(), It.IsAny<StepInstance>()))
                                 .Returns((List<BindingMatch> matches, StepInstance _) => new AmbiguousBindingException("Ambiguous step", matches));
                RegisterAmbiguousStepDefinition();
                break;
            case StepExecutionUseCase.BeforeStepHookError:
                _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(beforeStepHookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                        .Throws(new InvalidOperationException("simulated before step hook error"));
                goto default;
            case StepExecutionUseCase.AfterStepHookError:
                _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(afterStepHookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                        .Throws(new InvalidOperationException("simulated after step hook error"));
                goto default;
            case StepExecutionUseCase.StepBlockHookError:
                var stepBlockHookMock = CreateHookMock(_beforeScenarioBlockEvents);
                _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(stepBlockHookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                        .Throws(new InvalidOperationException("simulated before step block hook error"));
                goto default;
            default:
                RegisterStepDefinition();
                break;
        }

        Exception actualExceptionThrown = null;
        try
        {
            await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
        }
        catch (Exception ex)
        {
            actualExceptionThrown = ex;
        }

        _contextManagerStub.Object.StepContext.Status.Should().Be(customExpectedStepStatus ?? expectedStatus);

        _scenarioContext.ScenarioExecutionStatus.Should().Be(expectedStatus);
        if (expectedErrorType == null) 
            _scenarioContext.TestError.Should().BeNull();
        else
            _scenarioContext.TestError.Should().BeOfType(expectedErrorType);

        if (shouldThrow)
        {
            actualExceptionThrown.Should().NotBeNull();
            actualExceptionThrown.Should().BeOfType(expectedErrorType);
        }
        else
            actualExceptionThrown.Should().BeNull();

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(beforeStepHookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()),
                                        shouldRunStepHooks ? Times.Once() : Times.Never());
        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(afterStepHookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()),
                                        shouldRunStepHooks ? Times.Once() : Times.Never());

        _testThreadExecutionEventPublisher.Verify(te => te.PublishEventAsync(It.IsAny<StepStartedEvent>()),
                                                  shouldPublishStepStartedFinishedEvent ? Times.Once() : Times.Never());
        _testThreadExecutionEventPublisher.Verify(te => te.PublishEventAsync(It.IsAny<StepFinishedEvent>()),
                                                  shouldPublishStepStartedFinishedEvent ? Times.Once() : Times.Never());
        _testThreadExecutionEventPublisher.Verify(te => te.PublishEventAsync(It.IsAny<StepSkippedEvent>()),
                                                  shouldPublishStepSkippedEvent ? Times.Once() : Times.Never());
    }


    [Fact]
    public async Task Should_execute_before_step()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateHookMock(_beforeStepEvents);

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Once());
    }

    [Fact]
    public async Task Should_execute_after_step()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateHookMock(_afterStepEvents);

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Once());
    }

    [Fact]
    public async Task Should_not_execute_step_when_there_was_an_error_earlier()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        var stepDefMock = RegisterStepDefinition();

        _scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(stepDefMock.Object, It.IsAny<IContextManager>(), It.IsAny<object[]>(), It.IsAny<ITestTracer>(), It.IsAny<DurationHolder>()), Times.Never());
    }

    [Fact]
    public async Task Should_not_execute_step_hooks_when_there_was_an_error_earlier()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        _scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

        var beforeStepMock = CreateHookMock(_beforeStepEvents);
        var afterStepMock = CreateHookMock(_afterStepEvents);

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(beforeStepMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Never());
        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(afterStepMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Never());
    }

    [Fact]
    public async Task Should_not_execute_step_argument_transformations_when_there_was_an_error_earlier()
    {
        var testExecutionEngine = CreateTestExecutionEngine();

        var bindingTypeStub = new Mock<IBindingType>();
        RegisterStepDefinitionWithTransformation(bindingTypeStub.Object);

        _scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

        UserCreator stepTransformationInstance = new UserCreator();
        var transformMethod = new RuntimeBindingMethod(stepTransformationInstance.GetType().GetMethod("Create"));
        var stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);
        _stepTransformations.Add(stepTransformationBinding);

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "user bar", null, null);

        _stepArgumentTypeConverterMock.Verify(i => i.ConvertAsync(It.IsAny<object>(), It.IsAny<IBindingType>(), It.IsAny<CultureInfo>()), Times.Never);
    }

    [Fact]
    public async Task Should_execute_after_step_when_step_definition_failed()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterFailingStepDefinition();

        var hookMock = CreateHookMock(_afterStepEvents);

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()));
    }

    [Fact]
    public async Task Should_cleanup_step_context_when_before_scenario_block_hook_error()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateHookMock(_beforeScenarioBlockEvents);
        _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                .Throws(new Exception("simulated before block hook error"));

        await FluentActions.Awaiting(() => testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null))
                           .Should().ThrowAsync<Exception>("execution of the step should have failed because of the exception thrown by the before scenario block hook");

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Once());
        _contextManagerStub.Verify(cm => cm.CleanupStepContext());

        _contextManagerStub.Object.ScenarioContext.ScenarioExecutionStatus.Should().Be(ScenarioExecutionStatus.TestError);
        _contextManagerStub.Object.ScenarioContext.TestError?.Message.Should().Be("simulated before block hook error");
    }

    [Fact]
    public async Task Should_cleanup_step_context_when_after_scenario_block_hook_error()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateHookMock(_afterScenarioBlockEvents);
        _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                .Throws(new Exception("simulated after block hook error"));

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
        await FluentActions.Awaiting(() => testExecutionEngine.StepAsync(StepDefinitionKeyword.When, null, "bar", null, null))
                           .Should().ThrowAsync<Exception>("execution of the step should have failed because of the exception thrown by the before scenario block hook");

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Once());
        _contextManagerStub.Verify(cm => cm.CleanupStepContext());

        _contextManagerStub.Object.ScenarioContext.ScenarioExecutionStatus.Should().Be(ScenarioExecutionStatus.TestError);
        _contextManagerStub.Object.ScenarioContext.TestError?.Message.Should().Be("simulated after block hook error");
    }

    [Fact]
    public async Task Should_cleanup_step_context_when_before_step_hook_error()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateHookMock(_beforeStepEvents);
        _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                .Throws(new Exception("simulated before step hook error"));

        _reqnrollConfiguration.StopAtFirstError = true;
        await FluentActions.Awaiting(() => testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null))
                           .Should().ThrowAsync<Exception>("execution of the step should have failed because of the exception thrown by the before scenario block hook");

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Once());
        _contextManagerStub.Verify(cm => cm.CleanupStepContext());

        _contextManagerStub.Object.ScenarioContext.ScenarioExecutionStatus.Should().Be(ScenarioExecutionStatus.TestError);
        _contextManagerStub.Object.ScenarioContext.TestError?.Message.Should().Be("simulated before step hook error");
    }

    [Fact]
    public async Task Should_cleanup_step_context_when_after_step_hook_error()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateHookMock(_afterStepEvents);
        _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                .Throws(new Exception("simulated after step hook error"));

        await FluentActions.Awaiting(() => testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null))
                           .Should().ThrowAsync<Exception>("execution of the step should have failed because of the exception thrown by the before scenario block hook");

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Once());
        _contextManagerStub.Verify(cm => cm.CleanupStepContext());

        _contextManagerStub.Object.ScenarioContext.ScenarioExecutionStatus.Should().Be(ScenarioExecutionStatus.TestError);
        _contextManagerStub.Object.ScenarioContext.TestError?.Message.Should().Be("simulated after step hook error");
    }

    [Fact]
    public async Task Should_not_execute_after_step_when_step_is_undefined()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterUndefinedStepDefinition();
        _errorProviderStub.Setup(ep => ep.GetMissingStepDefinitionError()).Returns(new MissingStepDefinitionException());


        var afterStepMock = CreateHookMock(_afterStepEvents);

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "undefined", null, null);

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(afterStepMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Never());
    }
        
    [Fact]
    public async Task Should_cleanup_scenario_context_on_scenario_end()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
        await testExecutionEngine.OnScenarioStartAsync();
        await testExecutionEngine.OnScenarioEndAsync();

        _contextManagerStub.Verify(cm => cm.CleanupScenarioContext(), Times.Once);
    }

    [Fact]
    public async Task Should_cleanup_scenario_context_after_AfterScenario_hook_error()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        CreateParametrizedHookMock(_afterScenarioEvents, typeof(DummyClass));
        var hookMock = CreateHookMock(_afterScenarioEvents);
        _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                .Throws(new Exception("simulated error"));


        testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
        await testExecutionEngine.OnScenarioStartAsync();
        Func<Task> act = async () => await testExecutionEngine.OnScenarioEndAsync();

        await act.Should().ThrowAsync<Exception>().WithMessage("simulated error");
        _contextManagerStub.Verify(cm => cm.CleanupScenarioContext(), Times.Once);
    }

    [Fact]
    public async Task Should_resolve_FeatureContext_hook_parameter()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateParametrizedHookMock(_beforeFeatureEvents, typeof(FeatureContext));

        await testExecutionEngine.OnFeatureStartAsync(_featureInfo);
        AssertHooksWasCalledWithParam(hookMock, _contextManagerStub.Object.FeatureContext);
    }

    [Fact]
    public async Task Should_resolve_custom_class_hook_parameter()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateParametrizedHookMock(_beforeFeatureEvents, typeof(DummyClass));

        await testExecutionEngine.OnFeatureStartAsync(_featureInfo);
        AssertHooksWasCalledWithParam(hookMock, DummyClass.LastInstance);
    }

    [Fact]
    public async Task Should_resolve_container_hook_parameter()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateParametrizedHookMock(_beforeTestRunEvents, typeof(IObjectContainer));

        await testExecutionEngine.OnTestRunStartAsync();

        AssertHooksWasCalledWithParam(hookMock, _globalContainer);
    }

    [Fact]
    public async Task Should_resolve_multiple_hook_parameter()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateParametrizedHookMock(_beforeFeatureEvents, typeof(DummyClass), typeof(FeatureContext));

        await testExecutionEngine.OnFeatureStartAsync(_featureInfo);
        AssertHooksWasCalledWithParam(hookMock, DummyClass.LastInstance);
        AssertHooksWasCalledWithParam(hookMock, _contextManagerStub.Object.FeatureContext);
    }

    [Fact]
    public async Task Should_resolve_BeforeAfterTestRun_hook_parameter_from_test_thread_container()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var beforeHook = CreateParametrizedHookMock(_beforeTestRunEvents, typeof(DummyClass));
        var afterHook = CreateParametrizedHookMock(_afterTestRunEvents, typeof(DummyClass));

        await testExecutionEngine.OnTestRunStartAsync();
        await testExecutionEngine.OnTestRunEndAsync();

        AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
        AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
        _testObjectResolverMock.Verify(bir => bir.ResolveBindingInstance(typeof(DummyClass), _globalContainer),
                                      Times.Exactly(2));
    }

    [Fact]
    public async Task Should_resolve_BeforeAfterScenario_hook_parameter_from_scenario_container()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var beforeHook = CreateParametrizedHookMock(_beforeScenarioEvents, typeof(DummyClass));
        var afterHook = CreateParametrizedHookMock(_afterScenarioEvents, typeof(DummyClass));

        testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
        await testExecutionEngine.OnScenarioStartAsync();
        await testExecutionEngine.OnScenarioEndAsync();

        AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
        AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
        _testObjectResolverMock.Verify(bir => bir.ResolveBindingInstance(typeof(DummyClass), _scenarioContainer),
                                      Times.Exactly(2));
    }

    [Fact]
    public async Task Should_be_possible_to_register_instance_in_scenario_container_before_firing_scenario_events()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        var instanceToAddBeforeScenarioEventFiring = new AnotherDummyClass();
        var beforeHook = CreateParametrizedHookMock(_beforeScenarioEvents, typeof(DummyClass));

        // Setup binding method mock so it attempts to resolve an instance from the scenario container.
        // If this fails, then the instance was not registered before the method was invoked.
        AnotherDummyClass actualInstance = null;
        _methodBindingInvokerMock.Setup(s => s.InvokeBindingAsync(It.IsAny<IBinding>(), It.IsAny<IContextManager>(),
                                                                 It.IsAny<object[]>(),It.IsAny<ITestTracer>(), It.IsAny<DurationHolder>()))
                                .Callback(() => actualInstance = testExecutionEngine.ScenarioContext.ScenarioContainer.Resolve<AnotherDummyClass>());

        testExecutionEngine.OnScenarioInitialize(_scenarioInfo, _ruleInfo);
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

        var beforeHook = CreateParametrizedHookMock(_beforeScenarioBlockEvents, typeof(DummyClass));
        var afterHook = CreateParametrizedHookMock(_afterScenarioBlockEvents, typeof(DummyClass));

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
        await testExecutionEngine.OnAfterLastStepAsync();

        AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
        AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
        _testObjectResolverMock.Verify(bir => bir.ResolveBindingInstance(typeof(DummyClass), _scenarioContainer),
                                      Times.Exactly(2));
    }

    [Fact]
    public async Task Should_resolve_BeforeAfterStep_hook_parameter_from_scenario_container()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var beforeHook = CreateParametrizedHookMock(_beforeStepEvents, typeof(DummyClass));
        var afterHook = CreateParametrizedHookMock(_afterStepEvents, typeof(DummyClass));

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
        AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
        _testObjectResolverMock.Verify(bir => bir.ResolveBindingInstance(typeof(DummyClass), _scenarioContainer),
                                      Times.Exactly(2));
    }

    [Fact]
    public async Task Should_cleanup_feature_context_when_after_feature_hook_error()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var hookMock = CreateHookMock(_afterFeatureEvents);
        _methodBindingInvokerMock.Setup(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()))
                                .Throws(new Exception("simulated after feature hook error"));

        await testExecutionEngine.OnFeatureStartAsync(_featureInfo);
        await FluentActions.Awaiting(testExecutionEngine.OnFeatureEndAsync)
                           .Should().ThrowAsync<Exception>("execution of the step should have failed because of the exception thrown by the before scenario block hook");

        _methodBindingInvokerMock.Verify(i => i.InvokeBindingAsync(hookMock.Object, _contextManagerStub.Object, null, _testTracerStub.Object, It.IsAny<DurationHolder>()), Times.Once());
        _contextManagerStub.Verify(cm => cm.CleanupFeatureContext());
    }

    [Fact]
    public async Task Should_resolve_BeforeAfterFeature_hook_parameter_from_feature_container()
    {
        var testExecutionEngine = CreateTestExecutionEngine();
        RegisterStepDefinition();

        var beforeHook = CreateParametrizedHookMock(_beforeFeatureEvents, typeof(DummyClass));
        var afterHook = CreateParametrizedHookMock(_afterFeatureEvents, typeof(DummyClass));

        await testExecutionEngine.OnFeatureStartAsync(_featureInfo);
        await testExecutionEngine.OnFeatureEndAsync();

        AssertHooksWasCalledWithParam(beforeHook, DummyClass.LastInstance);
        AssertHooksWasCalledWithParam(afterHook, DummyClass.LastInstance);
        _testObjectResolverMock.Verify(bir => bir.ResolveBindingInstance(typeof(DummyClass), _featureContainer),
                                      Times.Exactly(2));
    }

    [Fact]
    public async Task Should_TryToSend_ProjectRunningEvent()
    {
        var testExecutionEngine = CreateTestExecutionEngine();

        await testExecutionEngine.OnTestRunStartAsync();

        _telemetryService.Verify(ts => ts.SendProjectRunningEvent(), Times.Once);
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(3, 1)]
    public async Task Should_execute_all_ISkippedStepHandlers_for_each_skipped_step(int numberOfHandlers, int numberOfSkippedSteps)
    {
        var sut = CreateTestExecutionEngine();
        _scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

        var skippedStepHandlerMocks = new List<Mock<ISkippedStepHandler>>();
        for (int i = 0; i < numberOfHandlers; i++)
        {
            var mockHandler = new Mock<ISkippedStepHandler>();
            mockHandler.Setup(b => b.Handle(It.IsAny<ScenarioContext>())).Verifiable();
            skippedStepHandlerMocks.Add(mockHandler);
            _scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler.Object, i.ToString());
        }

        for (int i = 0; i < numberOfSkippedSteps; i++)
        {
            RegisterStepDefinition();
            await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
        }

        foreach (var handler in skippedStepHandlerMocks)
        {
            handler.Verify(action => action.Handle(It.IsAny<ScenarioContext>()), Times.Exactly(numberOfSkippedSteps));
        }
    }

    [Fact]
    public async Task Should_not_change_ScenarioExecutionStatus_on_dummy_ISkippedStepHandler()
    {
        var sut = CreateTestExecutionEngine();
        _scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

        var mockHandler = new Mock<ISkippedStepHandler>();
        mockHandler.Setup(b => b.Handle(It.IsAny<ScenarioContext>())).Callback(() => Console.WriteLine("ISkippedStepHandler"));
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler.Object);

        RegisterStepDefinition();
        await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        _scenarioContext.ScenarioExecutionStatus.Should().Be(ScenarioExecutionStatus.TestError);
    }

    [Fact]
    public async Task Should_not_call_ISkippedStepHandler_on_UndefinedStepDefinition()
    {
        var sut = CreateTestExecutionEngine();
        _scenarioContext.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError;

        _errorProviderStub.Setup(ep => ep.GetMissingStepDefinitionError()).Returns(new MissingStepDefinitionException());

        var mockHandler = new Mock<ISkippedStepHandler>();
        mockHandler.Setup(b => b.Handle(It.IsAny<ScenarioContext>())).Verifiable();
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler.Object);

        RegisterUndefinedStepDefinition();
        await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        mockHandler.Verify(action => action.Handle(It.IsAny<ScenarioContext>()), Times.Never);
    }

    [Fact]
    public async Task Should_not_call_ISkippedStepHandler_on_successful_test_run()
    {
        var sut = CreateTestExecutionEngine();

        var mockHandler = new Mock<ISkippedStepHandler>();
        mockHandler.Setup(b => b.Handle(It.IsAny<ScenarioContext>())).Verifiable();
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler.Object);

        RegisterStepDefinition();
        await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        mockHandler.Verify(action => action.Handle(It.IsAny<ScenarioContext>()), Times.Never);
    }

    [Fact]
    public async Task Should_not_call_ISkippedStepHandler_if_only_last_step_is_failing()
    {
        var sut = CreateTestExecutionEngine();

        var mockHandler = new Mock<ISkippedStepHandler>();
        mockHandler.Setup(b => b.Handle(It.IsAny<ScenarioContext>())).Callback(() => Console.WriteLine("ISkippedStepHandler"));
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(mockHandler.Object);

        RegisterFailingStepDefinition();
        await sut.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        mockHandler.Verify(action => action.Handle(It.IsAny<ScenarioContext>()), Times.Never);
    }

    [Fact]
    public async Task Should_set_correct_duration_in_case_of_failed_step()
    {
        TimeSpan executionDuration = TimeSpan.Zero;
        _testTracerStub.Setup(c => c.TraceError(It.IsAny<Exception>(), It.IsAny<TimeSpan>()))
                      .Callback<Exception, TimeSpan>((_, duration) => executionDuration = duration);

        var testExecutionEngine = CreateTestExecutionEngine();

        TimeSpan expectedDuration = TimeSpan.FromSeconds(5);
        RegisterFailingStepDefinition(expectedDuration);

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);
            
        _testTracerStub.Verify(tracer => tracer.TraceError(It.IsAny<Exception>(), It.IsAny<TimeSpan>()), Times.Once());
        executionDuration.Should().Be(expectedDuration);
    }

    [Fact]
    public async Task Should_set_correct_duration_in_case_of_passed_step()
    {
        TimeSpan executionDuration = TimeSpan.Zero;
        _testTracerStub.Setup(c => c.TraceStepDone(It.IsAny<BindingMatch>(), It.IsAny<object[]>(), It.IsAny<TimeSpan>()))
                      .Callback<BindingMatch, object[], TimeSpan>((_, _, duration) => executionDuration = duration);

        var testExecutionEngine = CreateTestExecutionEngine();

        TimeSpan expectedDuration = TimeSpan.FromSeconds(5);

        var stepDefStub = RegisterStepDefinition();
        _methodBindingInvokerMock
            .Setup(i => i.InvokeBindingAsync(stepDefStub.Object, _contextManagerStub.Object, It.IsAny<object[]>(), _testTracerStub.Object, It.IsAny<DurationHolder>()))
            .Callback((IBinding _, IContextManager _, object[] _, ITestTracer _, DurationHolder durationHolder) => durationHolder.Duration = expectedDuration)
            .ReturnsAsync(new object());

        await testExecutionEngine.StepAsync(StepDefinitionKeyword.Given, null, "foo", null, null);

        _testTracerStub.Verify(tracer => tracer.TraceStepDone(It.IsAny<BindingMatch>(), It.IsAny<object[]>(), It.IsAny<TimeSpan>()), Times.Once());
        executionDuration.Should().Be(expectedDuration);
    }
}
