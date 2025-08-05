using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Infrastructure;
using Reqnroll.TestFramework;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    /// <summary>
    /// Tests to reproduce issue #568: Assert.Ignore in BeforeFeature hook gives System.NullReferenceException
    /// https://github.com/reqnroll/Reqnroll/issues/568
    /// 
    /// This test demonstrates the scenario where:
    /// 1. A BeforeFeature hook calls Assert.Ignore() (or similar exception)
    /// 2. The test framework tries to tear down, but encounters a NullReferenceException
    /// 3. The fix should ensure proper cleanup even when before feature hooks fail
    /// </summary>
    public partial class TestExecutionEngineTests
    {

        [Fact]
        public async Task Should_handle_ignore_exception_in_before_feature_hook_without_null_reference()
        {
            // Arrange: Create a before feature hook that throws an IgnoreException (simulating Assert.Ignore)
            var testExecutionEngine = CreateTestExecutionEngine();
            var beforeFeatureHook = CreateHookMock(beforeFeatureEvents);
            
            // Setup the binding invoker to throw an IgnoreException when the hook is called
            methodBindingInvokerMock
                .Setup(bi => bi.InvokeBindingAsync(beforeFeatureHook.Object, contextManagerStub.Object, null, testTracerStub.Object, It.IsAny<DurationHolder>()))
                .Throws(new IgnoreException("for testing"));

            // Act & Assert: The hook should throw IgnoreException, but should not cause NullReferenceException later
            await FluentActions.Awaiting(() => testExecutionEngine.OnFeatureStartAsync(featureInfo))
                              .Should().ThrowAsync<IgnoreException>()
                              .WithMessage("for testing");

            // Verify that the BeforeFeatureHookError is properly set (part of the fix in PR #560)
            var featureContext = contextManagerStub.Object.FeatureContext;
            featureContext.BeforeFeatureHookFailed.Should().BeTrue();
            featureContext.BeforeFeatureHookError.Should().BeOfType<IgnoreException>();
            featureContext.BeforeFeatureHookError.Message.Should().Be("for testing");

            // The critical test: Verify that feature end can be called without causing NullReferenceException
            await FluentActions.Awaiting(() => testExecutionEngine.OnFeatureEndAsync())
                              .Should().NotThrowAsync<NullReferenceException>();

            // Verify cleanup was called properly
            contextManagerStub.Verify(cm => cm.CleanupFeatureContext(), Times.Once);
        }

        [Fact]
        public async Task Should_handle_pending_step_exception_in_before_feature_hook_without_null_reference()
        {
            // Arrange: This simulates the MSTest scenario mentioned in the issue comments
            var testExecutionEngine = CreateTestExecutionEngine();
            var beforeFeatureHook = CreateHookMock(beforeFeatureEvents);
            
            // Setup the binding invoker to throw a PendingStepException when the hook is called
            methodBindingInvokerMock
                .Setup(bi => bi.InvokeBindingAsync(beforeFeatureHook.Object, contextManagerStub.Object, null, testTracerStub.Object, It.IsAny<DurationHolder>()))
                .Throws(new PendingStepException("Step is pending"));

            // Act & Assert: The hook should throw PendingStepException, but should not cause NullReferenceException later
            await FluentActions.Awaiting(() => testExecutionEngine.OnFeatureStartAsync(featureInfo))
                              .Should().ThrowAsync<PendingStepException>()
                              .WithMessage("Step is pending");

            // Verify that the BeforeFeatureHookError is properly set
            var featureContext = contextManagerStub.Object.FeatureContext;
            featureContext.BeforeFeatureHookFailed.Should().BeTrue();
            featureContext.BeforeFeatureHookError.Should().BeOfType<PendingStepException>();

            // The critical test: Verify that feature end can be called without causing NullReferenceException
            await FluentActions.Awaiting(() => testExecutionEngine.OnFeatureEndAsync())
                              .Should().NotThrowAsync<NullReferenceException>();

            // Verify cleanup was called properly
            contextManagerStub.Verify(cm => cm.CleanupFeatureContext(), Times.Once);
        }

        [Fact]
        public async Task Should_skip_scenario_execution_when_before_feature_hook_failed()
        {
            // Arrange: Create a test execution engine
            var testExecutionEngine = CreateTestExecutionEngine();
            
            // Simulate a failed before feature hook by setting the error directly
            var featureContext = contextManagerStub.Object.FeatureContext;
            featureContext.BeforeFeatureHookError = new IgnoreException("Feature was ignored");

            // Set up scenario context to null (as it would be when feature hook fails and scenario never starts)
            contextManagerStub.Setup(cm => cm.ScenarioContext).Returns((ScenarioContext)null);

            // Act & Assert: Scenario end should handle the case where scenario was never really started
            // This tests the fix that checks for null ScenarioContext before processing
            await FluentActions.Awaiting(() => testExecutionEngine.OnScenarioEndAsync())
                              .Should().NotThrowAsync();

            // Verify that no attempt was made to clean up scenario context when it's null
            contextManagerStub.Verify(cm => cm.CleanupScenarioContext(), Times.Never);
        }

        [Fact]
        public async Task Should_properly_cleanup_feature_context_even_when_before_feature_hook_fails()
        {
            // Arrange: Simulate the exact scenario from the issue - BeforeFeature hook throws an exception
            var testExecutionEngine = CreateTestExecutionEngine();
            var beforeFeatureHook = CreateHookMock(beforeFeatureEvents);
            
            // Setup the binding invoker to throw an exception when the BeforeFeature hook is called
            var originalException = new IgnoreException("Simulated Assert.Ignore from BeforeFeature");
            methodBindingInvokerMock
                .Setup(bi => bi.InvokeBindingAsync(beforeFeatureHook.Object, contextManagerStub.Object, null, testTracerStub.Object, It.IsAny<DurationHolder>()))
                .Throws(originalException);

            // Act: Start the feature (this should fail) 
            await FluentActions.Awaiting(() => testExecutionEngine.OnFeatureStartAsync(featureInfo))
                              .Should().ThrowAsync<IgnoreException>();

            // The critical part: End the feature - this should not throw NullReferenceException
            // The original issue was that this call would fail with NullReferenceException in teardown
            await FluentActions.Awaiting(() => testExecutionEngine.OnFeatureEndAsync())
                              .Should().NotThrowAsync<NullReferenceException>();

            // Assert: Verify proper cleanup occurred
            contextManagerStub.Verify(cm => cm.CleanupFeatureContext(), Times.Once);
            
            // Verify the error was properly captured
            var featureContext = contextManagerStub.Object.FeatureContext;
            featureContext.BeforeFeatureHookFailed.Should().BeTrue();
            featureContext.BeforeFeatureHookError.Should().BeSameAs(originalException);
        }
    }

    /// <summary>
    /// Custom IgnoreException to simulate NUnit's Assert.Ignore behavior
    /// This represents the actual exception type that would be thrown by NUnit's Assert.Ignore()
    /// </summary>
    public class IgnoreException : Exception
    {
        public IgnoreException(string message) : base(message) { }
    }
}