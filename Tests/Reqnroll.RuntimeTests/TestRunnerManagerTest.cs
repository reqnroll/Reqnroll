using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Reqnroll.RuntimeTests
{
    /// <summary>
    /// Testing instance members of TestRunnerManager
    /// </summary>
    
    public class TestRunnerManagerTest
    {
        private readonly Assembly anAssembly = Assembly.GetExecutingAssembly();
        private TestRunnerManager testRunnerManager;

        public TestRunnerManagerTest()
        {
            testRunnerManager = (TestRunnerManager)TestRunnerManager.GetTestRunnerManager(anAssembly, new RuntimeTestsContainerBuilder());
        }

        [Fact]
        public void CreateTestRunner_should_be_able_to_create_a_testrunner()
        {
            var testRunner = testRunnerManager.CreateTestRunner("0");

            testRunner.Should().NotBeNull();
            testRunner.Should().BeOfType<TestRunner>();
        }

        [Fact]
        public void GetTestRunner_should_be_able_to_create_a_testrunner()
        {
            var testRunner = testRunnerManager.GetTestRunner("0");

            testRunner.Should().NotBeNull();
            testRunner.Should().BeOfType<TestRunner>();
        }

        [Fact]
        public void GetTestRunner_should_cache_instance()
        {
            var testRunner1 = testRunnerManager.GetTestRunner("0");
            var testRunner2 = testRunnerManager.GetTestRunner("0");


            testRunner1.Should().Be(testRunner2);
        }

        [Fact]
        public void Should_return_different_instances_for_different_thread_ids()
        {
            var testRunner1 = testRunnerManager.GetTestRunner("0");
            var testRunner2 = testRunnerManager.GetTestRunner("1");

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
            var testRunner1 = testRunnerManager.GetTestRunner("0");
            var testRunner2 = testRunnerManager.GetTestRunner("1");

            var disposableClass = new DisposableClass();
            testRunner1.TestThreadContext.TestThreadContainer.RegisterInstanceAs(disposableClass, dispose: true);

            await TestRunnerManager.OnTestRunEndAsync(anAssembly);

            disposableClass.IsDisposed.Should().BeTrue();
        }
    }
}