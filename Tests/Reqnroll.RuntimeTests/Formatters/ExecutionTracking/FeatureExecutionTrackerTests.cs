using FluentAssertions;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class FeatureExecutionTrackerTests
{
    class TestableFeatureExecutionTracker(
        FeatureStartedEvent featureStartedEvent,
        string testRunStartedId,
        ConcurrentDictionary<IBinding, string> stepDefinitionsByBinding,
        IIdGenerator idGenerator,
        IPickleExecutionTrackerFactory pickleFactory,
        IMessagePublisher publisher)
        : FeatureExecutionTracker(featureStartedEvent, testRunStartedId, stepDefinitionsByBinding, idGenerator, pickleFactory, publisher)
    {
        public void SetPickleJar(PickleJar value) => PickleJar = value;
    }

    private Mock<IIdGenerator> _idGeneratorMock;
    private FeatureStartedEvent _featureStartedEventDummy;
    private IFeatureContext _mockFeatureContext;
    private Mock<IPickleExecutionTracker> _pickleTrackerMock;

    private ConcurrentDictionary<IBinding, string> _stepDefinitionsByBinding;
    private int _idCounter;
    private FeatureInfo _featureInfoDummy;
    private Mock<IClock> _clockMock;
    private Mock<IObjectContainer> _featureContainer;
    private Mock<IMessagePublisher> _publisherMock;

    private TestableFeatureExecutionTracker InitializeFeatureTrackerSut()
    {
        _idCounter = 10;
        _idGeneratorMock = new Mock<IIdGenerator>();
        _idGeneratorMock.Setup(g => g.GetNewId()).Returns(() => _idCounter++.ToString());
        _clockMock = new Mock<IClock>();
        _clockMock.Setup(clock => clock.GetNowDateAndTime()).Returns(DateTime.UnixEpoch);
        _publisherMock = new Mock<IMessagePublisher>();

        _featureContainer = new Mock<IObjectContainer>();
        _featureContainer.Setup(c => c.Resolve<IClock>()).Returns(_clockMock.Object);
        var mockFeatureContext = new Mock<IFeatureContext>();
        _mockFeatureContext = mockFeatureContext.Object;


        _stepDefinitionsByBinding = new ConcurrentDictionary<IBinding, string>();
        var featureLevelCucumberMessagesMock = new Mock<IFeatureLevelCucumberMessages>();
        featureLevelCucumberMessagesMock.SetupGet(m => m.Source).Returns(new Source("c:\\file", "Feature test", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN));
        featureLevelCucumberMessagesMock.SetupGet(m => m.GherkinDocument).Returns(new GherkinDocument("", new Feature(new Location(0, 0), [], "en", "Feature", "Dummy Feature", "", new List<FeatureChild>()), []));
        featureLevelCucumberMessagesMock.SetupGet(m => m.Pickles).Returns( new List<Pickle>([new Pickle("0", "", "", "en", [new PickleStep(null, [], "step1", PickleStepType.ACTION, "I eat a cuke")], [], [])]));
        featureLevelCucumberMessagesMock.SetupGet(m => m.HasMessages).Returns(true);

        _featureInfoDummy = new FeatureInfo(CultureInfo.InvariantCulture, null, "Test Feature", null, ProgrammingLanguage.CSharp, null, featureLevelCucumberMessagesMock.Object);
        mockFeatureContext.Setup(m => m.FeatureInfo).Returns(_featureInfoDummy);
        mockFeatureContext.Setup(m => m.FeatureContainer).Returns(_featureContainer.Object);

        _featureStartedEventDummy = new FeatureStartedEvent(mockFeatureContext.Object);

        _pickleTrackerMock = new Mock<IPickleExecutionTracker>();
        _pickleTrackerMock.Setup(t => t.TestCaseStartedTimeStamp).Returns(DateTime.Now);

        var pickleFactoryMock = new Mock<IPickleExecutionTrackerFactory>();
        pickleFactoryMock.Setup(f => f.CreatePickleTracker(It.IsAny<IFeatureExecutionTracker>(), It.IsAny<string>()))
            .Returns(_pickleTrackerMock.Object);

        // Initialize the FeatureExecutionTracker
        return new TestableFeatureExecutionTracker(_featureStartedEventDummy, "TestRunId", _stepDefinitionsByBinding, _idGeneratorMock.Object, pickleFactoryMock.Object, _publisherMock.Object);
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
    public async Task ProcessEvent_Should_Generate_StaticMessages_On_FeatureStartedEvent()
    {
        var sut = InitializeFeatureTrackerSut();
        // Act
        await sut.ProcessEvent(_featureStartedEventDummy); 

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<Envelope>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessEvent_Should_Calculate_FeatureExecutionSuccess_On_TestRunFinished()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();

        // Act
        await sut.FinalizeTracking();

        // Assert
        sut.FeatureExecutionSuccess.Should().BeTrue(); // No test cases were added, so it should default to true
    }

    [Fact]
    public async Task ProcessEvent_Should_Calculate_FeatureExecutionFailure_On_FeatureFinishedEventWhenAScenarioFails()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        _pickleTrackerMock.Setup(t => t.ScenarioExecutionStatus).Returns(ScenarioExecutionStatus.TestError);
        _pickleTrackerMock.Setup(t => t.Finished).Returns(true);
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);
        var scenarioFinishedEventMock = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock.Object);

        // Act
        await sut.ProcessEvent(scenarioFinishedEventMock.Object);
        await sut.FinalizeTracking();

        // Assert
        sut.FeatureExecutionSuccess.Should().BeFalse(); 
    }

    [Fact]
    public async Task ProcessEvent_Should_Calculate_FeatureExecutionFailure_On_FeatureFinishedEvent_When_AScenarioFails_And_AScenarioIsSkipped()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();

        // Set up one scenario to fail
        _pickleTrackerMock.Setup(t => t.ScenarioExecutionStatus).Returns(ScenarioExecutionStatus.TestError);
        _pickleTrackerMock.Setup(t => t.Finished).Returns(true);
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);
        var scenarioFinishedEventMock0 = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock.Object);

        // Set up another scenario to be skipped
        var pickleTrackerMock2 = new Mock<IPickleExecutionTracker>();
        pickleTrackerMock2.Setup(t => t.ScenarioExecutionStatus).Returns(ScenarioExecutionStatus.Skipped);
        pickleTrackerMock2.Setup(t => t.Finished).Returns(true);
        sut.GetOrAddPickleExecutionTracker("1");

        var scenarioInfoDummy1 = new ScenarioInfo("dummy SI", "", null, null, null, "1");
        var scenarioContextMock1 = new Mock<IScenarioContext>();
        scenarioContextMock1.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy1);
        var scenarioFinishedEventMock1 = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock1.Object);

        // Act
        await sut.ProcessEvent(scenarioFinishedEventMock0.Object);
        await sut.ProcessEvent(scenarioFinishedEventMock1.Object);
        await sut.FinalizeTracking();

        // Assert
        sut.FeatureExecutionSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessEvent_Should_Calculate_FeatureExecutionSuccess_On_FeatureFinishedEvent_When_AScenarioSucceeds_And_AScenarioIsSkipped()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();

        // Set up one scenario to succeed
        _pickleTrackerMock.Setup(t => t.ScenarioExecutionStatus).Returns(ScenarioExecutionStatus.OK);
        _pickleTrackerMock.Setup(t => t.Finished).Returns(true);
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);
        var scenarioFinishedEventMock0 = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock.Object);

        // Set up another scenario to be skipped
        var pickleTrackerMock2 = new Mock<IPickleExecutionTracker>();
        pickleTrackerMock2.Setup(t => t.ScenarioExecutionStatus).Returns(ScenarioExecutionStatus.Skipped);
        pickleTrackerMock2.Setup(t => t.Finished).Returns(true);
        sut.GetOrAddPickleExecutionTracker("1");

        var scenarioInfoDummy1 = new ScenarioInfo("dummy SI", "", null, null, null, "1");
        var scenarioContextMock1 = new Mock<IScenarioContext>();
        scenarioContextMock1.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy1);
        var scenarioFinishedEventMock1 = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock1.Object);

        // Act
        await sut.ProcessEvent(scenarioFinishedEventMock0.Object);
        await sut.ProcessEvent(scenarioFinishedEventMock1.Object);
        await sut.FinalizeTracking();

        // Assert
        sut.FeatureExecutionSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessEvent_Should_Calculate_FeatureExecutionSuccess_On_FeatureFinishedEvent_When_All_ScenarioAreSkipped()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();

        // Set up one scenario to be skipped
        _pickleTrackerMock.Setup(t => t.ScenarioExecutionStatus).Returns(ScenarioExecutionStatus.Skipped);
        _pickleTrackerMock.Setup(t => t.Finished).Returns(true);
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);
        var scenarioFinishedEventMock0 = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock.Object);

        // Set up another scenario to be skipped
        var pickleTrackerMock2 = new Mock<IPickleExecutionTracker>();
        pickleTrackerMock2.Setup(t => t.ScenarioExecutionStatus).Returns(ScenarioExecutionStatus.Skipped);
        pickleTrackerMock2.Setup(t => t.Finished).Returns(true);
        sut.GetOrAddPickleExecutionTracker("1");

        var scenarioInfoDummy1 = new ScenarioInfo("dummy SI", "", null, null, null, "1");
        var scenarioContextMock1 = new Mock<IScenarioContext>();
        scenarioContextMock1.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy1);
        var scenarioFinishedEventMock1 = new Mock<ScenarioFinishedEvent>(MockBehavior.Strict, _mockFeatureContext, scenarioContextMock1.Object);

        // Act
        await sut.ProcessEvent(scenarioFinishedEventMock0.Object);
        await sut.ProcessEvent(scenarioFinishedEventMock1.Object);
        await sut.FinalizeTracking();

        // Assert
        sut.FeatureExecutionSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessEvent_Should_Throw_Exception_For_Invalid_ScenarioStartedEvent()
    {
        // Arrange
        var scenarioStartedEventMock = new Mock<ScenarioStartedEvent>(MockBehavior.Strict, null!, null!);
        var sut = InitializeFeatureTrackerSut();

        // Act
        Func<Task> act = async () => await sut.ProcessEvent(scenarioStartedEventMock.Object);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessEvent_Should_Handle_Valid_ScenarioStartedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        // set up the PickleJar and PickleIds for the Scenario in this test
        sut.SetPickleJar(new PickleJar(new List<Pickle> { new("0", "", "dummyPickle Name", "en-US", new List<PickleStep>(), new List<PickleTag>(), [""]) }));
        sut.PickleIds.Add("0", "0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);

        var scenarioStartedEvent = new ScenarioStartedEvent(_mockFeatureContext, scenarioContextMock.Object);

        // Act
        await sut.ProcessEvent(scenarioStartedEvent);
        // Assert
        _pickleTrackerMock.Verify(t => t.ProcessEvent(scenarioStartedEvent));
        sut.PickleExecutionTrackers.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProcessEvent_Should_Handle_ScenarioFinishedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        // set up the PickleJar and PickleIds for the Scenario in this test
        sut.SetPickleJar(new PickleJar(new List<Pickle> { new("0", "", "dummyPickle Name", "en-US", new List<PickleStep>(), new List<PickleTag>(), [""]) }));
        sut.PickleIds.Add("0", "0");
        // Ensure that the PickleExecutionTracker is created for the Scenario
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);
        var scenarioFinishedEventMock = new ScenarioFinishedEvent(_mockFeatureContext, scenarioContextMock.Object);

        // Act
        await sut.ProcessEvent(scenarioFinishedEventMock);

        // Assert that the ScenarioFinishedEvent was dispatched to the PickleExecutionTracker
        _pickleTrackerMock.Verify(t => t.ProcessEvent(scenarioFinishedEventMock));
    }

    [Fact]
    public async Task ProcessEvent_Should_Handle_StepStartedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        // set up the PickleJar and PickleIds for the Scenario in this test
        sut.SetPickleJar(new PickleJar(new List<Pickle> { new("0", "", "dummyPickle Name", "en-US", new List<PickleStep>(), new List<PickleTag>(), [""]) }));
        sut.PickleIds.Add("0", "0");
        // Ensure that the PickleExecutionTracker is created for the Scenario

        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);

        var stepInfo = new StepInfo(StepDefinitionType.Given, "I eat a cuke", null, null, "step1");
        var stepContext = new ScenarioStepContext(stepInfo);

        var stepStartedEvent = new StepStartedEvent(_mockFeatureContext, scenarioContextMock.Object, stepContext);

        // Act
        await sut.ProcessEvent(stepStartedEvent);

        // Assert that the StepStartedEvent was dispatched to the PickleExecutionTracker
        _pickleTrackerMock.Verify(t => t.ProcessEvent(stepStartedEvent));
    }

    [Fact]
    public async Task ProcessEvent_Should_Handle_StepFinishedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        // set up the PickleJar and PickleIds for the Scenario in this test
        sut.SetPickleJar(new PickleJar(new List<Pickle> { new("0", "", "dummyPickle Name", "en-US", new List<PickleStep>(), new List<PickleTag>(), [""]) }));
        sut.PickleIds.Add("0", "0");
        // Ensure that the PickleExecutionTracker is created for the Scenario
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.Setup(m => m.ScenarioInfo).Returns(scenarioInfoDummy);

        var stepInfo = new StepInfo(StepDefinitionType.Given, "I eat a cuke", null, null, "step1");
        var stepContext = new ScenarioStepContext(stepInfo);

        stepContext.Status = ScenarioExecutionStatus.OK;
        var stepFinishedEvent = new StepFinishedEvent(_mockFeatureContext, scenarioContextMock.Object, stepContext);

        // Act
        await sut.ProcessEvent(stepFinishedEvent);

        // Assert
        _pickleTrackerMock.Verify(t => t.ProcessEvent(stepFinishedEvent));
    }

    [Fact]
    public async Task ProcessEvent_Should_Handle_HookBindingStartedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        // set up the PickleJar and PickleIds for the Scenario in this test
        sut.SetPickleJar(new PickleJar(new List<Pickle> { new("0", "", "dummyPickle Name", "en-US", new List<PickleStep>(), new List<PickleTag>(), [""]) }));
        sut.PickleIds.Add("0", "0");
        // Ensure that the PickleExecutionTracker is created for the Scenario
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");

        var testOjbResolverMock = new Mock<ITestObjectResolver>();
        var contextManagerMock = new Mock<IContextManager>();
        contextManagerMock.Setup(cm => cm.ScenarioContext).Returns(new ScenarioContext(null, scenarioInfoDummy, null, testOjbResolverMock.Object));

        var hookBindingStarted = new HookBindingStartedEvent(null, contextManagerMock.Object);

        // Act
        await sut.ProcessEvent(hookBindingStarted);

        // Assert
        _pickleTrackerMock.Verify(t => t.ProcessEvent(hookBindingStarted));
    }

    [Fact]
    public async Task ProcessEvent_Should_Handle_HookBindingFinishedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        // set up the PickleJar and PickleIds for the Scenario in this test
        sut.SetPickleJar(new PickleJar(new List<Pickle> { new("0", "", "dummyPickle Name", "en-US", new List<PickleStep>(), new List<PickleTag>(), [""]) }));
        sut.PickleIds.Add("0", "0");
        // Ensure that the PickleExecutionTracker is created for the Scenario
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0");

        var testOjbResolverMock = new Mock<ITestObjectResolver>();

        var contextManagerMock = new Mock<IContextManager>();
        contextManagerMock.Setup(cm => cm.ScenarioContext).Returns(new ScenarioContext(null, scenarioInfoDummy, null, testOjbResolverMock.Object));

        var hookBindingFinished = new HookBindingFinishedEvent(null, TimeSpan.Zero, contextManagerMock.Object, ScenarioExecutionStatus.OK);

        // Act
        await sut.ProcessEvent(hookBindingFinished);

        // Assert
        _pickleTrackerMock.Verify(t => t.ProcessEvent(hookBindingFinished));
    }

    [Fact]
    public async Task ProcessEvent_Should_Handle_AttachmentAddedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        // set up the PickleJar and PickleIds for the Scenario in this test
        sut.SetPickleJar(new PickleJar(new List<Pickle> { new("0", "", "dummyPickle Name", "en-US", new List<PickleStep>(), new List<PickleTag>(), [""]) }));
        sut.PickleIds.Add("0", "0");
        // Ensure that the PickleExecutionTracker is created for the Scenario
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0")
        {
            PickleId = "0"
        };

        var attachmentAddedEvent = new AttachmentAddedEvent("attachmentFileName.png", _mockFeatureContext.FeatureInfo,  scenarioInfoDummy);

        // Act
        await sut.ProcessEvent(attachmentAddedEvent);

        // Assert
        _pickleTrackerMock.Verify(t => t.ProcessEvent(attachmentAddedEvent));
    }

    [Fact]
    public async Task ProcessEvent_Should_Handle_OutputAddedEvent()
    {
        // Arrange
        var sut = InitializeFeatureTrackerSut();
        // set up the PickleJar and PickleIds for the Scenario in this test
        sut.SetPickleJar(new PickleJar(new List<Pickle> { new("0", "", "dummyPickle Name", "en-US", new List<PickleStep>(), new List<PickleTag>(), [""]) }));
        sut.PickleIds.Add("0", "0");
        // Ensure that the PickleExecutionTracker is created for the Scenario
        sut.GetOrAddPickleExecutionTracker("0");

        var scenarioInfoDummy = new ScenarioInfo("dummy SI", "", null, null, null, "0")
        {
            PickleId = "0"
        };

        var outputAddedEvent = new OutputAddedEvent("sample output text", _featureInfoDummy, scenarioInfoDummy);

        // Act
        await sut.ProcessEvent(outputAddedEvent);

        // Assert
        _pickleTrackerMock.Verify(t => t.ProcessEvent(outputAddedEvent));
    }
}