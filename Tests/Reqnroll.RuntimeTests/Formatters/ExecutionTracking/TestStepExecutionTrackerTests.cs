using FluentAssertions;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using Xunit;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class TestStepExecutionTrackerTests
{
    private readonly Mock<ICucumberMessageFactory> _messageFactoryMock;
    private readonly Mock<IPickleExecutionTracker> _pickleExecutionTrackerMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly Mock<IStepTrackerFactory> _stepTrackerFactoryMock;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly TestCaseExecutionTracker _testCaseExecutionTrackerStub;
    private readonly TestStepExecutionTracker _testStepExecutionTrackerSut;
    private readonly TestCaseTracker _testCaseTrackerStub;
        
    public TestStepExecutionTrackerTests()
    {
        _messageFactoryMock = new Mock<ICucumberMessageFactory>();
        _pickleExecutionTrackerMock = new Mock<IPickleExecutionTracker>();
        _publisherMock = new Mock<IMessagePublisher>();
        _stepTrackerFactoryMock = new Mock<IStepTrackerFactory>();
        _testCaseTrackerStub = new TestCaseTracker("testCaseId", "testCasePickleId", _pickleExecutionTrackerMock.Object);
        _testCaseExecutionTrackerStub = CreateTestCaseExecutionTracker();

        _pickleExecutionTrackerMock.SetupGet(t => t.TestCaseTracker).Returns(_testCaseTrackerStub);

        // Setup mocked objects for tests
        _testStepExecutionTrackerSut = new TestStepExecutionTracker(
            _testCaseExecutionTrackerStub,
            _messageFactoryMock.Object,
            _publisherMock.Object
        );
    }

    private TestCaseExecutionTracker CreateTestCaseExecutionTracker(int attemptId = 0) =>
        new(_pickleExecutionTrackerMock.Object, attemptId, "testCaseStartedId", "testCaseId", _testCaseTrackerStub, _messageFactoryMock.Object, _publisherMock.Object, _stepTrackerFactoryMock.Object);

    [Fact]
    public async Task TestStepTracker_GenerateFrom_StepStartedEvent_PublishesOneEnvelope()
    {
        // Arrange
        var testCaseExecutionTracker = CreateTestCaseExecutionTracker(1);
        var testStepExecutionTrackerSut = new TestStepExecutionTracker(
            testCaseExecutionTracker,
            _messageFactoryMock.Object,
            _publisherMock.Object
        );
        var stepContextMock = CreateStepContextWithPickleId("pickle123");
        var testStepStarted = new TestStepStarted("testCaseStartedId", "testStepId", new Timestamp(0, 1));
        var stepStartedEvent = new StepStartedEvent(null, null, stepContextMock.Object);

        _messageFactoryMock
            .Setup(f => f.ToTestStepStarted(testStepExecutionTrackerSut))
            .Returns(testStepStarted);

        // Act
        await testStepExecutionTrackerSut.ProcessEvent(stepStartedEvent);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.Is<Envelope>(e => e.TestStepStarted == testStepStarted)), Times.Once);
    }

    [Fact]
    public async Task TestStepTracker_GenerateFrom_StepFinishedEvent_Publishes_OneEnvelope()
    {
        // Arrange
        var testCaseExecutionTracker = CreateTestCaseExecutionTracker(1);
        var testStepExecutionTrackerSut = new TestStepExecutionTracker(
            testCaseExecutionTracker,
            _messageFactoryMock.Object,
            _publisherMock.Object
        );
        var stepContextMock = CreateStepContextWithPickleId("pickle123");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.SetupGet(s => s.TestError).Returns((System.Exception)null);
        var stepFinishedEvent = new Mock<StepFinishedEvent>(null!, scenarioContextMock.Object, stepContextMock.Object);
        var testStepFinished = new TestStepFinished("testCaseStartedId", "testStepId", new TestStepResult(new Duration(0,1), "", TestStepResultStatus.PASSED, null),new Timestamp(0, 1));
            
        _messageFactoryMock
            .Setup(f => f.ToTestStepFinished(testStepExecutionTrackerSut))
            .Returns(testStepFinished);

        // Act
        await testStepExecutionTrackerSut.ProcessEvent(stepFinishedEvent.Object);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.Is<Envelope>(e => e.TestStepFinished == testStepFinished)), Times.Once);
    }

    [Fact]
    public async Task TestStepTracker_ProcessEvent_StepStartedEvent_FirstAttempt_CreatesDefinition()
    {
        // Arrange
        var stepContextMock = CreateStepContextWithPickleId("pickle123");
        var testStepStarted = new TestStepStarted("testCaseStartedId", "testStepId", new Timestamp(0, 1));

        _messageFactoryMock
            .Setup(f => f.ToTestStepStarted(_testStepExecutionTrackerSut))
            .Returns(testStepStarted);

        var stepStartedEvent = new StepStartedEvent(null, null, stepContextMock.Object);
            
        var idGenerator = new Mock<IIdGenerator>();
        idGenerator.Setup(g => g.GetNewId()).Returns("newStepId");

        _pickleExecutionTrackerMock.SetupGet(t => t.AttemptCount).Returns(0);
        _pickleExecutionTrackerMock.SetupGet(t => t.IdGenerator).Returns(idGenerator.Object);

        // Act
        await _testStepExecutionTrackerSut.ProcessEvent(stepStartedEvent);

        // Assert
        _testStepExecutionTrackerSut.StepTracker.Should().NotBeNull();
        var stepDef = _testStepExecutionTrackerSut.StepTracker;
        _testCaseTrackerStub.Steps.Should().Contain(stepDef);
    }

    [Fact]
    public async Task TestStepTracker_ProcessEvent_StepFinishedEvent_FirstAttempt_PopulatesDefinition()
    {
        // Arrange
        var testError = new ApplicationException("Test error");
        var status = ScenarioExecutionStatus.TestError;

        var stepContextMock = CreateStepContextWithPickleId("stepPickleId");
        stepContextMock.Setup(sc => sc.Status).Returns(status);
        var testStepFinished = new TestStepFinished("testCaseStartedId", "testStepId", new TestStepResult(new Duration(0, 0), "", TestStepResultStatus.FAILED, new Io.Cucumber.Messages.Types.Exception(testError.GetType().Name, "", null)), new Timestamp(0,0));

        _messageFactoryMock
            .Setup(f => f.ToTestStepFinished(_testStepExecutionTrackerSut))
            .Returns(testStepFinished);

        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.SetupGet(s => s.TestError).Returns(testError);

        var stepFinishedEvent = new StepFinishedEvent(null, scenarioContextMock.Object, stepContextMock.Object);

        var definitionStub = new TestStepTracker("stepId", "stepPickleId", null);
        _testStepExecutionTrackerSut.StepTracker = definitionStub;

        _pickleExecutionTrackerMock.SetupGet(t => t.AttemptCount).Returns(0); // First attempt

        // Act
        await _testStepExecutionTrackerSut.ProcessEvent(stepFinishedEvent);

        // Assert
        _testStepExecutionTrackerSut.Exception.Should().Be(testError);
        _testStepExecutionTrackerSut.Status.Should().Be(status);
            
    }

    private Mock<IScenarioStepContext> CreateStepContextWithPickleId(string pickleId)
    {
        var stepInfo = new StepInfo(StepDefinitionType.Given, "a pattern", null, null, pickleId);
        var stepContextMock = new Mock<IScenarioStepContext>();
        stepContextMock.SetupGet(s => s.StepInfo).Returns(stepInfo);
        return stepContextMock;
    }
}