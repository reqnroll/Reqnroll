using FluentAssertions;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
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

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class FeatureExecutionTrackerTests
{
    private Mock<IIdGenerator> _idGeneratorMock;
    private FeatureStartedEvent _featureStartedEventDummy;
    private IFeatureContext _mockFeatureContext;
    private Mock<IPickleExecutionTracker> _testCaseTrackerMock;

    private ConcurrentDictionary<IBinding, string> _stepDefinitionsByBinding;
    private int _idCounter;
    private FeatureInfo _featureInfoDummy;
    private Mock<IClock> _clockMock;
    private Mock<IObjectContainer> _featureContainer;

    private FeatureExecutionTracker InitializeFeatureTrackerSut()
    {
        _idCounter = 10;
        _idGeneratorMock = new Mock<IIdGenerator>();
        _idGeneratorMock.Setup(g => g.GetNewId()).Returns(() => _idCounter++.ToString());
        _clockMock = new Mock<IClock>();
        _clockMock.Setup(clock => clock.GetNowDateAndTime()).Returns(DateTime.UnixEpoch);

        _featureContainer = new Mock<IObjectContainer>();
        _featureContainer.Setup(c => c.Resolve<IClock>()).Returns(_clockMock.Object);
        var mockFeatureContext = new Mock<IFeatureContext>();
        _mockFeatureContext = mockFeatureContext.Object;


        _stepDefinitionsByBinding = new ConcurrentDictionary<IBinding, string>();
        var featureLevelCucumberMessagesDummy = new FeatureLevelCucumberMessages(
            () => new Source("c:\\file", "Feature test", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN),
            () => new GherkinDocument("", new Feature(new Location(0, 0), [], "en", "Feature", "Dummy Feature", "", new List<FeatureChild>()), []),
            () => new List<Pickle>([new Pickle("0", "", "", "en", [new PickleStep(null, [], "step1", PickleStepType.ACTION, "I eat a cuke")], [], [])]));

        _featureInfoDummy = new FeatureInfo(CultureInfo.InvariantCulture, null, "Test Feature", null, ProgrammingLanguage.CSharp, null, featureLevelCucumberMessagesDummy);
        mockFeatureContext.Setup(m => m.FeatureInfo).Returns(_featureInfoDummy);
        mockFeatureContext.Setup(m => m.FeatureContainer).Returns(_featureContainer.Object);

        _featureStartedEventDummy = new FeatureStartedEvent(mockFeatureContext.Object);

        // Initialize the FeatureExecutionTracker
        var ft = new FeatureExecutionTracker(_featureStartedEventDummy, "TestRunId", _idGeneratorMock.Object, _stepDefinitionsByBinding, new CucumberMessageFactory());

        _testCaseTrackerMock = new Mock<IPickleExecutionTracker>();
        _testCaseTrackerMock.Setup(t => t.TestCaseStartedTimeStamp).Returns(DateTime.Now);

        ft.PickleExecutionTrackerFactory = (_, _) => _testCaseTrackerMock.Object;
        return ft;
    }

    [Fact]
    public void Constructor_Should_Initialize_Properties()
    {
        var sut = InitializeFeatureTrackerSut();

        // Assert
        sut.Enabled.Should().BeTrue();
        sut.FeatureName.Should().Be("Test Feature");
        sut.TestRunStartedId.Should().Be("TestRunId");
        sut.StepDefinitionsByBinding.Should().BeSameAs(_stepDefinitionsByBinding);
    }

    [Fact]
    public void ProcessEvent_Should_Generate_StaticMessages_On_FeatureStartedEvent()
    {
        var sut = InitializeFeatureTrackerSut();
        // Act
        // sut.ProcessEvent(_featureStartedEventDummy); //the featureStartedEvent is processed during FeatureExecutionTracker constructor

        // Assert
        sut.StaticMessages.Should().NotBeNull();
        sut.StaticMessages.ToList().Count.Should().Be(3);
    }

    [Fact]
    public void ProcessEvent_Should_Calculate_FeatureExecutionSuccess_On_FeatureFinishedEvent()
    {
        // Arrange
        var featureFinishedEventMock = new Mock<FeatureFinishedEvent>(MockBehavior.Strict, null!);
        var sut = InitializeFeatureTrackerSut();

        // Act
        sut.ProcessEvent(featureFinishedEventMock.Object);

        // Assert
        sut.FeatureExecutionSuccess.Should().BeTrue(); // No test cases were added, so it should default to true
    }

    [Fact]
    public void ProcessEvent_Should_Calculate_FeatureExecutionFailure_On_FeatureFinishedEventWhenAScenarioFails()
    {
        // Arrange
        var featureFinishedEventMock = new Mock<FeatureFinishedEvent>(MockBehavior.Strict, null!);
        var sut = InitializeFeatureTrackerSut();
        _testCaseTrackerMock.Setup(t => t.ScenarioExecutionStatus).Returns(ScenarioExecutionStatus.TestError);
        _testCaseTrackerMock.Setup(t => t.Finished).Returns(true);
        _ = sut.StaticMessages.ToList();
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);
        var scenarioFinishedEventMock = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock.Object);

        // Act
        sut.ProcessEvent(scenarioFinishedEventMock.Object);
        sut.ProcessEvent(featureFinishedEventMock.Object);

        // Assert
        sut.FeatureExecutionSuccess.Should().BeFalse(); // No test cases were added, so it should default to true
    }



    [Fact]
    public void ProcessEvent_Should_Throw_Exception_For_Invalid_ScenarioStartedEvent()
    {
        // Arrange
        var scenarioStartedEventMock = new Mock<ScenarioStartedEvent>(MockBehavior.Strict, null!, null!);
        var sut = InitializeFeatureTrackerSut();

        // Act
        Action act = () => sut.ProcessEvent(scenarioStartedEventMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProcessEvent_Should_Handle_Valid_ScenarioStartedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);

        var scenarioStartedEventMock = new Mock<ScenarioStartedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock.Object);
        _ = sut.StaticMessages.ToList();

        // Act
        sut.ProcessEvent(scenarioStartedEventMock.Object);
        // Assert
        _testCaseTrackerMock.Verify(t => t.ProcessEvent(scenarioStartedEventMock.Object));
        sut.PickleExecutionTrackers.Should().NotBeEmpty();
    }

    [Fact]
    public void ProcessEvent_Should_Handle_ScenarioFinishedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        _ = sut.StaticMessages.ToList();
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);
        var scenarioFinishedEventMock = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock.Object);

        // Act
        sut.ProcessEvent(scenarioFinishedEventMock.Object);

        // Assert that the ScenarioFinishedEvent was dispatched to the PickleExecutionTracker
        _testCaseTrackerMock.Verify(t => t.ProcessEvent(scenarioFinishedEventMock.Object));
    }

    [Fact]
    public void ProcessEvent_Should_Handle_StepStartedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        _ = sut.StaticMessages.ToList();
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);

        var stepInfo = new StepInfo(StepDefinitionType.Given, "I eat a cuke", null, null, "step1");
        var stepContext = new ScenarioStepContext(stepInfo);

        var stepStartedEvent = new StepStartedEvent(_mockFeatureContext, scenarioContextMock.Object, stepContext);

        // Act
        sut.ProcessEvent(stepStartedEvent);

        // Assert that the StepStartedEvent was dispatched to the PickleExecutionTracker
        _testCaseTrackerMock.Verify(t => t.ProcessEvent(stepStartedEvent));
    }

    [Fact]
    public void ProcessEvent_Should_Handle_StepFinishedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        _ = sut.StaticMessages.ToList();
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);

        var stepInfo = new StepInfo(StepDefinitionType.Given, "I eat a cuke", null, null, "step1");
        var stepContext = new ScenarioStepContext(stepInfo);

        stepContext.Status = ScenarioExecutionStatus.OK;
        var stepFinishedEvent = new StepFinishedEvent(_mockFeatureContext, scenarioContextMock.Object, stepContext);

        // Act
        sut.ProcessEvent(stepFinishedEvent);

        // Assert
        _testCaseTrackerMock.Verify(t => t.ProcessEvent(stepFinishedEvent));
    }

    [Fact]
    public void ProcessEvent_Should_Handle_HookBindingStartedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        _ = sut.StaticMessages.ToList();
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");

        var testOjbResolverMock = new Mock<ITestObjectResolver>();
        var contextManagerMock = new Mock<IContextManager>();
        contextManagerMock.Setup(cm => cm.ScenarioContext).Returns(new ScenarioContext(null, scenarioInfoDummy, null, testOjbResolverMock.Object));

        var hookBindingStarted = new HookBindingStartedEvent(null, contextManagerMock.Object);

        // Act
        sut.ProcessEvent(hookBindingStarted);

        // Assert
        _testCaseTrackerMock.Verify(t => t.ProcessEvent(hookBindingStarted));
    }

    [Fact]
    public void ProcessEvent_Should_Handle_HookBindingFinishedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        _ = sut.StaticMessages.ToList();
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");

        var testOjbResolverMock = new Mock<ITestObjectResolver>();

        var contextManagerMock = new Mock<IContextManager>();
        contextManagerMock.Setup(cm => cm.ScenarioContext).Returns(new ScenarioContext(null, scenarioInfoDummy, null, testOjbResolverMock.Object));

        var hookBindingFinished = new HookBindingFinishedEvent(null, new TimeSpan(), contextManagerMock.Object);

        // Act
        sut.ProcessEvent(hookBindingFinished);

        // Assert
        _testCaseTrackerMock.Verify(t => t.ProcessEvent(hookBindingFinished));
    }

    [Fact]
    public void ProcessEvent_Should_Handle_AttachmentAddedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        _ = sut.StaticMessages.ToList();
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        scenarioInfoDummy.PickleId = "0";

        var attachmentAddedEvent = new AttachmentAddedEvent("attachmentFileName.png", _mockFeatureContext.FeatureInfo,  scenarioInfoDummy);

        // Act
        sut.ProcessEvent(attachmentAddedEvent);

        // Assert
        _testCaseTrackerMock.Verify(t => t.ProcessEvent(attachmentAddedEvent));
    }

    [Fact]
    public void ProcessEvent_Should_Handle_OutputAddedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        _ = sut.StaticMessages.ToList();
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        scenarioInfoDummy.PickleId = "0";

        var outputAddedEvent = new OutputAddedEvent("sample output text", _featureInfoDummy, scenarioInfoDummy);

        // Act
        sut.ProcessEvent(outputAddedEvent);

        // Assert
        _testCaseTrackerMock.Verify(t => t.ProcessEvent(outputAddedEvent));
    }
}