using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Reqnroll.BoDi;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Configuration;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using System.Runtime.ExceptionServices;

namespace Reqnroll;

public class TestRunnerManager : ITestRunnerManager
{
    protected readonly IObjectContainer _globalContainer;
    protected readonly IContainerBuilder _containerBuilder;
    protected readonly ReqnrollConfiguration _reqnrollConfiguration;
    protected readonly IRuntimeBindingRegistryBuilder _bindingRegistryBuilder;
    protected readonly ITestTracer _testTracer;

    /// <summary>
    /// Contains hints to for the test runner manager to choose the test runners from multiple available
    /// ones in a more optimal way. Our goal is to keep the test runner "sticky" to the test framework workers
    /// so that no unnecessary feature context switches occur.
    /// Currently, we remember the feature info and the managed thread ID to help this.
    /// </summary>
    class TestWorkerContainerHint(FeatureInfo lastUsedFeatureInfo, int? releasedOnManagedThreadId)
    {
        public FeatureInfo LastUsedFeatureInfo { get; } = lastUsedFeatureInfo;

        public int? ReleasedOnManagedThreadId { get; } = releasedOnManagedThreadId;

        /// <summary>
        /// Returns information about how optimal the provided hint for the current situation. Smaller number means more optimal.
        /// </summary>
        public static int GetDistance(TestWorkerContainerHint hint, FeatureInfo featureHint)
        {
            int distance = 0;
            distance += hint?.LastUsedFeatureInfo == null || featureHint == null || !ReferenceEquals(hint.LastUsedFeatureInfo, featureHint) ?
                2 : 0;
            distance += hint?.ReleasedOnManagedThreadId == null || hint.ReleasedOnManagedThreadId != Thread.CurrentThread.ManagedThreadId ? 
                1 : 0;
            return distance;
        }
    }

    private readonly ConcurrentDictionary<IObjectContainer, TestWorkerContainerHint> _availableTestWorkerContainers = new();
    private readonly ConcurrentDictionary<IObjectContainer, TestWorkerContainerHint> _usedTestWorkerContainers = new();
    private int _nextTestWorkerContainerId;

    public bool IsTestRunInitialized { get; private set; }
    private int _wasDisposed = 0;
    private int _wasSingletonInstanceDisabled = 0;
    private readonly object _createTestRunnerLockObject = new();
    private volatile ITestRunner _globalTestRunner;

    public Assembly TestAssembly { get; private set; }
    public Assembly[] BindingAssemblies { get; private set; }

    public bool IsMultiThreaded => GetWorkerTestRunnerCount() > 1;

    public TestRunnerManager(IObjectContainer globalContainer, IContainerBuilder containerBuilder, ReqnrollConfiguration reqnrollConfiguration, IRuntimeBindingRegistryBuilder bindingRegistryBuilder,
        ITestTracer testTracer)
    {
        _globalContainer = globalContainer;
        _containerBuilder = containerBuilder;
        _reqnrollConfiguration = reqnrollConfiguration;
        _bindingRegistryBuilder = bindingRegistryBuilder;
        _testTracer = testTracer;
    }

    private int GetWorkerTestRunnerCount()
    {
        var hasTestRunStartWorker = _globalTestRunner != null;
        return _usedTestWorkerContainers.Count - (hasTestRunStartWorker ? 1 : 0);
    }

    public virtual ITestRunner GetOrCreateTestRunner(FeatureInfo featureHint = null)
    {
        var testRunner = GetOrCreateTestRunnerInstance(featureHint);

        if (!IsTestRunInitialized)
        {
            lock (_createTestRunnerLockObject)
            {
                if (!IsTestRunInitialized)
                {
                    InitializeBindingRegistry(testRunner);
                    IsTestRunInitialized = true;
                }
            }
        }

        return testRunner;
    }

    public virtual void ReleaseTestRunnerToPool(ITestRunner testRunner)
    {
        var testThreadContainer = testRunner.TestThreadContext.TestThreadContainer;
        if (!_usedTestWorkerContainers.TryRemove(testThreadContainer, out _))
            throw new InvalidOperationException($"TestThreadContext with id {TestThreadContainerInfo.GetId(testThreadContainer)} was already released");
        // We construct the container hint based on the current feature context of the runner. If the runner performed the feature closing procedure already, that will be null
        var containerHint = new TestWorkerContainerHint(testRunner.FeatureContext?.FeatureInfo, Thread.CurrentThread.ManagedThreadId);
        if (!_availableTestWorkerContainers.TryAdd(testThreadContainer, containerHint))
            throw new InvalidOperationException($"TestThreadContext with id {TestThreadContainerInfo.GetId(testThreadContainer)} was released twice");
    }

    public virtual async Task EndFeatureForAvailableTestRunnersAsync(FeatureInfo featureInfo)
    {
        var items = _availableTestWorkerContainers.ToArray();
        foreach (var item in items)
        {
            if (!ReferenceEquals(featureInfo, item.Value.LastUsedFeatureInfo))
                continue; // This is for a different feature
            if (!_availableTestWorkerContainers.TryRemove(item.Key, out _))
                continue; // Container was already taken by another thread
            await EndFeatureForTestRunnerAsync(item.Key, featureInfo);
        }
    }

    private async Task EndFeatureForTestRunnerAsync(IObjectContainer testThreadContainer, FeatureInfo featureInfo)
    {
        if (!_usedTestWorkerContainers.TryAdd(testThreadContainer, new TestWorkerContainerHint(featureInfo, null)))
            throw new InvalidOperationException($"TestThreadContext with id {TestThreadContainerInfo.GetId(testThreadContainer)} is already in usage");

        var testRunner = testThreadContainer.Resolve<ITestRunner>();
        try
        {
            // The feature info in the hint (item.Value.LastUsedFeatureInfo) may be outdated, so we need to check against the TestRunner instance
            if (testRunner.FeatureContext != null && ReferenceEquals(testRunner.FeatureContext.FeatureInfo, featureInfo))
                await testRunner.OnFeatureEndAsync();
        }
        finally
        {
            ReleaseTestRunnerToPool(testRunner);
        }
    }

    protected virtual void InitializeBindingRegistry(ITestRunner testRunner)
    {
        BindingAssemblies = _bindingRegistryBuilder.GetBindingAssemblies(TestAssembly);
        BuildBindingRegistry(BindingAssemblies);

        void DomainUnload(object sender, EventArgs e)
        {
            OnDomainUnloadAsync().Wait();
        }

        AppDomain.CurrentDomain.DomainUnload += DomainUnload;
        AppDomain.CurrentDomain.ProcessExit += DomainUnload;
    }

    protected virtual void BuildBindingRegistry(IEnumerable<Assembly> bindingAssemblies)
    {
        foreach (Assembly assembly in bindingAssemblies)
        {
            _bindingRegistryBuilder.BuildBindingsFromAssembly(assembly);
        }
        _bindingRegistryBuilder.BuildingCompleted();
    }

    protected internal virtual async Task OnDomainUnloadAsync()
    {
        await DisposeAsync();
    }

    ITestRunner GetOrCreateGlobalTestRunner()
    {
        if (_globalTestRunner == null)
        {
            lock (_availableTestWorkerContainers)
            {
                if (_globalTestRunner == null)
                    _globalTestRunner = GetTestRunner();
            }
        }
        return _globalTestRunner;
    }

    public async Task FireTestRunEndAsync()
    {
        // this method must not be called multiple times
        var onTestRunnerEndExecutionHost = GetOrCreateGlobalTestRunner();
        if (onTestRunnerEndExecutionHost != null)
            await onTestRunnerEndExecutionHost.OnTestRunEndAsync();
    }

    public async Task FireTestRunStartAsync()
    {
        // this method must not be called multiple times
        var onTestRunnerStartExecutionHost = GetOrCreateGlobalTestRunner();
        if (onTestRunnerStartExecutionHost != null)
            await onTestRunnerStartExecutionHost.OnTestRunStartAsync();
    }

    private bool TryGetTestRunnerFromAvailableTestWorkerContainers(FeatureInfo featureHint, out IObjectContainer testThreadContainer)
    {
        testThreadContainer = null;

        for (int i = 0; i < 5; i++) // Try to get an available Container max 5 times
        {
            var items = _availableTestWorkerContainers.ToArray();
            if (items.Length == 0)
                return false; // No Containers are available

            var featuresInUse = _usedTestWorkerContainers
                .Select(it => it.Value.LastUsedFeatureInfo)
                .Where(fi => fi != null)
                .Distinct()
                .ToArray();

            // take all containers that are bound to our feature or not bound to any
            var sameFeatureOrUnboundContainers = items
                .Where(it => it.Value.LastUsedFeatureInfo == featureHint || it.Value.LastUsedFeatureInfo == null);

            // take all from others, except one from each that is "reserved" to finish that feature
            var otherContainers = items
              .Where(it => it.Value.LastUsedFeatureInfo != featureHint && it.Value.LastUsedFeatureInfo != null)
              .GroupBy(it => it.Value.LastUsedFeatureInfo)
              .SelectMany(g => featuresInUse.Contains(g.Key) ? 
                g : // if the feature is in use, we anyway have the "reserved" runner already
                g.OrderByDescending(it => TestWorkerContainerHint.GetDistance(it.Value, featureHint)).Skip(1));

            // put all these to a priority list
            var prioritizedContainers = sameFeatureOrUnboundContainers.Concat(otherContainers)
                .OrderBy(it => TestWorkerContainerHint.GetDistance(it.Value, featureHint))
                .Select(it => it.Key);

            bool containersTested = false;

            foreach (var container in prioritizedContainers)
            {
                containersTested = true;
                if (!_availableTestWorkerContainers.TryRemove(container, out _))
                    continue; // Container was already taken by another thread
                testThreadContainer = container;
                return true;
            }

            // if there was no item in 'prioritizedContainers', we don't even retry
            if (!containersTested) 
                return false;
        }
        
        return false;
    }

    protected virtual ITestRunner GetOrCreateTestRunnerInstance(FeatureInfo featureHint = null)
    {
        if (TryGetTestRunnerFromAvailableTestWorkerContainers(featureHint, out var testThreadContainer))
        {
            // We found an available test runner. Select it to prevent a new test thread context from being created.
        }
        else
        {
            // A free test runner was not found. Create one.
            testThreadContainer = _containerBuilder.CreateTestThreadContainer(_globalContainer);
            var id = Interlocked.Increment(ref _nextTestWorkerContainerId);
            var testThreadContainerInfo = new TestThreadContainerInfo(id.ToString(CultureInfo.InvariantCulture));
            testThreadContainer.RegisterInstanceAs(testThreadContainerInfo);
        }

        if (!_usedTestWorkerContainers.TryAdd(testThreadContainer, new TestWorkerContainerHint(featureHint, null)))
            throw new InvalidOperationException($"TestThreadContext with id {TestThreadContainerInfo.GetId(testThreadContainer)} is already in usage");

        return testThreadContainer.Resolve<ITestRunner>();
    }

    public void Initialize(Assembly assignedTestAssembly)
    {
        TestAssembly = assignedTestAssembly;
    }

    public virtual ITestRunner GetTestRunner(FeatureInfo featureHint = null)
    {
        try
        {
            return GetTestRunnerWithoutExceptionHandling(featureHint);
        }
        catch (Exception ex)
        {
            _testTracer.TraceError(ex,TimeSpan.Zero);
            throw;
        }
    }

    private ITestRunner GetTestRunnerWithoutExceptionHandling(FeatureInfo featureHint)
    {
        var testRunner = GetOrCreateTestRunner(featureHint);

        if (IsMultiThreaded && Interlocked.CompareExchange(ref _wasSingletonInstanceDisabled, 1, 0) == 0)
        {
            FeatureContext.DisableSingletonInstance();
            ScenarioContext.DisableSingletonInstance();
            ScenarioStepContext.DisableSingletonInstance();
        }

        return testRunner;
    }

    private async Task<ExceptionDispatchInfo[]> FireRemainingAfterFeatureHooks()
    {
        var onFeatureEndErrors = new List<ExceptionDispatchInfo>();
        var testWorkerContainers = _availableTestWorkerContainers.Keys.Concat(_usedTestWorkerContainers.Keys).ToArray();
        foreach (var testWorkerContainer in testWorkerContainers)
        {
            var contextManager = testWorkerContainer.Resolve<IContextManager>();
            if (contextManager.FeatureContext != null)
            {
                var testRunner = testWorkerContainer.Resolve<ITestRunner>();
                try
                {
                    await testRunner.OnFeatureEndAsync();
                }
                catch (Exception ex)
                {
                    _testTracer.TraceWarning("[AfterFeature] error: " + ex);
                    onFeatureEndErrors.Add(ExceptionDispatchInfo.Capture(ex));
                }
            }
        }
        return onFeatureEndErrors.ToArray();
    }

    public virtual async Task DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _wasDisposed, 1, 0) == 0)
        {
            var onFeatureEndErrors = await FireRemainingAfterFeatureHooks();
            ExceptionDispatchInfo onTestRunEndError = null;

            try
            {
                await FireTestRunEndAsync();
            }
            catch (Exception ex)
            {
                onTestRunEndError = ExceptionDispatchInfo.Capture(ex);
            }

            if (_globalTestRunner != null)
            {
                ReleaseTestRunnerToPool(_globalTestRunner);
            }

            var testWorkerContainers = _availableTestWorkerContainers.ToArray();
            while (testWorkerContainers.Length > 0)
            {
                foreach (var item in testWorkerContainers)
                {
                    item.Key.Dispose();
                    _availableTestWorkerContainers.TryRemove(item.Key, out _);
                }
                testWorkerContainers = _availableTestWorkerContainers.ToArray();
            }

            var notReleasedTestWorkerContainers = _usedTestWorkerContainers.ToArray();
            if (notReleasedTestWorkerContainers.Length > 0)
            {
                var errorText = $"Found {notReleasedTestWorkerContainers.Length} not released TestRunners (ids: {string.Join(",", notReleasedTestWorkerContainers.Select(x => TestThreadContainerInfo.GetId(x.Key)))})";
                _globalContainer.Resolve<ITestTracer>().TraceWarning(errorText);
            }

            // this call dispose on this object, but _wasDisposed will avoid double execution
            _globalContainer.Dispose();

            OnTestRunnerManagerDisposed(this);

            // if we have errors in [AfterFeature] and [AfterTestRun] hooks, we throw them all
            if (onFeatureEndErrors.Any())
            {
                if (onTestRunEndError == null)
                    throw new AggregateException("Errors in [AfterFeature] hooks", onFeatureEndErrors.Select(x => x.SourceException));
                throw new AggregateException("Errors in [AfterFeature] and [AfterTestRun] hooks", onFeatureEndErrors.Select(x => x.SourceException).Concat([onTestRunEndError.SourceException]));
            }
            // if we have only error in [AfterTestRun] hooks, we throw it
            onTestRunEndError?.Throw();
        }
    }

    #region Static API

    private static readonly ConcurrentDictionary<Assembly, ITestRunnerManager> _testRunnerManagerRegistry = new();

    public static ITestRunnerManager GetTestRunnerManager(Assembly testAssembly = null, IContainerBuilder containerBuilder = null, bool createIfMissing = true)
    {
        testAssembly ??= GetCallingAssembly();

        if (!createIfMissing)
        {
            return _testRunnerManagerRegistry.TryGetValue(testAssembly, out var value) ? value : null;
        }

        var testRunnerManager = _testRunnerManagerRegistry.GetOrAdd(
            testAssembly,
            assembly => CreateTestRunnerManager(assembly, containerBuilder));
        return testRunnerManager;
    }

    /// <summary>
    /// This is a workaround method solving not correctly working Assembly.GetCallingAssembly() when called from async method (due to state machine).
    /// </summary>
    private static Assembly GetCallingAssembly([CallerMemberName] string callingMethodName = null)
    {
        var stackTrace = new StackTrace();

        var callingMethodIndex = -1;

        for (var i = 0; i < stackTrace.FrameCount; i++)
        {
            var frame = stackTrace.GetFrame(i);

            if (frame.GetMethod().Name == callingMethodName)
            {
                callingMethodIndex = i;
                break;
            }
        }

        Assembly result = null;

        if (callingMethodIndex >= 0 && callingMethodIndex + 1 < stackTrace.FrameCount)
        {
            result = stackTrace.GetFrame(callingMethodIndex + 1).GetMethod().DeclaringType?.Assembly;
        }

        return result ?? GetCallingAssembly();
    }
        
    private static ITestRunnerManager CreateTestRunnerManager(Assembly testAssembly, IContainerBuilder containerBuilder = null)
    {
        containerBuilder ??= new ContainerBuilder();

        var container = containerBuilder.CreateGlobalContainer(testAssembly);
        var testRunnerManager = container.Resolve<ITestRunnerManager>();
        testRunnerManager.Initialize(testAssembly);
        return testRunnerManager;
    }

    public static async Task OnTestRunEndAsync(Assembly testAssembly = null, IContainerBuilder containerBuilder = null)
    {
        testAssembly ??= GetCallingAssembly();
        var testRunnerManager = GetTestRunnerManager(testAssembly, createIfMissing: false, containerBuilder: containerBuilder);
        if (testRunnerManager != null)
        {
            // DisposeAsync invokes FireTestRunEndAsync
            await testRunnerManager.DisposeAsync();
        }
    }

    public static async Task OnTestRunStartAsync(Assembly testAssembly = null, IContainerBuilder containerBuilder = null)
    {
        testAssembly ??= GetCallingAssembly();
        var testRunnerManager = GetTestRunnerManager(testAssembly, createIfMissing: true, containerBuilder: containerBuilder);

        await testRunnerManager.FireTestRunStartAsync();
    }

    /// <summary>
    /// Provides a test runner for the specified or current assembly with optionally a custom container builder and a feature hint.
    /// When a feature hint is provided the test runner manager will attempt to return the same test runner that was used for that
    /// feature before. 
    /// </summary>
    /// <param name="testAssembly">The test assembly. If omitted or invoked with <c>null</c>, the calling assembly is used.</param>
    /// <param name="containerBuilder">The container builder to be used to set up Reqnroll dependencies. If omitted or invoked with <c>null</c>, the default container builder is used.</param>
    /// <param name="featureHint">If specified, it is used as a hint for the test runner manager to choose the test runner that has been used for the feature before, if possible.</param>
    /// <returns>A test runner that can be used to interact with Reqnroll.</returns>
    public static ITestRunner GetTestRunnerForAssembly(Assembly testAssembly = null, IContainerBuilder containerBuilder = null, FeatureInfo featureHint = null)
    {
        testAssembly ??= GetCallingAssembly();
        var testRunnerManager = GetTestRunnerManager(testAssembly, containerBuilder);
        return testRunnerManager.GetTestRunner(featureHint);
    }

    public static void ReleaseTestRunner(ITestRunner testRunner)
    {
        testRunner.TestThreadContext.TestThreadContainer.Resolve<ITestRunnerManager>().ReleaseTestRunnerToPool(testRunner);
    }

    public static async Task ReleaseFeatureAsync(FeatureInfo featureInfo, Assembly testAssembly = null, IContainerBuilder containerBuilder = null)
    {
        testAssembly ??= GetCallingAssembly();
        var testRunnerManager = GetTestRunnerManager(testAssembly, containerBuilder);
        await testRunnerManager.EndFeatureForAvailableTestRunnersAsync(featureInfo);
    }

    internal static async Task ResetAsync()
    {
        while (!_testRunnerManagerRegistry.IsEmpty)
        {
            foreach (var assembly in _testRunnerManagerRegistry.Keys.ToArray())
            {
                if (_testRunnerManagerRegistry.TryRemove(assembly, out var testRunnerManager))
                {
                    await testRunnerManager.DisposeAsync();
                }
            }
        }
    }

    private static void OnTestRunnerManagerDisposed(TestRunnerManager testRunnerManager)
    {
        _testRunnerManagerRegistry.TryRemove(testRunnerManager.TestAssembly, out _);
    }

    #endregion
}