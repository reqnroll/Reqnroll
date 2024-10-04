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

namespace Reqnroll;

public class TestRunnerManager : ITestRunnerManager
{
    protected readonly IObjectContainer _globalContainer;
    protected readonly IContainerBuilder _containerBuilder;
    protected readonly ReqnrollConfiguration _reqnrollConfiguration;
    protected readonly IRuntimeBindingRegistryBuilder _bindingRegistryBuilder;
    protected readonly ITestTracer _testTracer;

    private readonly ConcurrentDictionary<IObjectContainer, object> _availableTestWorkerContainers = new();
    private readonly ConcurrentDictionary<IObjectContainer, object> _usedTestWorkerContainers = new();
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

    public virtual ITestRunner CreateTestRunner()
    {
        var testRunner = CreateTestRunnerInstance();

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

    public virtual void ReleaseTestThreadContext(ITestThreadContext testThreadContext)
    {
        var testThreadContainer = testThreadContext.TestThreadContainer;
        if (!_usedTestWorkerContainers.TryRemove(testThreadContainer, out _))
            throw new InvalidOperationException($"TestThreadContext with id {TestThreadContainerInfo.GetId(testThreadContainer)} was already released");
        if (!_availableTestWorkerContainers.TryAdd(testThreadContainer, null))
            throw new InvalidOperationException($"TestThreadContext with id {TestThreadContainerInfo.GetId(testThreadContainer)} was released twice");
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

    protected virtual ITestRunner CreateTestRunnerInstance()
    {
        IObjectContainer testThreadContainer = null;
        for (int i = 0; i < 5 && testThreadContainer == null; i++) // Try to get a available Container max 5 times
        {
            var items = _availableTestWorkerContainers.ToArray();
            if (items.Length == 0)
                break; // No Containers are available
            foreach (var item in items)
            {
                if (!_availableTestWorkerContainers.TryRemove(item.Key, out _))
                    continue; // Container was already taken by another thread
                testThreadContainer = item.Key;
                break;
            }
        }
        
        if (testThreadContainer == null)
        {
            testThreadContainer = _containerBuilder.CreateTestThreadContainer(_globalContainer);
            var id = Interlocked.Increment(ref _nextTestWorkerContainerId);
            var testThreadContainerInfo = new TestThreadContainerInfo(id.ToString(CultureInfo.InvariantCulture));
            testThreadContainer.RegisterInstanceAs(testThreadContainerInfo);
        }

        if (!_usedTestWorkerContainers.TryAdd(testThreadContainer, null))
            throw new InvalidOperationException($"TestThreadContext with id {TestThreadContainerInfo.GetId(testThreadContainer)} is already in usage");

        return testThreadContainer.Resolve<ITestRunner>();
    }

    public void Initialize(Assembly assignedTestAssembly)
    {
        TestAssembly = assignedTestAssembly;
    }

    public virtual ITestRunner GetTestRunner()
    {
        try
        {
            return GetTestRunnerWithoutExceptionHandling();
        }
        catch (Exception ex)
        {
            _testTracer.TraceError(ex,TimeSpan.Zero);
            throw;
        }
    }

    private ITestRunner GetTestRunnerWithoutExceptionHandling()
    {
        var testRunner = CreateTestRunner();

        if (IsMultiThreaded && Interlocked.CompareExchange(ref _wasSingletonInstanceDisabled, 1, 0) == 0)
        {
            FeatureContext.DisableSingletonInstance();
            ScenarioContext.DisableSingletonInstance();
            ScenarioStepContext.DisableSingletonInstance();
        }

        return testRunner;
    }

    private async Task FireRemainingAfterFeatureHooks()
    {
        var testWorkerContainers = _availableTestWorkerContainers.Concat(_usedTestWorkerContainers).ToArray();
        foreach (var testWorkerContainer in testWorkerContainers)
        {
            var contextManager = testWorkerContainer.Key.Resolve<IContextManager>();
            if (contextManager.FeatureContext != null)
            {
                var testRunner = testWorkerContainer.Key.Resolve<ITestRunner>();
                await testRunner.OnFeatureEndAsync();
            }
        }
    }

    public virtual async Task DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _wasDisposed, 1, 0) == 0)
        {
            await FireRemainingAfterFeatureHooks();

            await FireTestRunEndAsync();

            if (_globalTestRunner != null)
            {
                ReleaseTestThreadContext(_globalTestRunner.TestThreadContext);
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

    public static ITestRunner GetTestRunnerForAssembly(Assembly testAssembly = null, IContainerBuilder containerBuilder = null)
    {
        testAssembly ??= GetCallingAssembly();
        var testRunnerManager = GetTestRunnerManager(testAssembly, containerBuilder);
        return testRunnerManager.GetTestRunner();
    }

    public static void ReleaseTestRunner(ITestRunner testRunner)
    {
        testRunner.TestThreadContext.TestThreadContainer.Resolve<ITestRunnerManager>().ReleaseTestThreadContext(testRunner.TestThreadContext);
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