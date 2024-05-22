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
        var testRunner = _testRunnerManager.CreateTestRunner("0");

        testRunner.Should().NotBeNull();
        testRunner.Should().BeOfType<TestRunner>();
    }

    [Fact]
    public void GetTestRunner_should_be_able_to_create_a_TestRunner()
    {
        var testRunner = _testRunnerManager.GetTestRunner("0");

        testRunner.Should().NotBeNull();
        testRunner.Should().BeOfType<TestRunner>();
    }

    [Fact]
    public void GetTestRunner_should_cache_instance()
    {
        var testRunner1 = _testRunnerManager.GetTestRunner("0");
        var testRunner2 = _testRunnerManager.GetTestRunner("0");


        testRunner1.Should().Be(testRunner2);
    }

    [Fact]
    public void Should_return_different_instances_for_different_thread_ids()
    {
        var testRunner1 = _testRunnerManager.GetTestRunner("0");
        var testRunner2 = _testRunnerManager.GetTestRunner("1");

        testRunner1.Should().NotBe(testRunner2);
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
        var testRunner1 = _testRunnerManager.GetTestRunner("0");
        var testRunner2 = _testRunnerManager.GetTestRunner("1");

        var disposableClass1 = new DisposableClass();
        testRunner1.TestThreadContext.TestThreadContainer.RegisterInstanceAs(disposableClass1, dispose: true);

        var disposableClass2 = new DisposableClass();
        testRunner2.TestThreadContext.TestThreadContainer.RegisterInstanceAs(disposableClass2, dispose: true);

        await TestRunnerManager.OnTestRunEndAsync(_anAssembly);

        disposableClass1.IsDisposed.Should().BeTrue();
        disposableClass2.IsDisposed.Should().BeTrue();
    }
}