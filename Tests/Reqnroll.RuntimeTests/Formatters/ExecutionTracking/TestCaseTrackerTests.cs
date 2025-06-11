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
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using HookType = Reqnroll.Bindings.HookType;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking
{
    public class TestCaseTrackerTests
    {
        private readonly Mock<IIdGenerator> _mockIdGenerator;
        private readonly Mock<ICucumberMessageFactory> _mockMessageFactory;
        private FeatureInfo _featureInfoStub;
        private FeatureContext _featureContextStub;
        private ScenarioInfo _scenarioInfoStub;
        private ScenarioContext _scenarioContextSub;
        private Mock<IScenarioStepContext> _mockStepContext;
        private Mock<IContextManager> _mockContextManager;
        private readonly Mock<IHookBinding> _mockHookBinding;
        private readonly ConcurrentDictionary<string, string> _stepDefinitionsByMethodSignature;
        private readonly Timestamp _testTime = new Timestamp(0, 1);
        private readonly ObjectContainer objectContainerStub = new();

        public TestCaseTrackerTests()
        {
            _mockIdGenerator = new Mock<IIdGenerator>();
            _mockIdGenerator.Setup(x => x.GetNewId()).Returns("test-id");

            _mockMessageFactory = new Mock<ICucumberMessageFactory>();

            SetupMockContexts();

            _mockHookBinding = new Mock<IHookBinding>();

            _stepDefinitionsByMethodSignature = new ConcurrentDictionary<string, string>();

            // Setup message factory mocks to return test envelopes
            _mockMessageFactory
                .Setup(m => m.ToTestCase(It.IsAny<TestCaseDefinition>()))
                .Returns(new TestCase("test-id", "test-pickle-id", new List<TestStep>(), "test-run-started-id"));

            _mockMessageFactory
                .Setup(m => m.ToTestCaseStarted(It.IsAny<TestCaseExecutionRecord>(), It.IsAny<string>()))
                .Returns(new TestCaseStarted(0, "test-case-started-id", "test-id", "", _testTime));

            _mockMessageFactory
                .Setup(m => m.ToTestCaseFinished(It.IsAny<TestCaseExecutionRecord>()))
                .Returns(new TestCaseFinished("test-case-started-id", _testTime, false));

            _mockMessageFactory
                .Setup(m => m.ToTestStepStarted(It.IsAny<TestStepTracker>()))
                .Returns(new TestStepStarted("testCaseStartedId", "test-step-Id", new Timestamp(0, 1)));

            _mockMessageFactory
                .Setup(m => m.ToTestStepFinished(It.IsAny<TestStepTracker>()))
                .Returns(new TestStepFinished("testCaseStartedId", "test-step-Id", new TestStepResult(new Duration(0, 1), "result-message", TestStepResultStatus.PASSED, null), new Timestamp(0, 1)));

            _mockMessageFactory
                .Setup(m => m.ToTestStepStarted(It.IsAny<HookStepTracker>()))
                .Returns(new TestStepStarted("testCaseStartedId", "hook-Id", new Timestamp(0, 1)));

            _mockMessageFactory
                .Setup(m => m.ToTestStepFinished(It.IsAny<HookStepTracker>()))
                .Returns(new TestStepFinished("testCaseStartedId", "hook-Id", new TestStepResult(new Duration(0, 1), "result-message", TestStepResultStatus.PASSED, null), new Timestamp(0, 1)));

            _mockMessageFactory
                .Setup(m => m.ToAttachment(It.IsAny<AttachmentAddedEventWrapper>()))
                .Returns(new Attachment("attachmentbody", AttachmentContentEncoding.BASE64, "filename", "mediatype", new Source("uri", "data", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN), "test-case-started-id", "test-step-Id", "url", "test-run-started-id"));

            _mockMessageFactory
                .Setup(m => m.ToAttachment(It.IsAny<OutputAddedEventWrapper>()))
                .Returns(new Attachment("attachmentbody", AttachmentContentEncoding.BASE64, "filename", "mediatype", new Source("uri", "data", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN), "test-case-started-id", "test-step-Id", "url", "test-run-started-id"));

            // Setup message factory to canonicalize a method signature
            _mockMessageFactory
                .Setup(m => m.CanonicalizeHookBinding(It.IsAny<IHookBinding>()))
                .Returns("hookBinding-method-signature");
            _stepDefinitionsByMethodSignature.TryAdd("hookBinding-method-signature", "hook-id");
        }

        private void SetupMockContexts()
        {
            var testOjbResolverMock = new Mock<ITestObjectResolver>();

            _featureInfoStub = new FeatureInfo(CultureInfo.CurrentCulture, "", "Test Feature", "");
            _featureContextStub = new FeatureContext(objectContainerStub, _featureInfoStub, ConfigurationLoader.GetDefault());

            _scenarioInfoStub = new ScenarioInfo("Test Scenario", "", [], null);
            _scenarioContextSub = new ScenarioContext(objectContainerStub, _scenarioInfoStub, null, testOjbResolverMock.Object);

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
            var tracker = CreateTestCaseTracker();

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
        public void ProcessEvent_ScenarioStartedEvent_IncrementsAttemptCountAndCreatesTestCaseDefinition()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);

            // Act
            tracker.ProcessEvent(scenarioStartedEvent);

            // Assert
            tracker.AttemptCount.Should().Be(0);
            tracker.TestCaseId.Should().Be("test-id");
            tracker.TestCaseDefinition.Should().NotBeNull();
            tracker.Finished.Should().BeFalse();
            _scenarioInfoStub.PickleId.Should().Be("test-pickle-id");
        }

        [Fact]
        public void ProcessEvent_ScenarioFinishedEvent_SetsFinishedFlag()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
            var scenarioFinishedEvent = new ScenarioFinishedEvent(_featureContextStub, _scenarioContextSub);

            tracker.ProcessEvent(scenarioStartedEvent);

            // Act
            tracker.ProcessEvent(scenarioFinishedEvent);

            // Assert
            tracker.Finished.Should().BeTrue();
        }

        [Fact]
        public void ProcessEvent_RetryScenario_CreatesMultipleExecutionRecords()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();

            // First execution
            var scenarioStartedEvent1 = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
            var scenarioFinishedEvent1 = new ScenarioFinishedEvent(_featureContextStub, _scenarioContextSub);

            // Second execution (retry)
            var scenarioStartedEvent2 = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
            var scenarioFinishedEvent2 = new ScenarioFinishedEvent(_featureContextStub, _scenarioContextSub);

            // Act
            tracker.ProcessEvent(scenarioStartedEvent1);
            tracker.ProcessEvent(scenarioFinishedEvent1);
            tracker.ProcessEvent(scenarioStartedEvent2);
            tracker.ProcessEvent(scenarioFinishedEvent2);

            // Assert
            tracker.AttemptCount.Should().Be(1);
            tracker.Finished.Should().BeTrue();

            // Check that RuntimeGeneratedMessages produces messages for both executions
            var messages = tracker.RuntimeGeneratedMessages.ToList();
            messages.Should().NotBeEmpty();

            // Verify that we have at least one message with willBeRetried=true
            var envelopesWithTestCaseFinished = messages
                .Where(e => e.Content() is TestCaseFinished)
                .ToList();

            envelopesWithTestCaseFinished.Should().HaveCountGreaterThan(1);

            // The first TestCaseFinished should have willBeRetried=true
            var firstTestCaseFinished = envelopesWithTestCaseFinished.First().Content() as TestCaseFinished;
            firstTestCaseFinished.WillBeRetried.Should().BeTrue();
        }

        [Fact]
        public void ProcessEvent_StepEvents_AddsStepTrackerToExecutionRecord()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
            var stepStartedEvent = new StepStartedEvent(_featureContextStub, _scenarioContextSub, _mockStepContext.Object);
            var stepFinishedEvent = new StepFinishedEvent(_featureContextStub, _scenarioContextSub, _mockStepContext.Object);

            // Initial setup
            tracker.ProcessEvent(scenarioStartedEvent);

            // Act
            tracker.ProcessEvent(stepStartedEvent);
            tracker.ProcessEvent(stepFinishedEvent);
            var msgs = tracker.RuntimeGeneratedMessages;

            // Assert - check that step events are being processed
            // Since we can't directly access the private fields, we'll verify through the mock message factory
            _mockMessageFactory.Verify(m => m.ToTestStepStarted(It.IsAny<TestStepTracker>()), Times.Once);
            _mockMessageFactory.Verify(m => m.ToTestStepFinished(It.IsAny<TestStepTracker>()), Times.Once);
        }

        [Fact]
        public void ProcessEvent_HookEvents_AddsHookTrackerToExecutionRecord()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);

            _mockHookBinding.Setup(h => h.HookType).Returns(HookType.BeforeScenario);

            var hookStartedEvent = new HookBindingStartedEvent(_mockHookBinding.Object, _mockContextManager.Object);
            var hookFinishedEvent = new HookBindingFinishedEvent(_mockHookBinding.Object, new TimeSpan(1), _mockContextManager.Object);

            // Initial setup
            tracker.ProcessEvent(scenarioStartedEvent);

            // Act
            tracker.ProcessEvent(hookStartedEvent);
            tracker.ProcessEvent(hookFinishedEvent);
            var msgs = tracker.RuntimeGeneratedMessages;

            // Assert - check through mock interactions
            _mockMessageFactory.Verify(m => m.ToTestStepStarted(It.IsAny<HookStepTracker>()), Times.AtLeastOnce);
            _mockMessageFactory.Verify(m => m.ToTestStepFinished(It.IsAny<HookStepTracker>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ProcessEvent_IgnoresTestRunHookEvents()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);

            _mockHookBinding.Setup(h => h.HookType).Returns(HookType.BeforeTestRun);

            var hookStartedEvent = new HookBindingStartedEvent(_mockHookBinding.Object, _mockContextManager.Object);
            var hookFinishedEvent = new HookBindingFinishedEvent(_mockHookBinding.Object, new TimeSpan(1), _mockContextManager.Object);

            // Initial setup
            tracker.ProcessEvent(scenarioStartedEvent);

            // Act
            tracker.ProcessEvent(hookStartedEvent);
            tracker.ProcessEvent(hookFinishedEvent);

            // Assert - verify no hook messages were generated for test run hooks
            _mockMessageFactory.Verify(m => m.ToTestStepStarted(It.IsAny<HookStepTracker>()), Times.Never);
            _mockMessageFactory.Verify(m => m.ToTestStepFinished(It.IsAny<HookStepTracker>()), Times.Never);
        }

        [Fact]
        public void ProcessEvent_AttachmentAddedEvent_CreatesAttachmentMessage()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
            var stepStartedEvent = new StepStartedEvent(_featureContextStub, _scenarioContextSub, _mockStepContext.Object);

            var attachmentEvent = new AttachmentAddedEvent("file-path",
                _featureInfoStub,
                _scenarioInfoStub);

            // Initial setup
            tracker.ProcessEvent(scenarioStartedEvent);
            tracker.ProcessEvent(stepStartedEvent);

            // Act
            tracker.ProcessEvent(attachmentEvent);
            var msgs = tracker.RuntimeGeneratedMessages;
            // Assert
            _mockMessageFactory.Verify(m => m.ToAttachment(It.IsAny<AttachmentAddedEventWrapper>()), Times.Once);
        }

        [Fact]
        public void ProcessEvent_OutputAddedEvent_CreatesOutputMessage()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var scenarioStartedEvent = new ScenarioStartedEvent(_featureContextStub, _scenarioContextSub);
            var stepStartedEvent = new StepStartedEvent(_featureContextStub, _scenarioContextSub, _mockStepContext.Object);

            var outputEvent = new OutputAddedEvent("test output",
                new FeatureInfo(CultureInfo.CurrentCulture, "", "Test Feature", ""),
                new ScenarioInfo("Test Scenario", "", [], null)
                );

            // Initial setup
            tracker.ProcessEvent(scenarioStartedEvent);
            tracker.ProcessEvent(stepStartedEvent);

            // Act
            tracker.ProcessEvent(outputEvent);
            var msgs = tracker.RuntimeGeneratedMessages;

            // Assert
            _mockMessageFactory.Verify(m => m.ToAttachment(It.IsAny<OutputAddedEventWrapper>()), Times.Once);
        }

        [Fact]
        public void ProcessEvent_ThrowsForUnsupportedEventType()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var unsupportedEvent = new Mock<ExecutionEvent>().Object;

            // Act & Assert
            Action action = () => tracker.ProcessEvent(unsupportedEvent);
            action.Should().Throw<NotImplementedException>()
                  .WithMessage($"Event type {unsupportedEvent.GetType().Name} is not supported.");
        }

        [Fact]
        public void FixupWillBeRetried_SetsWillBeRetriedFlagToTrue()
        {
            // Arrange
            var tracker = CreateTestCaseTracker();
            var testCaseFinished = new TestCaseFinished("test-case-started-id", _testTime, false);
            var envelope = Envelope.Create(testCaseFinished);

            // Act
            var fixedEnvelope = (Envelope)typeof(TestCaseTracker)
                .GetMethod("FixupWillBeRetried", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(tracker, new object[] { envelope });

            var fixedTestCaseFinished = fixedEnvelope.Content() as TestCaseFinished;

            // Assert
            fixedTestCaseFinished.Should().NotBeNull();
            fixedTestCaseFinished.WillBeRetried.Should().BeTrue();
        }

        private TestCaseTracker CreateTestCaseTracker()
        {
            return new TestCaseTracker(
                "test-pickle-id",
                "test-run-id",
                "Test Feature",
                enabled: true,
                _mockIdGenerator.Object,
                _stepDefinitionsByMethodSignature,
                Converters.ToDateTime(_testTime),
                _mockMessageFactory.Object
            );
        }
    }
}
