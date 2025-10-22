using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.BoDi;
using Reqnroll.Bindings;
using Reqnroll.Configuration;
using Reqnroll.Tracing;
using System.Threading.Tasks;

namespace Reqnroll.Infrastructure;

public class ContextManager : IContextManager, IAsyncDisposable
{
    private class InternalContextManager<TContext>(ITestTracer testTracer) : IAsyncDisposable
        where TContext : ReqnrollContext
    {
        private IObjectContainer _objectContainer;

        public TContext Instance { get; private set; }

        public async Task InitAsync(TContext newInstance, IObjectContainer newObjectContainer)
        {
            if (Instance != null)
            {
                testTracer.TraceWarning($"The previous {typeof(TContext).Name} was not disposed.");
                await DisposeInstanceAsync();
            }
            Instance = newInstance;
            _objectContainer = newObjectContainer;
        }

        public ValueTask CleanupAsync()
        {
            if (Instance == null)
            {
                testTracer.TraceWarning($"The previous {typeof(TContext).Name} was already disposed.");
                return default;
            }

            return DisposeInstanceAsync();
        }

        private async ValueTask DisposeInstanceAsync()
        {
            if (_objectContainer != null)
            {
                await _objectContainer.DisposeAsync();
            }

            Instance = null;
            _objectContainer = null;
        }

        public ValueTask DisposeAsync()
        {
            if (Instance != null)
            {
                return DisposeInstanceAsync();
            }

            return default;
        }
    }

    /// <summary>
    /// Implementation of internal context manager which keeps a stack of contexts, rather than a single one.
    /// This allows the contexts to be used when a new context is created before the previous context has been completed
    /// which is what happens when a step calls other steps. This means that the step contexts will be reported
    /// correctly even when there is a nesting of steps calling steps.
    /// </summary>
    /// <typeparam name="TContext">A type derived from ReqnrollContext, which needs to be managed  in a way</typeparam>
    private class StackedInternalContextManager<TContext>(ITestTracer testTracer) : IDisposable
        where TContext : ReqnrollContext
    {
        private readonly Stack<TContext> _instances = new();

        public TContext Instance => IsEmpty ? null : _instances.Peek();

        public bool IsEmpty => !_instances.Any();

        public void Push(TContext newInstance)
        {
            _instances.Push(newInstance);
        }

        public void RemoveTop()
        {
            if (IsEmpty)
            {
                testTracer.TraceWarning($"The previous {typeof(TContext).Name} was already disposed.");
                return;
            }
            var instance = _instances.Pop();
            ((IDisposable)instance).Dispose();
        }

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            while (!IsEmpty)
            {
                RemoveTop();
            }
        }
    }

    private readonly IObjectContainer _testThreadContainer;
    private readonly InternalContextManager<ScenarioContext> _scenarioContextManager;
    private readonly InternalContextManager<FeatureContext> _featureContextManager;
    private readonly StackedInternalContextManager<ScenarioStepContext> _stepContextManager;
    private readonly IContainerBuilder _containerBuilder;

    /// <summary>
    /// Holds the StepDefinitionType of the last step that was executed from the actual feature file, excluding the types of the steps that were executed during the calling of a step
    /// </summary>
    public StepDefinitionType? CurrentTopLevelStepDefinitionType { get; private set; }

    public ContextManager(ITestTracer testTracer, IObjectContainer testThreadContainer, IContainerBuilder containerBuilder)
    {
        _featureContextManager = new InternalContextManager<FeatureContext>(testTracer);
        _scenarioContextManager = new InternalContextManager<ScenarioContext>(testTracer);
        _stepContextManager = new StackedInternalContextManager<ScenarioStepContext>(testTracer);
        _testThreadContainer = testThreadContainer;
        _containerBuilder = containerBuilder;

        InitializeTestThreadContext();
    }

    public FeatureContext FeatureContext => _featureContextManager.Instance;

    public ScenarioContext ScenarioContext => _scenarioContextManager.Instance;

    public ScenarioStepContext StepContext => _stepContextManager.Instance;

    public TestThreadContext TestThreadContext { get; private set; }

    private void RegisterInstanceAsInterfaceAndObjectType<TInterface, TObject>(IObjectContainer objectContainer, TObject instance, string name = null, bool dispose = false)
        where TInterface : class
        where TObject : class, TInterface
    {
        // We need two registrations, but both should have the same "dispose" setting,
        // because otherwise the second would override the first one.
        objectContainer.RegisterInstanceAs<TInterface>(instance, name, dispose);
        objectContainer.RegisterInstanceAs(instance, name, dispose);
    }

    private void InitializeTestThreadContext()
    {
        // Since both TestThreadContext and ContextManager are in the same container (test thread container)
        // their lifetime is the same, so we do not need the swap infrastructure like for the other contexts.
        // We just need to initialize it during construction time.
        var testThreadContext = new TestThreadContext(_testThreadContainer);
        // make sure that the TestThreadContext can also be resolved through the interface as well
        RegisterInstanceAsInterfaceAndObjectType<ITestThreadContext, TestThreadContext>(_testThreadContainer, testThreadContext, dispose: true);
        TestThreadContext = testThreadContext;
    }

    public async Task InitializeFeatureContextAsync(FeatureInfo featureInfo)
    {
        var featureContainer = _containerBuilder.CreateFeatureContainer(_testThreadContainer, featureInfo);
        var reqnrollConfiguration = _testThreadContainer.Resolve<ReqnrollConfiguration>();
        var newContext = new FeatureContext(featureContainer, featureInfo, reqnrollConfiguration);
        // make sure that the FeatureContext can also be resolved through the interface as well
        RegisterInstanceAsInterfaceAndObjectType<IFeatureContext, FeatureContext>(featureContainer, newContext, dispose: true);
        await _featureContextManager.InitAsync(newContext, featureContainer);
#pragma warning disable 618
        FeatureContext.Current = newContext;
#pragma warning restore 618
    }

    public ValueTask CleanupFeatureContextAsync()
    {
        return _featureContextManager.CleanupAsync();
    }

    public async Task InitializeScenarioContextAsync(ScenarioInfo scenarioInfo, RuleInfo ruleInfo)
    {
        var scenarioContainer = _containerBuilder.CreateScenarioContainer(FeatureContext.FeatureContainer, scenarioInfo);
        var testObjectResolver = scenarioContainer.Resolve<ITestObjectResolver>();
        var newContext = new ScenarioContext(scenarioContainer, scenarioInfo, ruleInfo, testObjectResolver);
        // make sure that the ScenarioContext can also be resolved through the interface as well
        RegisterInstanceAsInterfaceAndObjectType<IScenarioContext, ScenarioContext>(scenarioContainer, newContext, dispose: true);
        await _scenarioContextManager.InitAsync(newContext, scenarioContainer);
#pragma warning disable 618
        ScenarioContext.Current = newContext;
#pragma warning restore 618

        ResetCurrentStepStack();
    }

    private void ResetCurrentStepStack()
    {
        _stepContextManager.Reset();
        CurrentTopLevelStepDefinitionType = null;
        ScenarioStepContext.Current = null;
    }

    public ValueTask CleanupScenarioContextAsync()
    {
        return _scenarioContextManager.CleanupAsync();
    }

    public void InitializeStepContext(StepInfo stepInfo)
    {
        if (_stepContextManager.IsEmpty) // top-level step comes
            CurrentTopLevelStepDefinitionType = stepInfo.StepDefinitionType;
        var newContext = new ScenarioStepContext(stepInfo);
        _stepContextManager.Push(newContext);
        ScenarioStepContext.Current = newContext;
    }

    public void CleanupStepContext()
    {
        _stepContextManager.RemoveTop();
        ScenarioStepContext.Current = _stepContextManager.Instance;
        // we do not reset CurrentTopLevelStepDefinitionType in order to "remember" last top level type for "And" and "But" steps
    }
        
    public async ValueTask DisposeAsync()
    {
        if (_featureContextManager != null)
        {
            await _featureContextManager.DisposeAsync();
        }

        if (_scenarioContextManager != null)
        {
            await _scenarioContextManager.DisposeAsync();
        }

        _stepContextManager?.Dispose();
    }
}