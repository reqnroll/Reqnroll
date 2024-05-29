using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Reqnroll.RuntimeTests;

public class TestRunnerManagerTests
{
    private readonly Assembly _anAssembly = Assembly.GetExecutingAssembly();
    private readonly TestRunnerManager _testRunnerManager;

    public TestRunnerManagerTests()
    {
        _testRunnerManager = (TestRunnerManager)TestRunnerManager.GetTestRunnerManager(_anAssembly, new RuntimeTestsContainerBuilder());
    }

    [Fact]
    public void CreateTestRunner_should_be_able_to_create_a_TestRunner()
    {
        var testRunner = _testRunnerManager.CreateTestRunner();

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
}