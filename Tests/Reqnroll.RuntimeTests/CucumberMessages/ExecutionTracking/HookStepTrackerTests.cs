using FluentAssertions;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.CucumberMessages.ExecutionTracking;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Reqnroll.RuntimeTests.CucumberMessages.ExecutionTracking
{
    public class HookStepTrackerTests
    {
        private readonly Mock<ICucumberMessageFactory> _messageFactoryMock;
        private readonly Mock<ITestCaseTracker> _testCaseTrackerMock;
        private readonly ConcurrentDictionary<string, string> _stepDefinitionsByMethodSignature;
        private readonly TestCaseDefinition _testCaseDefinition;
        private readonly Mock<IIdGenerator> _idGeneratorMock;
        private readonly HookStepTracker _hookStepTracker;
        private FeatureInfo _featureInfoStub;
        private FeatureContext _featureContextStub;
        private ScenarioInfo _scenarioInfoStub;
        private ScenarioContext _scenarioContextSub;
        private ScenarioStepContext _stepContext;
        private IObjectContainer _objectContainerStub;
        private Mock<IContextManager> _mockContextManager;

        private void SetupMockContexts()
        {
            
            _featureInfoStub = new FeatureInfo(CultureInfo.CurrentCulture, "", "Test Feature", "");
            _featureContextStub = new FeatureContext(_objectContainerStub, _featureInfoStub, ConfigurationLoader.GetDefault());

            _scenarioInfoStub = new ScenarioInfo("Test Scenario", "", [], null);
            _scenarioContextSub = new ScenarioContext(_objectContainerStub, _scenarioInfoStub, null);

            _stepContext = new ScenarioStepContext(new StepInfo(StepDefinitionType.Given, "a test step", null, null, "pickleStepId"));

            _mockContextManager = new Mock<IContextManager>();
            _mockContextManager.Setup(x => x.FeatureContext).Returns(_featureContextStub);
            _mockContextManager.Setup(x => x.ScenarioContext).Returns(_scenarioContextSub);
            _mockContextManager.Setup(x => x.StepContext).Returns(_stepContext);
        }



        public HookStepTrackerTests()
        {
            _messageFactoryMock = new Mock<ICucumberMessageFactory>();
            _testCaseTrackerMock = new Mock<ITestCaseTracker>();
            _testCaseDefinition = new TestCaseDefinition("testCaseId", "testCasePickleId", _testCaseTrackerMock.Object);
            _idGeneratorMock = new Mock<IIdGenerator>();
            _stepDefinitionsByMethodSignature = new ConcurrentDictionary<string, string>();
            _objectContainerStub = new ObjectContainer();

            SetupMockContexts();

            // Setup mocks
            _testCaseTrackerMock.SetupGet(t => t.TestCaseDefinition).Returns(_testCaseDefinition);
            _testCaseTrackerMock.SetupGet(t => t.IDGenerator).Returns(_idGeneratorMock.Object);
            _testCaseTrackerMock.SetupGet(t => t.StepDefinitionsByMethodSignature)
                .Returns(_stepDefinitionsByMethodSignature);

            // Create a dummy execution record
            var dummyExecutionRecord = new TestCaseExecutionRecord(_messageFactoryMock.Object, 0, "testCaseStartedId", "testCaseId", _testCaseDefinition);

            // Create the tracker to test
            _hookStepTracker = new HookStepTracker(
                _testCaseTrackerMock.Object,
                dummyExecutionRecord,
                _messageFactoryMock.Object
            );
        }

        [Fact]
        public void HookStepTracker_GenerateFrom_HookBindingStartedEvent_ReturnsOneEnvelope()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);
            var testStepStarted = new TestStepStarted("testCaseStartedId", "testsStepId", new Timestamp(0, 1));
            
            _messageFactoryMock
                .Setup(f => f.ToTestStepStarted(_hookStepTracker))
                .Returns(testStepStarted);

            // Act
            var envelopes = _hookStepTracker.GenerateFrom(hookBindingStartedEvent).ToList();

            // Assert
            envelopes.Should().HaveCount(1);
            envelopes[0].TestStepStarted.Should().Be(testStepStarted);
            _messageFactoryMock.Verify(f => f.ToTestStepStarted(_hookStepTracker), Times.Once);
        }

        [Fact]
        public void HookStepTracker_GenerateFrom_HookBindingFinishedEvent_ReturnsOneEnvelope()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingFinishedEvent = new HookBindingFinishedEvent(hookBindingMock.Object, TimeSpan.Zero, _mockContextManager.Object);
            var testStepFinished = new TestStepFinished("testCaseStartedId", "testsStepId", new TestStepResult(new Duration(0, 1), "message", TestStepResultStatus.PASSED, null), new Timestamp(0, 1));
            
            _messageFactoryMock
                .Setup(f => f.ToTestStepFinished(_hookStepTracker))
                .Returns(testStepFinished);

            // Act
            var envelopes = _hookStepTracker.GenerateFrom(hookBindingFinishedEvent).ToList();

            // Assert
            envelopes.Should().HaveCount(1);
            envelopes[0].TestStepFinished.Should().Be(testStepFinished);
            _messageFactoryMock.Verify(f => f.ToTestStepFinished(_hookStepTracker), Times.Once);
        }

        [Fact]
        public void HookStepTracker_GenerateFrom_OtherEvent_ReturnsEmptyEnumerable()
        {
            // Arrange
            var otherEvent = new Mock<ExecutionEvent>().Object;

            // Act
            var envelopes = _hookStepTracker.GenerateFrom(otherEvent).ToList();

            // Assert
            envelopes.Should().BeEmpty();
        }

        [Fact]
        public void HookStepTracker_ProcessEvent_HookBindingStartedEvent_FirstAttempt_CreatesHookStepDefinition()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);

            var hookBindingSignature = "HookSignature";
            var hookId = "hook123";
            var testStepId = "step456";

            _messageFactoryMock
                .Setup(f => f.CanonicalizeHookBinding(hookBindingMock.Object))
                .Returns(hookBindingSignature);
            
            _stepDefinitionsByMethodSignature[hookBindingSignature] = hookId;
            
            _idGeneratorMock.Setup(g => g.GetNewId()).Returns(testStepId);
            
            _testCaseTrackerMock.SetupGet(t => t.AttemptCount).Returns(0); // First attempt

            // Act
            _hookStepTracker.ProcessEvent(hookBindingStartedEvent);

            // Assert
            _hookStepTracker.HookBindingSignature.Should().Be(hookBindingSignature);
            _hookStepTracker.Definition.Should().NotBeNull();
            _hookStepTracker.Definition.Should().BeOfType<HookStepDefinition>();
            
            _messageFactoryMock.Verify(f => f.CanonicalizeHookBinding(hookBindingMock.Object), Times.Once);
            _testCaseDefinition.StepDefinitions.Should().HaveCount(1);
            _testCaseDefinition.StepDefinitions[0].Should().BeOfType<HookStepDefinition>();
            (_testCaseDefinition.StepDefinitions[0] as HookStepDefinition).HookId.Should().Be(hookId);
        }

        [Fact]
        public void HookStepTracker_ProcessEvent_HookBindingStartedEvent_Retry_FindsExistingDefinition()
        {
            // Arrange
            var hookBindingMock = new Mock<IHookBinding>();
            var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);

            var hookBindingSignature = "HookSignature";
            var hookId = "hook123";
            
            _messageFactoryMock
                .Setup(f => f.CanonicalizeHookBinding(hookBindingMock.Object))
                .Returns(hookBindingSignature);
            
            _stepDefinitionsByMethodSignature[hookBindingSignature] = hookId;
            
            _testCaseTrackerMock.SetupGet(t => t.AttemptCount).Returns(1); // Retry
            
            // Pre-existing definition that should be found
            var existingHookStepDefinition = CreateDummyHookStepDefinition(hookId);

            _testCaseDefinition.StepDefinitions.Add(existingHookStepDefinition);

            // Act
            _hookStepTracker.ProcessEvent(hookBindingStartedEvent);

            // Assert
            _hookStepTracker.HookBindingSignature.Should().Be(hookBindingSignature);
            _hookStepTracker.Definition.Should().Be(existingHookStepDefinition);
            
            _messageFactoryMock.Verify(f => f.CanonicalizeHookBinding(hookBindingMock.Object), Times.Once);
            _testCaseDefinition.StepDefinitions.Should().HaveCount(1);
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
            _hookStepTracker.ProcessEvent(hookBindingFinishedEvent);

            // Assert
            _hookStepTracker.HookBindingFinishedEvent.Should().Be(hookBindingFinishedEvent);
            _hookStepTracker.Exception.Should().BeNull();
            _hookStepTracker.Status.Should().Be(ScenarioExecutionStatus.OK); // Success status
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
            _hookStepTracker.ProcessEvent(hookBindingFinishedEvent);

            // Assert
            _hookStepTracker.HookBindingFinishedEvent.Should().Be(hookBindingFinishedEvent);
            _hookStepTracker.Exception.Should().Be(exception);
            _hookStepTracker.Status.Should().Be(ScenarioExecutionStatus.TestError); // Error status
        }

        // Helper method to create a dummy HookStepDefinition without mocking
        private HookStepDefinition CreateDummyHookStepDefinition(string hookId)
        {
            return new HookStepDefinition(
                "dummyTestStepId", 
                hookId, 
                _testCaseDefinition, 
                _messageFactoryMock.Object
            );
        }
    }

 
    //// Extend TestStepDefinition to expose HookId in tests
    //public class HookStepDefinition : TestStepDefinition
    //{
    //    public HookStepDefinition(string testStepDefinitionId, string hookId, TestCaseDefinition parentTestCaseDefinition, ICucumberMessageFactory messageFactory)
    //        : base(testStepDefinitionId, hookId, parentTestCaseDefinition, messageFactory)
    //    {
    //        HookId = hookId;
    //    }

    //    public string HookId { get; }
    //}

    //// Base TestStepDefinition class needed by HookStepDefinition
    //public class TestStepDefinition
    //{
    //    public TestStepDefinition(string testStepDefinitionId, string pickleStepId, TestCaseDefinition parentTestCaseDefinition, ICucumberMessageFactory messageFactory)
    //    {
    //        TestStepId = testStepDefinitionId;
    //    }

    //    public string TestStepId { get; }
    //}
}
