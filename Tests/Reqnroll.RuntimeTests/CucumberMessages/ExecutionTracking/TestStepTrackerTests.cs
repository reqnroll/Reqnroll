using FluentAssertions;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Reqnroll.RuntimeTests.CucumberMessages.ExecutionTracking
{
    public class TestStepTrackerTests
    {
        private readonly Mock<ICucumberMessageFactory> _messageFactoryMock;
        private readonly Mock<ITestCaseTracker> _testCaseTrackerMock;
        private readonly TestCaseExecutionRecord _testCaseExecutionRecordStub;
        private readonly TestStepTracker _testStepTracker_sut;
        private readonly TestCaseDefinition _testCaseDefinitionStub;
        
        public TestStepTrackerTests()
        {
            _messageFactoryMock = new Mock<ICucumberMessageFactory>();
            _testCaseTrackerMock = new Mock<ITestCaseTracker>();
            _testCaseDefinitionStub = new TestCaseDefinition("testCaseId", "testCasePickleId", _testCaseTrackerMock.Object);
            _testCaseExecutionRecordStub = new TestCaseExecutionRecord(_messageFactoryMock.Object, 0, "testCaseStartedId", "testCaseId", _testCaseDefinitionStub);

            // Setup mocked objects for tests
            _testStepTracker_sut = new TestStepTracker(
                _testCaseTrackerMock.Object,
                _testCaseExecutionRecordStub,
                _messageFactoryMock.Object
            );
        }

        [Fact]
        public void TestStepTracker_GenerateFrom_StepStartedEvent_ReturnsOneEnvelope()
        {
            // Arrange
            var stepStartedEvent = new Mock<StepStartedEvent>(null, null, null);
            var testStepStarted = new TestStepStarted("testCaseStartedId", "testStepId", new Timestamp(0, 1));
            
            _messageFactoryMock
                .Setup(f => f.ToTestStepStarted(_testStepTracker_sut))
                .Returns(testStepStarted);

            // Act
            var envelopes = _testStepTracker_sut.GenerateFrom(stepStartedEvent.Object).ToList();

            // Assert
            envelopes.Should().HaveCount(1);
            envelopes[0].TestStepStarted.Should().Be(testStepStarted);
            _messageFactoryMock.Verify(f => f.ToTestStepStarted(_testStepTracker_sut), Times.Once);
        }

        [Fact]
        public void TestStepTracker_GenerateFrom_StepFinishedEvent_ReturnsOneEnvelope()
        {
            // Arrange
            var stepFinishedEvent = new Mock<StepFinishedEvent>(null, null, null);
            var testStepFinished = new TestStepFinished("testCaseStartedId", "testStepId", new TestStepResult(new Duration(0,1), "", TestStepResultStatus.PASSED, null),new Timestamp(0, 1));
            
            _messageFactoryMock
                .Setup(f => f.ToTestStepFinished(_testStepTracker_sut))
                .Returns(testStepFinished);

            // Act
            var envelopes = _testStepTracker_sut.GenerateFrom(stepFinishedEvent.Object).ToList();

            // Assert
            envelopes.Should().HaveCount(1);
            envelopes[0].TestStepFinished.Should().Be(testStepFinished);
            _messageFactoryMock.Verify(f => f.ToTestStepFinished(_testStepTracker_sut), Times.Once);
        }

        [Fact]
        public void TestStepTracker_GenerateFrom_OtherEvent_ReturnsEmptyEnumerable()
        {
            // Arrange
            var otherEvent = new Mock<ExecutionEvent>();

            // Act
            var envelopes = _testStepTracker_sut.GenerateFrom(otherEvent.Object).ToList();

            // Assert
            envelopes.Should().BeEmpty();
        }

        [Fact]
        public void TestStepTracker_ProcessEvent_StepStartedEvent_FirstAttempt_CreatesDefinition()
        {
            // Arrange
            var stepContextMock = CreateStepContextWithPickleId("pickle123");

            var stepStartedEvent = new StepStartedEvent(null, null, stepContextMock.Object);
            
            var testCaseDefinitionStub = new TestCaseDefinition("testCaseId", "pickleId", _testCaseTrackerMock.Object);
            var idGenerator = new Mock<IIdGenerator>();
            idGenerator.Setup(g => g.GetNewId()).Returns("newStepId");

            _testCaseTrackerMock.SetupGet(t => t.AttemptCount).Returns(0);
            _testCaseTrackerMock.SetupGet(t => t.IDGenerator).Returns(idGenerator.Object);
            _testCaseTrackerMock.SetupGet(t => t.TestCaseDefinition).Returns(testCaseDefinitionStub);

            // Act
            _testStepTracker_sut.ProcessEvent(stepStartedEvent);

            // Assert
            _testStepTracker_sut.Definition.Should().NotBeNull();
            var stepDef = _testStepTracker_sut.Definition;
            testCaseDefinitionStub.StepDefinitions.Should().Contain(stepDef);
        }

        [Fact]
        public void TestStepTracker_ProcessEvent_StepStartedEvent_Retry_FindsExistingDefinition()
        {
            // Arrange
            var pickleStepId = "pickle123";
            var stepContextMock = CreateStepContextWithPickleId(pickleStepId);

            var stepStartedEvent = new StepStartedEvent(null, null, stepContextMock.Object);
            
            var testCaseDefinitionStub = new TestCaseDefinition("testCaseId", "pickleId", _testCaseTrackerMock.Object);
            var existingDefinition = new TestStepDefinition("existingId", pickleStepId, testCaseDefinitionStub, _messageFactoryMock.Object);

            testCaseDefinitionStub.StepDefinitions.Add(existingDefinition);

            _testCaseTrackerMock.SetupGet(t => t.AttemptCount).Returns(1); // Retry
            _testCaseTrackerMock.SetupGet(t => t.TestCaseDefinition).Returns(testCaseDefinitionStub);

            // Act
            _testStepTracker_sut.ProcessEvent(stepStartedEvent);

            // Assert
            _testStepTracker_sut.Definition.Should().Be(existingDefinition);
        }

        [Fact]
        public void TestStepTracker_ProcessEvent_StepFinishedEvent_FirstAttempt_PopulatesDefinition()
        {
            // Arrange
            var testError = new ApplicationException("Test error");
            var status = ScenarioExecutionStatus.TestError;

            var stepContextMock = CreateStepContextWithPickleId("stepPickleId");
            stepContextMock.Setup(sc => sc.Status).Returns(status);

            var scenarioContextMock = new Mock<IScenarioContext>();
            scenarioContextMock.SetupGet(s => s.TestError).Returns(testError);

            var stepFinishedEvent = new StepFinishedEvent(null, scenarioContextMock.Object, stepContextMock.Object);

            var definitionStub = new TestStepDefinition("stepId", "stepPickleId", null, _messageFactoryMock.Object);
            _testStepTracker_sut.Definition = definitionStub;

            _testCaseTrackerMock.SetupGet(t => t.AttemptCount).Returns(0); // First attempt

            // Act
            _testStepTracker_sut.ProcessEvent(stepFinishedEvent);

            // Assert
            _testStepTracker_sut.Exception.Should().Be(testError);
            _testStepTracker_sut.Status.Should().Be(status);
            
            //definitionStub.Verify(d => d.PopulateStepDefinitionFromExecutionResult(stepFinishedEvent.Object), Times.Once);
        }

        private Mock<IScenarioStepContext> CreateStepContextWithPickleId(string pickleId)
        {
            var stepInfo = new StepInfo(StepDefinitionType.Given, "a pattern", null, null, pickleId);
            var stepContextMock = new Mock<IScenarioStepContext>();
            stepContextMock.SetupGet(s => s.StepInfo).Returns(stepInfo);
            return stepContextMock;
        }
    }
}
