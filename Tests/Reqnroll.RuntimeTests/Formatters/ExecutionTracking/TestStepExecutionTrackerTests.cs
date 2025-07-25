using FluentAssertions;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Linq;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class TestStepExecutionTrackerTests
{
    private readonly Mock<ICucumberMessageFactory> _messageFactoryMock;
    private readonly Mock<IPickleExecutionTracker> _pickleExecutionTrackerMock;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly TestCaseExecutionTracker _testCaseExecutionTrackerStub;
    private TestStepExecutionTracker _testStepExecutionTrackerSut;
    private readonly TestCaseTracker _testCaseTrackerStub;
        
    public TestStepExecutionTrackerTests()
    {
        _messageFactoryMock = new Mock<ICucumberMessageFactory>();
        _pickleExecutionTrackerMock = new Mock<IPickleExecutionTracker>();
        _testCaseTrackerStub = new TestCaseTracker("testCaseId", "testCasePickleId", _pickleExecutionTrackerMock.Object);
        _testCaseExecutionTrackerStub = CreateTestCaseExecutionRecord();

        _pickleExecutionTrackerMock.SetupGet(t => t.TestCaseTracker).Returns(_testCaseTrackerStub);

        // Setup mocked objects for tests
        _testStepExecutionTrackerSut = new TestStepExecutionTracker(
            _testCaseExecutionTrackerStub,
            _messageFactoryMock.Object
        );
    }

    private TestCaseExecutionTracker CreateTestCaseExecutionRecord(int attemptId = 0) =>
        new(_pickleExecutionTrackerMock.Object, _messageFactoryMock.Object, attemptId, "testCaseStartedId", "testCaseId", _testCaseTrackerStub);

    [Fact]
    public void TestStepTracker_GenerateFrom_StepStartedEvent_ReturnsOneEnvelope()
    {
        // Arrange
        var stepStartedEvent = new Mock<StepStartedEvent>(null!, null!, null!);
        var testStepStarted = new TestStepStarted("testCaseStartedId", "testStepId", new Timestamp(0, 1));
            
        _messageFactoryMock
            .Setup(f => f.ToTestStepStarted(_testStepExecutionTrackerSut))
            .Returns(testStepStarted);

        // Act
        var envelopes = ((IGenerateMessage)_testStepExecutionTrackerSut).GenerateFrom(stepStartedEvent.Object).ToList();

        // Assert
        envelopes.Should().HaveCount(1);
        envelopes[0].TestStepStarted.Should().Be(testStepStarted);
        _messageFactoryMock.Verify(f => f.ToTestStepStarted(_testStepExecutionTrackerSut), Times.Once);
    }

    [Fact]
    public void TestStepTracker_GenerateFrom_StepFinishedEvent_ReturnsOneEnvelope()
    {
        // Arrange
        var stepFinishedEvent = new Mock<StepFinishedEvent>(null!, null!, null!);
        var testStepFinished = new TestStepFinished("testCaseStartedId", "testStepId", new TestStepResult(new Duration(0,1), "", TestStepResultStatus.PASSED, null),new Timestamp(0, 1));
            
        _messageFactoryMock
            .Setup(f => f.ToTestStepFinished(_testStepExecutionTrackerSut))
            .Returns(testStepFinished);

        // Act
        var envelopes = ((IGenerateMessage)_testStepExecutionTrackerSut).GenerateFrom(stepFinishedEvent.Object).ToList();

        // Assert
        envelopes.Should().HaveCount(1);
        envelopes[0].TestStepFinished.Should().Be(testStepFinished);
        _messageFactoryMock.Verify(f => f.ToTestStepFinished(_testStepExecutionTrackerSut), Times.Once);
    }

    [Fact]
    public void TestStepTracker_GenerateFrom_OtherEvent_ReturnsEmptyEnumerable()
    {
        // Arrange
        var otherEvent = new Mock<ExecutionEvent>();

        // Act
        var envelopes = ((IGenerateMessage)_testStepExecutionTrackerSut).GenerateFrom(otherEvent.Object).ToList();

        // Assert
        envelopes.Should().BeEmpty();
    }

    [Fact]
    public void TestStepTracker_ProcessEvent_StepStartedEvent_FirstAttempt_CreatesDefinition()
    {
        // Arrange
        var stepContextMock = CreateStepContextWithPickleId("pickle123");

        var stepStartedEvent = new StepStartedEvent(null, null, stepContextMock.Object);
            
        var testCaseDefinitionStub = new TestCaseTracker("testCaseId", "pickleId", _pickleExecutionTrackerMock.Object);
        var idGenerator = new Mock<IIdGenerator>();
        idGenerator.Setup(g => g.GetNewId()).Returns("newStepId");

        _pickleExecutionTrackerMock.SetupGet(t => t.AttemptCount).Returns(0);
        _pickleExecutionTrackerMock.SetupGet(t => t.IdGenerator).Returns(idGenerator.Object);
        _pickleExecutionTrackerMock.SetupGet(t => t.TestCaseTracker).Returns(testCaseDefinitionStub);

        // Act
        _testStepExecutionTrackerSut.ProcessEvent(stepStartedEvent);

        // Assert
        _testStepExecutionTrackerSut.StepTracker.Should().NotBeNull();
        var stepDef = _testStepExecutionTrackerSut.StepTracker;
        testCaseDefinitionStub.Steps.Should().Contain(stepDef);
    }

    [Fact]
    public void TestStepTracker_ProcessEvent_StepStartedEvent_Retry_FindsExistingDefinition()
    {
        // Arrange
        var pickleStepId = "pickle123";
        var stepContextMock = CreateStepContextWithPickleId(pickleStepId);

        var stepStartedEvent = new StepStartedEvent(null, null, stepContextMock.Object);
            
        var testCaseDefinitionStub = new TestCaseTracker("testCaseId", "pickleId", _pickleExecutionTrackerMock.Object);
        var existingDefinition = new TestStepTracker("existingId", pickleStepId, testCaseDefinitionStub);

        testCaseDefinitionStub.Steps.Add(existingDefinition);
        _pickleExecutionTrackerMock.SetupGet(t => t.TestCaseTracker).Returns(testCaseDefinitionStub);

        // Create a hook tracker for a Retry
        _testStepExecutionTrackerSut = new TestStepExecutionTracker(
            CreateTestCaseExecutionRecord(1),
            _messageFactoryMock.Object
        );

        // Act
        _testStepExecutionTrackerSut.ProcessEvent(stepStartedEvent);

        // Assert
        _testStepExecutionTrackerSut.StepTracker.Should().Be(existingDefinition);
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

        var definitionStub = new TestStepTracker("stepId", "stepPickleId", null);
        _testStepExecutionTrackerSut.StepTracker = definitionStub;

        _pickleExecutionTrackerMock.SetupGet(t => t.AttemptCount).Returns(0); // First attempt

        // Act
        _testStepExecutionTrackerSut.ProcessEvent(stepFinishedEvent);

        // Assert
        _testStepExecutionTrackerSut.Exception.Should().Be(testError);
        _testStepExecutionTrackerSut.Status.Should().Be(status);
            
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