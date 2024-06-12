using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Reqnroll.RuntimeTests
{
    public class TestRunnerManagerStaticApiTest : IAsyncLifetime
    {
        private readonly Assembly _anAssembly = Assembly.GetExecutingAssembly();
        private readonly Assembly _anotherAssembly = typeof(TestRunnerManager).Assembly;

        public async Task InitializeAsync()
        {
            await TestRunnerManager.ResetAsync();
        }

        [Fact]
        public async Task GetTestRunner_without_arguments_should_return_TestRunner_instance()
        {
            var testRunner = TestRunnerManager.GetTestRunnerForAssembly(containerBuilder: new RuntimeTestsContainerBuilder());

            testRunner.Should().NotBeNull();
            testRunner.Should().BeOfType<TestRunner>();

            TestRunnerManager.ReleaseTestRunner(testRunner);
        }

        [Fact]
        public async Task GetTestRunner_should_return_different_instances_for_different_assemblies()
        {
            var testRunner1 = TestRunnerManager.GetTestRunnerForAssembly(_anAssembly, containerBuilder: new RuntimeTestsContainerBuilder());
            var testRunner2 = TestRunnerManager.GetTestRunnerForAssembly(_anotherAssembly, containerBuilder: new RuntimeTestsContainerBuilder());

            testRunner1.Should().NotBe(testRunner2);
            testRunner1.TestThreadContext.Should().NotBe(testRunner2.TestThreadContext);

            TestRunnerManager.ReleaseTestRunner(testRunner1);
            TestRunnerManager.ReleaseTestRunner(testRunner2);
        }

        [Fact]
        public async Task GetTestRunnerManager_without_arguments_should_return_an_instance_for_the_calling_assembly()
        {
            var testRunnerManager = TestRunnerManager.GetTestRunnerManager(containerBuilder: new RuntimeTestsContainerBuilder());

            testRunnerManager.Should().NotBeNull();
            testRunnerManager.TestAssembly.Should().BeSameAs(_anAssembly);
        }

        [Fact]
        public async Task GetTestRunnerManager_should_return_null_when_called_with_no_create_flag_and_there_was_no_instance_created_yet()
        {
            await TestRunnerManager.ResetAsync();

            var testRunnerManager = TestRunnerManager.GetTestRunnerManager(createIfMissing: false, containerBuilder: new RuntimeTestsContainerBuilder());

            testRunnerManager.Should().BeNull();
        }

        [Binding]
        public class AfterTestRunTestBinding
        {
            public static int AfterTestRunCallCount = 0;

            [AfterTestRun]
            public static void AfterTestRun()
            {
                AfterTestRunCallCount++;
            }
        }

        [Binding]
        public class BeforeTestRunTestBinding
        {
            public static int BeforeTestRunCallCount = 0;

            [BeforeTestRun]
            public static void BeforeTestRun()
            {
                BeforeTestRunCallCount++;
            }
        }

        [Fact]
        public async Task OnTestRunEnd_should_fire_AfterTestRun_events()
        {
            // make sure a test runner is initialized
            var testRunner = TestRunnerManager.GetTestRunnerForAssembly(_anAssembly, containerBuilder: new RuntimeTestsContainerBuilder());
            TestRunnerManager.ReleaseTestRunner(testRunner);

            AfterTestRunTestBinding.AfterTestRunCallCount = 0; //reset
            await TestRunnerManager.OnTestRunEndAsync(_anAssembly);

            AfterTestRunTestBinding.AfterTestRunCallCount.Should().Be(1);
        }

        [Fact]
        public async Task OnTestRunEnd_without_arguments_should_fire_AfterTestRun_events_for_calling_assembly()
        {
            // make sure a test runner is initialized
            var testRunner = TestRunnerManager.GetTestRunnerForAssembly(_anAssembly, containerBuilder: new RuntimeTestsContainerBuilder());
            TestRunnerManager.ReleaseTestRunner(testRunner);

            AfterTestRunTestBinding.AfterTestRunCallCount = 0; //reset
            await TestRunnerManager.OnTestRunEndAsync();

            AfterTestRunTestBinding.AfterTestRunCallCount.Should().Be(1);
        }

        [Fact]
        public async Task OnTestRunEnd_should_not_fire_AfterTestRun_events_multiple_times()
        {
            // make sure a test runner is initialized
            var testRunner = TestRunnerManager.GetTestRunnerForAssembly(_anAssembly, containerBuilder: new RuntimeTestsContainerBuilder());
            TestRunnerManager.ReleaseTestRunner(testRunner);

            AfterTestRunTestBinding.AfterTestRunCallCount = 0; //reset
            await TestRunnerManager.OnTestRunEndAsync(_anAssembly);
            await TestRunnerManager.OnTestRunEndAsync(_anAssembly);

            AfterTestRunTestBinding.AfterTestRunCallCount.Should().Be(1);
        }

        [Fact]
        public async Task OnTestRunStartAsync_should_initialize_binding_registry()
        {
            var testRunnerManager = (TestRunnerManager)TestRunnerManager.GetTestRunnerManager(_anAssembly, containerBuilder: new RuntimeTestsContainerBuilder());
            testRunnerManager.IsTestRunInitialized.Should().BeFalse("binding registry should not be initialized initially");

            await TestRunnerManager.OnTestRunStartAsync(_anAssembly);

            testRunnerManager.IsTestRunInitialized.Should().BeTrue("binding registry be initialized");
        }

        [Fact]
        public async Task OnTestRunStart_should_fire_BeforeTestRun_events()
        {
            BeforeTestRunTestBinding.BeforeTestRunCallCount = 0; //reset
            await TestRunnerManager.OnTestRunStartAsync(_anAssembly, containerBuilder: new RuntimeTestsContainerBuilder());

            BeforeTestRunTestBinding.BeforeTestRunCallCount.Should().Be(1);
        }

        [Fact]
        public async Task OnTestRunStart_without_arguments_should_fire_BeforeTestRun_events_for_calling_assembly()
        {
            BeforeTestRunTestBinding.BeforeTestRunCallCount = 0; //reset
            await TestRunnerManager.OnTestRunStartAsync(containerBuilder: new RuntimeTestsContainerBuilder());

            BeforeTestRunTestBinding.BeforeTestRunCallCount.Should().Be(1);
        }

        [Fact]
        public async Task OnTestRunStart_should_not_fire_BeforeTestRun_events_multiple_times()
        {
            BeforeTestRunTestBinding.BeforeTestRunCallCount = 0; //reset
            await TestRunnerManager.OnTestRunStartAsync(_anAssembly, containerBuilder: new RuntimeTestsContainerBuilder());
            await TestRunnerManager.OnTestRunStartAsync(_anAssembly);

            BeforeTestRunTestBinding.BeforeTestRunCallCount.Should().Be(1);
        }

        public async Task DisposeAsync()
        {
        }
    }
}
