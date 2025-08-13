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
using System.Globalization;
using Xunit;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class HookStepExecutionTrackerTests
{
    private readonly Mock<ICucumberMessageFactory> _messageFactoryMock;
    private readonly Mock<IPickleExecutionTracker> _testCaseTrackerMock;
    private readonly Mock<IStepTrackerFactory> _stepTrackerFactoryMock;
    private readonly ConcurrentDictionary<IBinding, string> _stepDefinitionsByBinding;
    private readonly TestCaseTracker _testCaseTracker;
    private readonly Mock<IIdGenerator> _idGeneratorMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private HookStepExecutionTracker _hookStepExecutionTracker;
    private FeatureInfo _featureInfoStub;
    private FeatureContext _featureContextStub;
    private ScenarioInfo _scenarioInfoStub;
    private ScenarioContext _scenarioContextSub;
    private ScenarioStepContext _stepContext;
    private readonly IObjectContainer _objectContainerStub;
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
        _publisherMock = new Mock<IMessagePublisher>();
        _stepTrackerFactoryMock = new Mock<IStepTrackerFactory>();
        _testCaseTrackerMock = new Mock<IPickleExecutionTracker>();
        _testCaseTracker = new TestCaseTracker("testCaseId", "testCasePickleId", _testCaseTrackerMock.Object);
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
            _messageFactoryMock.Object,
            _publisherMock.Object
        );
    }

    private TestCaseExecutionTracker CreateTestCaseExecutionRecord(int attemptId = 0) =>
        new(_testCaseTrackerMock.Object, attemptId, "testCaseStartedId", "testCaseId", _testCaseTracker, _messageFactoryMock.Object, _publisherMock.Object, _stepTrackerFactoryMock.Object);

    [Fact]
    public async Task HookStepTracker_ProcessEvent_HookBindingStartedEvent_PublishesOneEnvelope()
    {
        // Arrange
        var hookBindingMock = new Mock<IHookBinding>();
        var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);
        var testStepStarted = new TestStepStarted("testCaseStartedId", "testsStepId", new Timestamp(0, 1));
        _stepDefinitionsByBinding[hookBindingMock.Object] = "hook123";

        _messageFactoryMock
            .Setup(f => f.ToTestStepStarted(_hookStepExecutionTracker))
            .Returns(testStepStarted);

        // Act
        await _hookStepExecutionTracker.ProcessEvent(hookBindingStartedEvent);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.Is<Envelope>(e => e.TestStepStarted == testStepStarted)), Times.Once);
    }

    [Fact]
    public async Task HookStepTracker_ProcessEvent_HookBindingFinishedEvent_ReturnsOneEnvelope()
    {
        // Arrange
        var hookBindingMock = new Mock<IHookBinding>();
        var hookBindingFinishedEvent = new HookBindingFinishedEvent(hookBindingMock.Object, TimeSpan.Zero, _mockContextManager.Object, ScenarioExecutionStatus.OK);
        var testStepFinished = new TestStepFinished("testCaseStartedId", "testsStepId", new TestStepResult(new Duration(0, 1), "message", TestStepResultStatus.PASSED, null), new Timestamp(0, 1));

        _messageFactoryMock
            .Setup(f => f.ToTestStepFinished(_hookStepExecutionTracker))
            .Returns(testStepFinished);

        // Act
        await _hookStepExecutionTracker.ProcessEvent(hookBindingFinishedEvent);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.Is<Envelope>(e => e.TestStepFinished == testStepFinished)), Times.Once);
    }

    [Fact]
    public async Task HookStepTracker_ProcessEvent_HookBindingStartedEvent_FirstAttempt_CreatesHookStepDefinition()
    {
        // Arrange
        var hookBindingMock = new Mock<IHookBinding>();
        var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);

        var hookId = "hook123";
        var testStepId = "step456";
        var testStepStarted = new TestStepStarted("testCaseStartedId", testStepId, new Timestamp(0, 1));
        _stepDefinitionsByBinding[hookBindingMock.Object] = "hook123";

        _messageFactoryMock
            .Setup(f => f.ToTestStepStarted(_hookStepExecutionTracker))
            .Returns(testStepStarted);


        _stepDefinitionsByBinding[hookBindingStartedEvent.HookBinding] = hookId;

        _idGeneratorMock.Setup(g => g.GetNewId()).Returns(testStepId);

        _testCaseTrackerMock.SetupGet(t => t.AttemptCount).Returns(0); // First attempt

        // Act
        await _hookStepExecutionTracker.ProcessEvent(hookBindingStartedEvent);

        // Assert
        _hookStepExecutionTracker.StepTracker.Should().NotBeNull();
        _hookStepExecutionTracker.StepTracker.Should().BeOfType<HookStepTracker>();

        _testCaseTracker.Steps.Should().HaveCount(1);
        _testCaseTracker.Steps[0].Should().BeOfType<HookStepTracker>();
        ((HookStepTracker)_testCaseTracker.Steps[0]).HookId.Should().Be(hookId);
    }

    [Fact]
    public async Task HookStepTracker_ProcessEvent_HookBindingStartedEvent_Retry_FindsExistingDefinition()
    {
        // Arrange
        var hookBindingMock = new Mock<IHookBinding>();
        var hookBindingStartedEvent = new HookBindingStartedEvent(hookBindingMock.Object, _mockContextManager.Object);

        var hookId = "hook123";
        var testStepId = "step456";
        var testStepStarted = new TestStepStarted("testCaseStartedId", testStepId, new Timestamp(0, 1));

        _messageFactoryMock
            .Setup(f => f.ToTestStepStarted(It.IsAny<HookStepExecutionTracker>()))
            .Returns(testStepStarted);

        _stepDefinitionsByBinding[hookBindingStartedEvent.HookBinding] = hookId;

        // Create a hook tracker for a Retry
        _hookStepExecutionTracker = new HookStepExecutionTracker(
            CreateTestCaseExecutionRecord(1),
            _messageFactoryMock.Object,
            _publisherMock.Object
        );

        // Pre-existing definition that should be found
        var existingHookStepDefinition = CreateDummyHookStepDefinition(hookId);

        _testCaseTracker.Steps.Add(existingHookStepDefinition);

        // Act
        await _hookStepExecutionTracker.ProcessEvent(hookBindingStartedEvent);

        // Assert
        _hookStepExecutionTracker.StepTracker.Should().Be(existingHookStepDefinition);

        _testCaseTracker.Steps.Should().HaveCount(1);
    }

    [Fact]
    public async Task HookStepTracker_ProcessEvent_HookBindingFinishedEvent_WithNoException_SetsStatusToOK()
    {
        // Arrange
        var hookBindingMock = new Mock<IHookBinding>();
        var hookBindingFinishedEvent = new HookBindingFinishedEvent(
            hookBindingMock.Object,
            TimeSpan.FromMilliseconds(100),
            _mockContextManager.Object,
            ScenarioExecutionStatus.OK
        );
        var hookId = "hook123";
        var testStepId = "step456";
        var testStepFinished = new TestStepFinished("testCaseStartedId", testStepId, new TestStepResult(new Duration(0, 0), "", TestStepResultStatus.PASSED, null), new Timestamp(0, 1));

        _messageFactoryMock
            .Setup(f => f.ToTestStepFinished(It.IsAny<HookStepExecutionTracker>()))
            .Returns(testStepFinished);

        _stepDefinitionsByBinding[hookBindingFinishedEvent.HookBinding] = hookId;


        // Act
        await _hookStepExecutionTracker.ProcessEvent(hookBindingFinishedEvent);

        // Assert
        _hookStepExecutionTracker.Exception.Should().BeNull();
        _hookStepExecutionTracker.Status.Should().Be(ScenarioExecutionStatus.OK); // Success status
    }


    // Helper method to create a dummy HookStepTracker without mocking
    private HookStepTracker CreateDummyHookStepDefinition(string hookId)
    {
        return new HookStepTracker(
            "dummyTestStepId",
            hookId
        );
    }
}
