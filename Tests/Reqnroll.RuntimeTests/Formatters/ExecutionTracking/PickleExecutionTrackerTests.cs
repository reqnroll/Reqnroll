using Cucumber.Messages;
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
using HookType = Reqnroll.Bindings.HookType;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class PickleExecutionTrackerTests
{
    private readonly Mock<IIdGenerator> _mockIdGenerator;
    private readonly Mock<ICucumberMessageFactory> _mockMessageFactory;
    private readonly Mock<IMessagePublisher> _mockPublisher;
    private readonly ITestCaseExecutionTrackerFactory _stubTestCaseExecutionTrackerFactory;
    private FeatureInfo _featureInfoStub;
    private FeatureContext _featureContextStub;
    private ScenarioInfo _scenarioInfoStub;
    private ScenarioContext _scenarioContextSub;
    private Mock<IScenarioStepContext> _mockStepContext;
    private Mock<IContextManager> _mockContextManager;
    private readonly Mock<IHookBinding> _mockHookBinding;
    private readonly ConcurrentDictionary<IBinding, string> _stepDefinitionsByMethodSignature;
    private readonly Timestamp _testTime = new(0, 1);
    private readonly ObjectContainer _objectContainerStub = new();

    public PickleExecutionTrackerTests()
    {
        _mockIdGenerator = new Mock<IIdGenerator>();
        _mockIdGenerator.Setup(x => x.GetNewId()).Returns("test-id");
        _mockPublisher = new Mock<IMessagePublisher>();
        _mockMessageFactory = new Mock<ICucumberMessageFactory>();

        IStepTrackerFactory stepTrackerFactory = new StepTrackerFactory(_mockMessageFactory.Object, _mockPublisher.Object, _mockIdGenerator.Object);
        _stubTestCaseExecutionTrackerFactory = new TestCaseExecutionTrackerFactory(_mockIdGenerator.Object, _mockMessageFactory.Object, _mockPublisher.Object, stepTrackerFactory);

        SetupMockContexts();

        _mockHookBinding = new Mock<IHookBinding>();

        _stepDefinitionsByMethodSignature = new ConcurrentDictionary<IBinding, string>();

        // Setup message factory mocks to return test envelopes
        _mockMessageFactory
            .Setup(m => m.ToTestCase(It.IsAny<TestCaseTracker>()))
            .Returns(new TestCase("test-id", "test-pickle-id", new List<TestStep>(), "test-run-started-id"));

        _mockMessageFactory
            .Setup(m => m.ToTestCaseStarted(It.IsAny<TestCaseExecutionTracker>(), It.IsAny<string>()))
            .Returns(new TestCaseStarted(0, "test-case-started-id", "test-id", "", _testTime));

        _mockMessageFactory
            .Setup(m => m.ToTestCaseFinished(It.IsAny<TestCaseExecutionTracker>(), It.IsAny<bool>()))
            .Returns((TestCaseExecutionTracker _, bool willBeRetried) => new TestCaseFinished("test-case-started-id", _testTime, willBeRetried));

        _mockMessageFactory
            .Setup(m => m.ToTestStepStarted(It.IsAny<TestStepExecutionTracker>()))
            .Returns(new TestStepStarted("testCaseStartedId", "test-step-Id", new Timestamp(0, 1)));

        _mockMessageFactory
            .Setup(m => m.ToTestStepFinished(It.IsAny<TestStepExecutionTracker>()))
            .Returns(new TestStepFinished("testCaseStartedId", "test-step-Id", new TestStepResult(new Duration(0, 1), "result-message", TestStepResultStatus.PASSED, null), new Timestamp(0, 1)));

        _mockMessageFactory
            .Setup(m => m.ToTestStepStarted(It.IsAny<HookStepExecutionTracker>()))
            .Returns(new TestStepStarted("testCaseStartedId", "hook-Id", new Timestamp(0, 1)));

        _mockMessageFactory
            .Setup(m => m.ToTestStepFinished(It.IsAny<HookStepExecutionTracker>()))
            .Returns(new TestStepFinished("testCaseStartedId", "hook-Id", new TestStepResult(new Duration(0, 1), "result-message", TestStepResultStatus.PASSED, null), new Timestamp(0, 1)));

        _mockMessageFactory
            .Setup(m => m.ToAttachment(It.IsAny<AttachmentTracker>()))
            .Returns(new Attachment("attachmentbody", AttachmentContentEncoding.BASE64, "filename", "mediatype", new Source("uri", "data", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN), "test-case-started-id", "test-step-Id", "url", "test-run-started-id", "test-run-hook-started-id", new Timestamp(0, 1)));

        _mockMessageFactory
            .Setup(m => m.ToAttachment(It.IsAny<OutputMessageTracker>()))
            .Returns(new Attachment("attachmentbody", AttachmentContentEncoding.BASE64, "filename", "mediatype", new Source("uri", "data", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN), "test-case-started-id", "test-step-Id", "url", "test-run-started-id", "test-run-hook-started-id", new Timestamp(0, 1)));

        _stepDefinitionsByMethodSignature.TryAdd(_mockHookBinding.Object, "hook-id");
    }

    private void SetupMockContexts()
    {
        var testOjbResolverMock = new Mock<ITestObjectResolver>();

        _featureInfoStub = new FeatureInfo(CultureInfo.CurrentCulture, "", "Test Feature", "");
        _featureContextStub = new FeatureContext(_objectContainerStub, _featureInfoStub, ConfigurationLoader.GetDefault());

        _scenarioInfoStub = new ScenarioInfo("Test Scenario", "", [], null);
        _scenarioContextSub = new ScenarioContext(_objectContainerStub, _scenarioInfoStub, null, testOjbResolverMock.Object);

        _mockStepContext = new Mock<IScenarioStepContext>();
        _mockStepContext.Setup(x => x.StepInfo).Returns(new StepInfo(StepDefinitionType.Given, "a test step", null, null, "pickleStepId"));

        _mockContextManager = new Mock<IContextManager>();
        _mockContextManager.Setup(x => x.FeatureContext).Returns(_featureContextStub);
        _mockContextManager.Setup(x => x.ScenarioContext).Returns(_scenarioContextSub);
        //_mockContextManager.Setup(x => x.StepContext).Returns(_mockStepContext.Object);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var tracker = CreatePickleExecTracker();

        // Assert
        tracker.PickleId.Should().Be("test-pickle-id");
        tracker.TestRunStartedId.Should().Be("test-run-id");
        tracker.FeatureName.Should().Be("Test Feature");
        tracker.Enabled.Should().BeTrue();
        tracker.TestCaseStartedTimeStamp.Should().Be(Converters.ToDateTime(_testTime));
        tracker.AttemptCount.Should().Be(-1);
        tracker.Finished.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessEvent_ScenarioStartedEvent_IncrementsAttemptCountAndCreatesTestCaseDefinition()
    {
        // Arrange
        var tracker = CreatePickleExecTracker();
        var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);

        // Act
        await tracker.ProcessEvent(scenarioStartedEvent);

        // Assert
        tracker.AttemptCount.Should().Be(0);
        tracker.TestCaseId.Should().Be("test-id");
        tracker.TestCaseTracker.Should().NotBeNull();
        tracker.Finished.Should().BeFalse();
        _scenarioInfoStub.PickleId.Should().Be("test-pickle-id");
    }

    [Fact]
    public async Task ProcessEvent_ScenarioFinishedEvent_SetsFinishedFlag()
    {
        // Arrange
        var tracker = CreatePickleExecTracker();
        var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
        var scenarioFinishedEvent = new ScenarioFinishedEvent(_featureContextStub, _scenarioContextSub);

        await tracker.ProcessEvent(scenarioStartedEvent);

        // Act
        await tracker.ProcessEvent(scenarioFinishedEvent);

        // Assert
        tracker.Finished.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessEvent_RetryScenario_CreatesMultipleExecutionRecords()
    {
        // Arrange
        var tracker = CreatePickleExecTracker();

        var publishedEnvelopes = new List<Envelope>();
        _mockPublisher
            .Setup(p => p.PublishAsync(It.IsAny<Envelope>()))
            .Callback<Envelope>(env => publishedEnvelopes.Add(env))
            .Returns(Task.CompletedTask);

        // First execution
        var scenarioStartedEvent1 = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
        var scenarioFinishedEvent1 = new ScenarioFinishedEvent(_featureContextStub, _scenarioContextSub);
        // Create a StepStartedEvent with a valid step context
        // Create a StepFinishedEvent with ScenarioExecutionStatus set to Error
        var errorStepContext = new Mock<IScenarioStepContext>();
        errorStepContext.Setup(x => x.StepInfo).Returns(new StepInfo(StepDefinitionType.Given, "a test step", null, null, "pickleStepId"));
        errorStepContext.SetupProperty(x => x.Status, ScenarioExecutionStatus.TestError);
        var errorStepStartedEvent = new StepStartedEvent(_featureContextStub, _scenarioContextSub, errorStepContext.Object);
        var errorStepFinishedEvent = new StepFinishedEvent(_featureContextStub, _scenarioContextSub, errorStepContext.Object);
        errorStepContext.Object.Status = ScenarioExecutionStatus.TestError;

        // Second execution (retry)
        var scenarioStartedEvent2 = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
        var scenarioFinishedEvent2 = new ScenarioFinishedEvent(_featureContextStub, _scenarioContextSub);

        // Act

        await tracker.ProcessEvent(scenarioStartedEvent1);
        await tracker.ProcessEvent(errorStepStartedEvent);
        await tracker.ProcessEvent(errorStepFinishedEvent);
        _scenarioContextSub.ScenarioExecutionStatus = ScenarioExecutionStatus.TestError; // Simulate an error
        await tracker.ProcessEvent(scenarioFinishedEvent1);
        await tracker.ProcessEvent(scenarioStartedEvent2);
        await tracker.ProcessEvent(scenarioFinishedEvent2);
        await tracker.FinalizeTracking();

        // Assert
        tracker.AttemptCount.Should().Be(1);
        tracker.Finished.Should().BeTrue();

        publishedEnvelopes.Should().NotBeEmpty();

        // Verify that we have at least one message with willBeRetried=true
        var envelopesWithTestCaseFinished = publishedEnvelopes
            .Where(e => e.Content() is TestCaseFinished)
            .ToList();

        envelopesWithTestCaseFinished.Should().HaveCountGreaterThan(1);

        // The first TestCaseFinished should have willBeRetried=true
        var firstTestCaseFinished = envelopesWithTestCaseFinished.First().Content() as TestCaseFinished;
        firstTestCaseFinished?.WillBeRetried.Should().BeTrue();

        _scenarioContextSub.ScenarioExecutionStatus = ScenarioExecutionStatus.OK; // Reset for next tests
    }

    [Fact]
    public async Task ProcessEvent_StepEvents_AddsStepTrackerToExecutionRecord()
    {
        // Arrange
        var tracker = CreatePickleExecTracker();
        var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
        var stepStartedEvent = new StepStartedEvent(_featureContextStub, _scenarioContextSub, _mockStepContext.Object);
        var stepFinishedEvent = new StepFinishedEvent(_featureContextStub, _scenarioContextSub, _mockStepContext.Object);

        // Initial setup
        await tracker.ProcessEvent(scenarioStartedEvent);

        // Act
        await tracker.ProcessEvent(stepStartedEvent);
        await tracker.ProcessEvent(stepFinishedEvent);

        // Assert - check the internal _currentTestCaseExecutionTracker
        tracker.CurrentTestCaseExecutionTracker.Should().NotBeNull();
        tracker.ExecutionHistory.Should().NotBeEmpty();

        // Assert there is exactly one execution record
        tracker.ExecutionHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProcessEvent_HookEvents_AddsHookTrackerToExecutionRecord()
    {
        // Arrange
        var tracker = CreatePickleExecTracker();
        var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);

        _mockHookBinding.Setup(h => h.HookType).Returns(HookType.BeforeScenario);

        var hookStartedEvent = new HookBindingStartedEvent(_mockHookBinding.Object, _mockContextManager.Object);
        var hookFinishedEvent = new HookBindingFinishedEvent(_mockHookBinding.Object, new TimeSpan(1), _mockContextManager.Object, ScenarioExecutionStatus.OK);

        // Initial setup
        await tracker.ProcessEvent(scenarioStartedEvent);

        // Act
        await tracker.ProcessEvent(hookStartedEvent);
        await tracker.ProcessEvent(hookFinishedEvent);

        // Assert - check the internal _currentTestCaseExecutionTracker
        tracker.CurrentTestCaseExecutionTracker.Should().NotBeNull();
        tracker.ExecutionHistory.Should().NotBeEmpty();

        // Assert there is exactly one execution record
        tracker.ExecutionHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProcessEvent_IgnoresTestRunHookEvents()
    {
        // Arrange
        var tracker = CreatePickleExecTracker();
        var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);

        _mockHookBinding.Setup(h => h.HookType).Returns(HookType.BeforeTestRun);

        var hookStartedEvent = new HookBindingStartedEvent(_mockHookBinding.Object, _mockContextManager.Object);
        var hookFinishedEvent = new HookBindingFinishedEvent(_mockHookBinding.Object, new TimeSpan(1), _mockContextManager.Object, ScenarioExecutionStatus.OK);

        // Initial setup
        await tracker.ProcessEvent(scenarioStartedEvent);

        // Act
        await tracker.ProcessEvent(hookStartedEvent);
        await tracker.ProcessEvent(hookFinishedEvent);

        // Assert - verify no hook messages were generated for test run hooks
        _mockMessageFactory.Verify(m => m.ToTestStepStarted(It.IsAny<HookStepExecutionTracker>()), Times.Never);
        _mockMessageFactory.Verify(m => m.ToTestStepFinished(It.IsAny<HookStepExecutionTracker>()), Times.Never);
    }

    [Fact]
    public async Task ProcessEvent_AttachmentAddedEvent_CreatesAttachmentMessage()
    {
        // Arrange
        var tracker = CreatePickleExecTracker();
        var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
        var stepStartedEvent = new StepStartedEvent(_featureContextStub, _scenarioContextSub, _mockStepContext.Object);

        var attachmentEvent = new AttachmentAddedEvent("file-path",
                                                       _featureInfoStub,
                                                       _scenarioInfoStub);

        // Initial setup
        await tracker.ProcessEvent(scenarioStartedEvent);
        await tracker.ProcessEvent(stepStartedEvent);

        // Act
        await tracker.ProcessEvent(attachmentEvent);
        // Assert
        _mockMessageFactory.Verify(m => m.ToAttachment(It.IsAny<AttachmentTracker>()), Times.Once);
    }

    [Fact]
    public async Task ProcessEvent_OutputAddedEvent_CreatesOutputMessage()
    {
        // Arrange
        var tracker = CreatePickleExecTracker();
        var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
        var stepStartedEvent = new StepStartedEvent(_featureContextStub, _scenarioContextSub, _mockStepContext.Object);

        var outputEvent = new OutputAddedEvent("test output",
                                               new FeatureInfo(CultureInfo.CurrentCulture, "", "Test Feature", ""),
                                               new ScenarioInfo("Test Scenario", "", [], null)
        );

        // Initial setup
        await tracker.ProcessEvent(scenarioStartedEvent);
        await tracker.ProcessEvent(stepStartedEvent);

        // Act
        await tracker.ProcessEvent(outputEvent);

        // Assert
        _mockMessageFactory.Verify(m => m.ToAttachment(It.IsAny<OutputMessageTracker>()), Times.Once);
    }

    private PickleExecutionTracker CreatePickleExecTracker()
    {
        return new PickleExecutionTracker(
            "test-pickle-id",
            "test-run-id",
            "Test Feature",
            enabled: true,
            _mockIdGenerator.Object,
            _stepDefinitionsByMethodSignature,
            Converters.ToDateTime(_testTime),
            _mockMessageFactory.Object,
            _stubTestCaseExecutionTrackerFactory,
            _mockPublisher.Object
        );
    }
}