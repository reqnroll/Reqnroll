using FluentAssertions;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking
{
    public class HookStepExecutionTrackerTests
    {
        private readonly Mock<ICucumberMessageFactory> _messageFactoryMock;
        private readonly Mock<IPickleExecutionTracker> _testCaseTrackerMock;
        private readonly ConcurrentDictionary<IBinding, string> _stepDefinitionsByBinding;
        private readonly TestCaseTracker _testCaseTracker;
        private readonly Mock<IIdGenerator> _idGeneratorMock;
        private HookStepExecutionTracker _hookStepExecutionTracker;
        private FeatureInfo _featureInfoStub;
        private FeatureContext _featureContextStub;
        private ScenarioInfo _scenarioInfoStub;
        private ScenarioContext _scenarioContextSub;
        private ScenarioStepContext _stepContext;
        private IObjectContainer _objectContainerStub;
        private Mock<IContextManager> _mockContextManager;

        private void SetupMockContexts()
        {
            var testObjResolverMock = new Mock<ITestObjectResolver>();
            
            _featureInfoStub = new FeatureInfo(CultureInfo.CurrentCulture, "", "Test Feature", "");
            _featureContextStub = new FeatureContext(_objectContainerStub, _featureInfoStub, ConfigurationLoader.GetDefault());

            _scenarioInfoStub = new ScenarioInfo("Test Scenario", "", [], null);
            _scenarioContextSub = new ScenarioContext(_objectContainerStub, _scenarioInfoStub, null, testObjResolverMock.Object);

            _stepContext = new ScenarioStepContext(new StepInfo(StepDefinitionType.Given, "a test step", null, null, "pickleStepId"));

            _mockContextManager = new Mock<IContextManager>();
            _mockContextManager.Setup(x => x.FeatureContext).Returns(_featureContextStub);
            _mockContextManager.Setup(x => x.ScenarioContext).Returns(_scenarioContextSub);
            _mockContextManager.Setup(x => x.StepContext).Returns(_stepContext);
        }



        public HookStepExecutionTrackerTests()
        {
            _messageFactoryMock = new Mock<ICucumberMessageFactory>();
            _testCaseTrackerMock = new Mock<IPickleExecutionTracker>();
            _testCaseTracker = new TestCaseTracker("testCaseId", "testCasePickleId", _testCaseTrackerMock.Object, _messageFactoryMock.Object);
            _idGeneratorMock = new Mock<IIdGenerator>();
            _stepDefinitionsByBinding = new ConcurrentDictionary<IBinding, string>();
            _objectContainerStub = new ObjectContainer();

            SetupMockContexts();

            // Setup mocks
            _testCaseTrackerMock.SetupGet(t => t.TestCaseTracker).Returns(_testCaseTracker);
            _testCaseTrackerMock.SetupGet(t => t.IdGenerator).Returns(_idGeneratorMock.Object);
            _testCaseTrackerMock.SetupGet(t => t.StepDefinitionsByBinding)
                .Returns(_stepDefinitionsByBinding);

            // Create the tracker to test
            _hookStepExecutionTracker = new HookStepExecutionTracker(
                CreateTestCaseExecutionRecord(),
                _messageFactoryMock.Object
            );
        }

        private TestCaseExecutionTracker CreateTestCaseExecutionRecord(int attemptId = 0) => 
            new(_testCaseTrackerMock.Object, _messageFactoryMock.Object, attemptId, "testCaseStartedId", "testCaseId", _testCaseTracker);

        [Fact]
        public void HookStepTracker_GenerateFrom_HookBindingStartedEvent_ReturnsOneEnvelope()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);
            var testStepStarted = new TestStepStarted("testCaseStartedId", "testsStepId", new Timestamp(0, 1));
            
            _messageFactoryMock
                .Setup(f => f.ToTestStepStarted(_hookStepExecutionTracker))
                .Returns(testStepStarted);

            // Act
            var envelopes = ((IGenerateMessage)_hookStepExecutionTracker).GenerateFrom(hookBindingStartedEvent).ToList();

            // Assert
            envelopes.Should().HaveCount(1);
            envelopes[0].TestStepStarted.Should().Be(testStepStarted);
            _messageFactoryMock.Verify(f => f.ToTestStepStarted(_hookStepExecutionTracker), Times.Once);
        }

        [Fact]
        public void HookStepTracker_GenerateFrom_HookBindingFinishedEvent_ReturnsOneEnvelope()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingFinishedEvent = new HookBindingFinishedEvent(hookBindingMock.Object, TimeSpan.Zero, _mockContextManager.Object);
            var testStepFinished = new TestStepFinished("testCaseStartedId", "testsStepId", new TestStepResult(new Duration(0, 1), "message", TestStepResultStatus.PASSED, null), new Timestamp(0, 1));
            
            _messageFactoryMock
                .Setup(f => f.ToTestStepFinished(_hookStepExecutionTracker))
                .Returns(testStepFinished);

            // Act
            var envelopes = ((IGenerateMessage)_hookStepExecutionTracker).GenerateFrom(hookBindingFinishedEvent).ToList();

            // Assert
            envelopes.Should().HaveCount(1);
            envelopes[0].TestStepFinished.Should().Be(testStepFinished);
            _messageFactoryMock.Verify(f => f.ToTestStepFinished(_hookStepExecutionTracker), Times.Once);
        }

        [Fact]
        public void HookStepTracker_GenerateFrom_OtherEvent_ReturnsEmptyEnumerable()
        {
            // Arrange
            var otherEvent = new Mock<ExecutionEvent>().Object;

            // Act
            var envelopes = ((IGenerateMessage)_hookStepExecutionTracker).GenerateFrom(otherEvent).ToList();

            // Assert
            envelopes.Should().BeEmpty();
        }

        [Fact]
        public void HookStepTracker_ProcessEvent_HookBindingStartedEvent_FirstAttempt_CreatesHookStepDefinition()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);

            var hookId = "hook123";
            var testStepId = "step456";

            
            _stepDefinitionsByBinding[hookBindingStartedEvent.HookBinding] = hookId;
            
            _idGeneratorMock.Setup(g => g.GetNewId()).Returns(testStepId);
            
            _testCaseTrackerMock.SetupGet(t => t.AttemptCount).Returns(0); // First attempt

            // Act
            _hookStepExecutionTracker.ProcessEvent(hookBindingStartedEvent);

            // Assert
            _hookStepExecutionTracker.StepTracker.Should().NotBeNull();
            _hookStepExecutionTracker.StepTracker.Should().BeOfType<HookStepTracker>();
            
            _testCaseTracker.Steps.Should().HaveCount(1);
            _testCaseTracker.Steps[0].Should().BeOfType<HookStepTracker>();
            (_testCaseTracker.Steps[0] as HookStepTracker).HookId.Should().Be(hookId);
        }

        [Fact]
        public void HookStepTracker_ProcessEvent_HookBindingStartedEvent_Retry_FindsExistingDefinition()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);

            var hookId = "hook123";
            
            _stepDefinitionsByBinding[hookBindingStartedEvent.HookBinding] = hookId;

            // Create a hook tracker for a Retry
            _hookStepExecutionTracker = new HookStepExecutionTracker(
                CreateTestCaseExecutionRecord(1),
                _messageFactoryMock.Object
            );
            
            // Pre-existing definition that should be found
            var existingHookStepDefinition = CreateDummyHookStepDefinition(hookId);

            _testCaseTracker.Steps.Add(existingHookStepDefinition);

            // Act
            _hookStepExecutionTracker.ProcessEvent(hookBindingStartedEvent);

            // Assert
            _hookStepExecutionTracker.StepTracker.Should().Be(existingHookStepDefinition);
            
            _testCaseTracker.Steps.Should().HaveCount(1);
        }

        [Fact]
        public void HookStepTracker_ProcessEvent_HookBindingFinishedEvent_WithNoException_SetsStatusToOK()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingFinishedEvent = new HookBindingFinishedEvent(
                hookBindingMock.Object, 
                TimeSpan.FromMilliseconds(100),
                _mockContextManager.Object
            );

            // Act
            _hookStepExecutionTracker.ProcessEvent(hookBindingFinishedEvent);

            // Assert
            _hookStepExecutionTracker.Exception.Should().BeNull();
            _hookStepExecutionTracker.Status.Should().Be(ScenarioExecutionStatus.OK); // Success status
        }

        [Fact]
        public void HookStepTracker_ProcessEvent_HookBindingFinishedEvent_WithException_SetsStatusToTestError()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var exception = new ApplicationException("Test hook exception");
            var hookBindingFinishedEvent = new HookBindingFinishedEvent(
                hookBindingMock.Object, 
                new TimeSpan(1),
                _mockContextManager.Object,
                exception
            );

            // Act
            _hookStepExecutionTracker.ProcessEvent(hookBindingFinishedEvent);

            // Assert
            _hookStepExecutionTracker.Exception.Should().Be(exception);
            _hookStepExecutionTracker.Status.Should().Be(ScenarioExecutionStatus.TestError); // Error status
        }

        // Helper method to create a dummy HookStepTracker without mocking
        private HookStepTracker CreateDummyHookStepDefinition(string hookId)
        {
            return new HookStepTracker(
                "dummyTestStepId", 
                hookId, 
                _testCaseTracker, 
                _messageFactoryMock.Object
            );
        }
    }

 
    //// Extend TestStepTracker to expose HookId in tests
    //public class HookStepTracker : TestStepTracker
    //{
    //    public HookStepTracker(string testStepDefinitionId, string hookId, TestCaseTracker parentTestCaseTracker, ICucumberMessageFactory messageFactory)
    //        : base(testStepDefinitionId, hookId, parentTestCaseTracker, messageFactory)
    //    {
    //        HookId = hookId;
    //    }

    //    public string HookId { get; }
    //}

    //// Base TestStepTracker class needed by HookStepTracker
    //public class TestStepTracker
    //{
    //    public TestStepTracker(string testStepDefinitionId, string pickleStepId, TestCaseTracker parentTestCaseTracker, ICucumberMessageFactory messageFactory)
    //    {
    //        TestStepId = testStepDefinitionId;
    //    }

    //    public string TestStepId { get; }
    //}
}
