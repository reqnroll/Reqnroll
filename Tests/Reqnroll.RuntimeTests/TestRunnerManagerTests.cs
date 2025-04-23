using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Reqnroll.Tracing;
using Xunit;
using static Reqnroll.RuntimeTests.TestRunnerManagerStaticApiTest;

namespace Reqnroll.RuntimeTests;

public class TestRunnerManagerTests : IAsyncLifetime
{
    private readonly Assembly _anAssembly = Assembly.GetExecutingAssembly();
    private TestRunnerManager _testRunnerManager;

    public async Task InitializeAsync()
    {
        await TestRunnerManager.ResetAsync();
        _testRunnerManager = (TestRunnerManager)TestRunnerManager.GetTestRunnerManager(_anAssembly, new RuntimeTestsContainerBuilder());
    }

    public async Task DisposeAsync()
    {
        //nop;
    }

    [Fact]
    public void CreateTestRunner_should_be_able_to_create_a_TestRunner()
    {
        var testRunner = _testRunnerManager.GetOrCreateTestRunner();

        testRunner.Should().NotBeNull();
        testRunner.Should().BeOfType<TestRunner>();

        TestRunnerManager.ReleaseTestRunner(testRunner);
    }

    [Fact]
    public void GetTestRunner_should_be_able_to_create_a_TestRunner()
    {
        var testRunner = _testRunnerManager.GetTestRunner();

        testRunner.Should().NotBeNull();
        testRunner.Should().BeOfType<TestRunner>();

        TestRunnerManager.ReleaseTestRunner(testRunner);
    }

    [Fact]
    public void Should_return_different_thread_ids_for_different_instances()
    {
        // Use an explicit new ITestRunnerManager to make sure that the Ids are created in a new way.
        var container = new RuntimeTestsContainerBuilder().CreateGlobalContainer(_anAssembly);
        var testRunnerManager = container.Resolve<ITestRunnerManager>();
        testRunnerManager.Initialize(_anAssembly);

        var testRunner1 = testRunnerManager.GetTestRunner();
        var testRunner2 = testRunnerManager.GetTestRunner();

        testRunner1.Should().NotBe(testRunner2);
        testRunner1.TestWorkerId.Should().Be("1");
        testRunner2.TestWorkerId.Should().Be("2");

        TestRunnerManager.ReleaseTestRunner(testRunner1);
        TestRunnerManager.ReleaseTestRunner(testRunner2);

        // TestRunner3 reused an existing TestThreadContainer, so the Id should be one of the previously created ones
        var testRunner3 = testRunnerManager.GetTestRunner();
        testRunner3.TestWorkerId.Should().Match(x => x == "1" || x == "2");

        TestRunnerManager.ReleaseTestRunner(testRunner3);
    }

    [Fact]
    public void Should_use_test_runner_that_was_active_last_time_on_the_same_thread_if_possible()
    {
        var ourFeatureInfo = new FeatureInfo(new CultureInfo("en-US"), null, "F1", null);
        var otherFeatureInfo = new FeatureInfo(new CultureInfo("en-US"), null, "F2", null);

        var otherRunners1 = Enumerable.Range(0, 5).Select(_ => TestRunnerManager.GetTestRunnerForAssembly(featureHint: otherFeatureInfo)).ToList();
        var otherRunners2 = Enumerable.Range(0, 5).Select(_ => TestRunnerManager.GetTestRunnerForAssembly(featureHint: otherFeatureInfo)).ToList();
        var runnerOnDifferentFeatureSameThread = TestRunnerManager.GetTestRunnerForAssembly(featureHint: otherFeatureInfo);
        var runnerOnSameFeatureDifferentThread = TestRunnerManager.GetTestRunnerForAssembly(featureHint: ourFeatureInfo);
        var ourRunner = TestRunnerManager.GetTestRunnerForAssembly(featureHint: ourFeatureInfo);

        void RunOnOtherThreadAndWait(Action action)
        {
            Task.Run(action).Wait(500);
        }

        // release otherRunners1 on a different thread
        RunOnOtherThreadAndWait(() => otherRunners1.ForEach(TestRunnerManager.ReleaseTestRunner));
        
        // release the selected runners
        TestRunnerManager.ReleaseTestRunner(ourRunner);
        TestRunnerManager.ReleaseTestRunner(runnerOnDifferentFeatureSameThread);
        RunOnOtherThreadAndWait(() => TestRunnerManager.ReleaseTestRunner(runnerOnSameFeatureDifferentThread));

        // release otherRunners2 on a different thread
        RunOnOtherThreadAndWait(() => otherRunners2.ForEach(TestRunnerManager.ReleaseTestRunner));

        // Priority:
        // 1. Same feature, same thread
        // 2. Same feature, different thread
        // 3. Other feature, same thread
        // 4. Other feature, other thread

        // Priority 1: from the same feature & same thread, we should get our test runner first
        var ourRunnerAgain = TestRunnerManager.GetTestRunnerForAssembly(featureHint: ourFeatureInfo);
        ourRunnerAgain.TestWorkerId.Should().Be(ourRunner.TestWorkerId);

        // Priority 2: from the same feature & different thread
        var runnerOnSameFeatureDifferentThreadAgain = TestRunnerManager.GetTestRunnerForAssembly(featureHint: ourFeatureInfo);
        runnerOnSameFeatureDifferentThreadAgain.TestWorkerId.Should().Be(runnerOnSameFeatureDifferentThread.TestWorkerId);

        // Priority 3: from the different feature & same thread
        var runnerOnDifferentFeatureSameThreadAgain = TestRunnerManager.GetTestRunnerForAssembly(featureHint: ourFeatureInfo);
        runnerOnDifferentFeatureSameThreadAgain.TestWorkerId.Should().Be(runnerOnDifferentFeatureSameThread.TestWorkerId);

        // Priority 4: if there is no from the same thread, just provide one of the others
        var otherRunnerWithoutHint = TestRunnerManager.GetTestRunnerForAssembly(featureHint: ourFeatureInfo);
        otherRunnerWithoutHint.TestWorkerId.Should().NotBe(ourRunner.TestWorkerId);

        TestRunnerManager.ReleaseTestRunner(ourRunnerAgain);
        TestRunnerManager.ReleaseTestRunner(runnerOnSameFeatureDifferentThreadAgain);
        TestRunnerManager.ReleaseTestRunner(runnerOnDifferentFeatureSameThreadAgain);
        TestRunnerManager.ReleaseTestRunner(otherRunnerWithoutHint);
    }

    [Fact]
    public async Task Should_fire_remaining_AfterFeature_hooks_of_test_threads()
    {
        _testRunnerManager.Initialize(_anAssembly);

        var testRunnerWithFeatureContext = _testRunnerManager.GetTestRunner();
        var testRunnerWithoutFeatureContext = _testRunnerManager.GetTestRunner();

        FeatureHookTracker.Reset();
        AfterTestRunTestBinding.Reset();
        await testRunnerWithFeatureContext.OnFeatureStartAsync(new FeatureInfo(new CultureInfo("en-US", false), string.Empty, "F", null));
        var disposableClass1 = new DisposableClass();
        testRunnerWithFeatureContext.FeatureContext.FeatureContainer.RegisterInstanceAs(disposableClass1, dispose: true);

        TestRunnerManager.ReleaseTestRunner(testRunnerWithFeatureContext);
        TestRunnerManager.ReleaseTestRunner(testRunnerWithoutFeatureContext);

        await TestRunnerManager.OnTestRunEndAsync(_anAssembly);
        FeatureHookTracker.AfterFeatureCalled.Should().BeTrue();
        disposableClass1.IsDisposed.Should().BeTrue();
        AfterTestRunTestBinding.AfterTestRunCallCount.Should().Be(1, "AfterTestRun should be called once");
    }

    [Fact]
    public async Task Should_fire_AfterTestRun_hook_even_if_remaining_AfterFeature_hooks_fail()
    {
        _testRunnerManager.Initialize(_anAssembly);

        var testRunnerWithFeatureContext = _testRunnerManager.GetTestRunner();
        var testRunnerWithoutFeatureContext = _testRunnerManager.GetTestRunner();

        FeatureHookTracker.Reset(shouldThrow: true);
        AfterTestRunTestBinding.Reset();
        await testRunnerWithFeatureContext.OnFeatureStartAsync(new FeatureInfo(new CultureInfo("en-US", false), string.Empty, "F", null));
        var disposableClass1 = new DisposableClass();
        testRunnerWithFeatureContext.FeatureContext.FeatureContainer.RegisterInstanceAs(disposableClass1, dispose: true);

        TestRunnerManager.ReleaseTestRunner(testRunnerWithFeatureContext);
        TestRunnerManager.ReleaseTestRunner(testRunnerWithoutFeatureContext);

        await FluentActions.Awaiting(() => TestRunnerManager.OnTestRunEndAsync(_anAssembly))
                     .Should()
                     .ThrowAsync<InvalidOperationException>();
        FeatureHookTracker.AfterFeatureCalled.Should().BeTrue();
        disposableClass1.IsDisposed.Should().BeTrue();
        AfterTestRunTestBinding.AfterTestRunCallCount.Should().Be(1, "AfterTestRun should be called once");
    }

    [Binding]
    static class FeatureHookTracker
    {
        private static readonly AsyncLocal<StrongBox<bool>> _afterFeatureCalled = new();
        private static readonly AsyncLocal<bool> _shouldThrow = new();

        public static void Reset(bool shouldThrow = false)
        {
            _afterFeatureCalled.Value = new StrongBox<bool>(false);
            _shouldThrow.Value = shouldThrow;
        }

        public static bool AfterFeatureCalled
        {
            get
            {
                if (_afterFeatureCalled.Value == null)
                    throw new InvalidOperationException($"Invoke {nameof(FeatureHookTracker)}.{nameof(Reset)} in the test arrange phase");
                return _afterFeatureCalled.Value.Value;
            }
        }

        [AfterFeature]
        public static void AfterFeature()
        {
            if (_afterFeatureCalled.Value != null)
                _afterFeatureCalled.Value.Value = true;
            if (_shouldThrow.Value)
                throw new InvalidOperationException("This is a test exception from AfterFeature");
        }
    }

    class DisposableClass : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Fact]
    public async Task Should_dispose_test_thread_container_at_after_test_run()
    {
        var testRunner1 = _testRunnerManager.GetTestRunner();
        var testRunner2 = _testRunnerManager.GetTestRunner();

        var disposableClass1 = new DisposableClass();
        testRunner1.TestThreadContext.TestThreadContainer.RegisterInstanceAs(disposableClass1, dispose: true);

        var disposableClass2 = new DisposableClass();
        testRunner2.TestThreadContext.TestThreadContainer.RegisterInstanceAs(disposableClass2, dispose: true);

        TestRunnerManager.ReleaseTestRunner(testRunner1);
        TestRunnerManager.ReleaseTestRunner(testRunner2);

        await TestRunnerManager.OnTestRunEndAsync(_anAssembly);

        disposableClass1.IsDisposed.Should().BeTrue();
        disposableClass2.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void First_call_to_CreateTestRunner_should_initialize_binding_registry()
    {
        _testRunnerManager.IsTestRunInitialized.Should().BeFalse("binding registry should not be initialized initially");

        _testRunnerManager.GetOrCreateTestRunner();

        _testRunnerManager.IsTestRunInitialized.Should().BeTrue("binding registry be initialized");
    }

    [Fact]
    public async Task Should_resolve_a_test_runner_specific_test_tracer()
    {
        var testRunner1 = TestRunnerManager.GetTestRunnerForAssembly(_anAssembly, new RuntimeTestsContainerBuilder());
        await testRunner1.OnFeatureStartAsync(new FeatureInfo(new CultureInfo("en-US", false), string.Empty, "sds", "sss"));
        testRunner1.OnScenarioInitialize(new ScenarioInfo("foo", "foo_desc", null, null));
        await testRunner1.OnScenarioStartAsync();
        var tracer1 = testRunner1.ScenarioContext.ScenarioContainer.Resolve<ITestTracer>();

        var testRunner2 = TestRunnerManager.GetTestRunnerForAssembly(_anAssembly, new RuntimeTestsContainerBuilder());
        await testRunner2.OnFeatureStartAsync(new FeatureInfo(new CultureInfo("en-US", false), string.Empty, "sds", "sss"));
        testRunner2.OnScenarioInitialize(new ScenarioInfo("foo", "foo_desc", null, null));
        await testRunner1.OnScenarioStartAsync();
        var tracer2 = testRunner2.ScenarioContext.ScenarioContainer.Resolve<ITestTracer>();

        tracer1.Should().NotBeSameAs(tracer2);

        TestRunnerManager.ReleaseTestRunner(testRunner1);
        TestRunnerManager.ReleaseTestRunner(testRunner2);
    }
}