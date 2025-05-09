using FluentAssertions;
using Moq;
using Reqnroll.BoDi;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.ExecutionTracking;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.PubSub;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Gherkin.CucumberMessages;
using Reqnroll.Time;
using Reqnroll.Plugins;
using System.Reflection.Metadata.Ecma335;
using System.Reflection;
using Reqnroll.EnvironmentAccess;
using Reqnroll.CucumberMessages.RuntimeSupport;
using System.CodeDom;
using Reqnroll.Infrastructure;
using Reqnroll.Configuration;
using System.Collections.Specialized;

namespace Reqnroll.RuntimeTests.CucumberMessages.PubSub
{
    public class CucumberMessagePublisherTests
    {
        private readonly Mock<IObjectContainer> _objectContainerMock;
        private readonly Mock<ICucumberMessageBroker> _brokerMock;
        private readonly Mock<IBindingRegistry> _bindingRegistryMock;
        private readonly Mock<IIdGenerator> _idGeneratorMock;
        private readonly Mock<IClock> _clockMock;
        private readonly Mock<ITestThreadExecutionEventPublisher> _eventPublisherMock;
        private readonly RuntimePluginEvents _runtimePluginEvents;
        private readonly RuntimePluginParameters _runtimePluginParameters;
        private readonly UnitTestProviderConfiguration _unitTestProviderConfiguration;
        private readonly CucumberMessagePublisher _sut;

        public CucumberMessagePublisherTests()
        {
            _objectContainerMock = new Mock<IObjectContainer>();
            _brokerMock = new Mock<ICucumberMessageBroker>();
            _bindingRegistryMock = new Mock<IBindingRegistry>();
            _idGeneratorMock = new Mock<IIdGenerator>();
            _clockMock = new Mock<IClock>();
            _eventPublisherMock = new Mock<ITestThreadExecutionEventPublisher>();
            _clockMock.Setup(c => c.GetNowDateAndTime()).Returns(DateTime.UtcNow);

            _runtimePluginEvents = new RuntimePluginEvents();
            _runtimePluginParameters = new RuntimePluginParameters();
            _unitTestProviderConfiguration = new UnitTestProviderConfiguration();

            _sut = new CucumberMessagePublisher();
        }

        [Fact]
        public void Initialize_Should_Register_Global_Dependencies()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();

            // Act
            _sut.Initialize(_runtimePluginEvents, _runtimePluginParameters, _unitTestProviderConfiguration);
            _runtimePluginEvents.RaiseRegisterGlobalDependencies(objectContainerStub);

            // Assert
            objectContainerStub.IsRegistered<IIdGenerator>().Should().BeTrue();
        }


        [Fact]
        public void Initialize_Should_Register_StartupAndShutdownEventHandlers()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            var lifeCycleEvents = new RuntimePluginTestExecutionLifecycleEvents();
            objectContainerStub.RegisterInstanceAs(lifeCycleEvents);

            // Act
            _sut.Initialize(_runtimePluginEvents, _runtimePluginParameters, _unitTestProviderConfiguration);
            _runtimePluginEvents.RaiseCustomizeGlobalDependencies(objectContainerStub, null);

            // Assert
            var beforeSubscriptions = InspectLifecycleEvents(lifeCycleEvents, "BeforeTestRun");
            beforeSubscriptions.Should().HaveCount(1);
            var afterSubscriptions = InspectLifecycleEvents(lifeCycleEvents, "AfterTestRun");
            afterSubscriptions.Should().HaveCount(1);

            static IEnumerable<Delegate> InspectLifecycleEvents(RuntimePluginTestExecutionLifecycleEvents lifeCycleEvents, string fieldName)
            {
                var eventField = typeof(RuntimePluginTestExecutionLifecycleEvents).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                var eventDelegate = (Delegate)eventField.GetValue(lifeCycleEvents);
                return eventDelegate.GetInvocationList();
            }
        }

        [Fact]
        public void Initialize_Should_Setup_TestThread_Dependencies()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterInstanceAs<ITestThreadExecutionEventPublisher>(_eventPublisherMock.Object);
            objectContainerStub.RegisterInstanceAs<ICucumberMessageBroker>(_brokerMock.Object);

            // Act
            _sut.Initialize(_runtimePluginEvents, _runtimePluginParameters, _unitTestProviderConfiguration);
            _runtimePluginEvents.RaiseCustomizeTestThreadDependencies(objectContainerStub);

            // Assert
            _eventPublisherMock.Verify(e => e.AddAsyncListener(_sut), Times.Once);
            _sut._brokerFactory.Value.Should().Be(_brokerMock.Object);
        }

        // PublisherStartup/BeforeTestRun causes no side-effects when broker is disabled
        [Fact]
        public void PublisherStartup_Should_Not_Perform_Actions_When_Broker_Is_Disabled()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _brokerMock.Setup(b => b.Enabled).Returns(false);
            objectContainerStub.RegisterInstanceAs<ICucumberMessageBroker>(_brokerMock.Object);
            _sut._brokerFactory = new Lazy<ICucumberMessageBroker>(() => _brokerMock.Object);

            // Act
            _sut.PublisherStartup(null, new RuntimePluginBeforeTestRunEventArgs(objectContainerStub));


            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Never);
        }

        class PublisherStartup_FactoryStub : CucumberMessageFactoryInner
        {
            internal override TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
            {
                return new TestRunStarted(new Timestamp(1, 0), "");
            }
            internal override Meta ToMeta(IObjectContainer container)
            {
                return new Meta("", new Product("", ""), new Product("", ""), new Product("", ""), new Product("", ""), null);
            }

        }

        // PublisherStartup populates binding caches and publishes startup messages
        [Fact]
        public void PublisherStartup_Should_Populate_BindingCaches_And_Publish_Startup_Messages()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            var beforeTestRunArgs = new RuntimePluginBeforeTestRunEventArgs(objectContainerStub);
            // nb. This approach required as Moq can't mock types from Io.Cucumber.Messages.Types (as they're not visibleTo and sealed)
            var msgFactory = new PublisherStartup_FactoryStub();
            CucumberMessageFactory.inner = msgFactory;

            _brokerMock.Setup(b => b.Enabled).Returns(true);
            objectContainerStub.RegisterInstanceAs<ICucumberMessageBroker>(_brokerMock.Object);
            objectContainerStub.RegisterInstanceAs<IIdGenerator>(_idGeneratorMock.Object);
            objectContainerStub.RegisterInstanceAs<IClock>(_clockMock.Object);
            objectContainerStub.RegisterInstanceAs<IBindingRegistry>(_bindingRegistryMock.Object);

            _sut._brokerFactory = new Lazy<ICucumberMessageBroker>(() => _brokerMock.Object);
            _sut._testThreadObjectContainer = objectContainerStub;

            _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns(new List<IStepDefinitionBinding>());
            _bindingRegistryMock.Setup(br => br.GetHooks()).Returns(new List<IHookBinding>());
            _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns(new List<IStepArgumentTransformationBinding>());

            // Act
            _sut.PublisherStartup(null, new RuntimePluginBeforeTestRunEventArgs(objectContainerStub));

            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.AtLeast(2)); // TestRunStarted and Meta messages
            _bindingRegistryMock.Verify(br => br.GetStepDefinitions(), Times.Exactly(2));
            _bindingRegistryMock.Verify(br => br.GetHooks(), Times.Once);
            _bindingRegistryMock.Verify(br => br.GetStepTransformations(), Times.Once);
        }

        // PublisherTestRunComplete/AfterTestRun causes no side-effects when Publisher is disabled
        [Fact]
        public void PublisherTestRunComplete_Should_Not_Perform_Actions_When_Broker_Is_Disabled()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _brokerMock.Setup(b => b.Enabled).Returns(false);
            objectContainerStub.RegisterInstanceAs<ICucumberMessageBroker>(_brokerMock.Object);
            objectContainerStub.RegisterInstanceAs<IClock>(_clockMock.Object);
            _sut._brokerFactory = new Lazy<ICucumberMessageBroker>(() => _brokerMock.Object);

            // Act
            _sut.PublisherStartup(null, new RuntimePluginBeforeTestRunEventArgs(objectContainerStub));
            _sut.PublisherTestRunComplete(null, new RuntimePluginAfterTestRunEventArgs(objectContainerStub));


            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Never);
        }

        // PublisherTestRunComplete calculates test run status as Success when all started features are complete and succeeded
        [Fact]
        public void PublisherTestRunComplete_Should_CalculateStatusWhenAllFeaturesSucceed()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _brokerMock.Setup(b => b.Enabled).Returns(true);
            objectContainerStub.RegisterInstanceAs<IIdGenerator>(_idGeneratorMock.Object);
            objectContainerStub.RegisterInstanceAs<IClock>(_clockMock.Object);
            _idGeneratorMock.Setup(g => g.GetNewId()).Returns("1");

            var featureTrackerMock = new Mock<IFeatureTracker>();
            featureTrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(true);

            IList<Envelope> publishedEnvelopes = new List<Envelope>();
            _brokerMock.Setup(b => b.PublishAsync(It.IsAny<Envelope>())).Returns<Envelope>(
                (e) =>
                    {
                        publishedEnvelopes.Add(e);
                        return Task.CompletedTask;
                    });

            _sut._broker = _brokerMock.Object;
            _sut._startedFeatures.TryAdd("feature1", featureTrackerMock.Object);
            _sut._startedFeatures.TryAdd("feature2", featureTrackerMock.Object);
            _sut._enabled = true;

            // Act
            //_sut.PublisherStartup(null, new RuntimePluginBeforeTestRunEventArgs(objectContainerStub));
            _sut.PublisherTestRunComplete(null, new RuntimePluginAfterTestRunEventArgs(objectContainerStub));

            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Once);
            publishedEnvelopes.Should().HaveCount(1);
            var msg = publishedEnvelopes[0].Content();
            msg.Should().BeOfType<TestRunFinished>();
            var trf = msg as TestRunFinished;
            trf.Success.Should().BeTrue();
        }

        // PublisherTestRunComplete calculates test run status as Failed when any started feature is not complete or any are Failed
        [Fact]
        public void PublisherTestRunComplete_Should_CalculateStatusWhenAFeatureFails()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _brokerMock.Setup(b => b.Enabled).Returns(true);
            objectContainerStub.RegisterInstanceAs<IIdGenerator>(_idGeneratorMock.Object);
            objectContainerStub.RegisterInstanceAs<IClock>(_clockMock.Object);
            _idGeneratorMock.Setup(g => g.GetNewId()).Returns("1");

            var feature1TrackerMock = new Mock<IFeatureTracker>();
            feature1TrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(true);
            var feature2TrackerMock = new Mock<IFeatureTracker>();
            feature2TrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(false);

            IList<Envelope> publishedEnvelopes = new List<Envelope>();
            _brokerMock.Setup(b => b.PublishAsync(It.IsAny<Envelope>())).Returns<Envelope>(
                (e) =>
                {
                    publishedEnvelopes.Add(e);
                    return Task.CompletedTask;
                });

            _sut._broker = _brokerMock.Object;
            _sut._startedFeatures.TryAdd("feature1", feature1TrackerMock.Object);
            _sut._startedFeatures.TryAdd("feature2", feature2TrackerMock.Object);
            _sut._enabled = true;

            // Act
            //_sut.PublisherStartup(null, new RuntimePluginBeforeTestRunEventArgs(objectContainerStub));
            _sut.PublisherTestRunComplete(null, new RuntimePluginAfterTestRunEventArgs(objectContainerStub));

            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Once);
            publishedEnvelopes.Should().HaveCount(1);
            var msg = publishedEnvelopes[0].Content();
            msg.Should().BeOfType<TestRunFinished>();
            var trf = msg as TestRunFinished;
            trf.Success.Should().BeFalse();
        }

        // PublisherTestRunComplete publishes TestCase messages for Pickles, then Execution messages, followed by the TestRunFinished message
        [Fact]
        public void PublisherTestRunComplete_Should_PublishTestCase_and_Exec_Messages()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _brokerMock.Setup(b => b.Enabled).Returns(true);
            objectContainerStub.RegisterInstanceAs<IIdGenerator>(_idGeneratorMock.Object);
            objectContainerStub.RegisterInstanceAs<IClock>(_clockMock.Object);
            _idGeneratorMock.Setup(g => g.GetNewId()).Returns("1");

            var featureTrackerMock = new Mock<IFeatureTracker>();
            featureTrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(true);
            var messages = new List<Envelope>
            {
                Envelope.Create(new TestCase("t", "p", [], "0")),
                Envelope.Create(new TestCaseStarted(0, "2", "t", "", new Timestamp(1, 0)))
            };


            IList<Envelope> publishedEnvelopes = new List<Envelope>();
            _brokerMock.Setup(b => b.PublishAsync(It.IsAny<Envelope>())).Returns<Envelope>(
                (e) =>
                {
                    publishedEnvelopes.Add(e);
                    return Task.CompletedTask;
                });

            _sut._broker = _brokerMock.Object;
            _sut._startedFeatures.TryAdd("feature1", featureTrackerMock.Object);
            _sut._enabled = true;
            _sut._messages = messages;

            // Act
            _sut.PublisherTestRunComplete(null, new RuntimePluginAfterTestRunEventArgs(objectContainerStub));

            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Exactly(3));
            publishedEnvelopes.Should().HaveCount(3);
        }
        // FeatureStartedEvent causes no side-effects when broker is disabled
        [Fact]
        public async Task FeatureStartedEvent_Should_cause_no_sideEffects_When_Disabled()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterInstanceAs<ITraceListener>(new Mock<ITraceListener>().Object);
            var featureContextMock = new Mock<IFeatureContext>();

            _sut._enabled = false;
            _sut._broker = _brokerMock.Object;
            _sut._testThreadObjectContainer = objectContainerStub;

            // Act
            await _sut.OnEventAsync(new FeatureStartedEvent(featureContextMock.Object));

            // Assert
            _sut._startedFeatures.Count.Should().Be(0);
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Never);
        }
        // FeatureStartedEvent does not start a second Feature Tracker if one already exists for the given Feature name
        [Fact]
        public async Task FeatureStartedEvent_Should_not_startASecondFeatureTrackerIfOneAlreadyExistsForAGivenFeatureName()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterInstanceAs<ITraceListener>(new Mock<ITraceListener>().Object);
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDEF", null);
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);

            var existingFeatureTrackerMock = new Mock<IFeatureTracker>();
            _sut._startedFeatures.TryAdd("ABCDEF", existingFeatureTrackerMock.Object);
            _sut._broker = _brokerMock.Object;
            _sut._testThreadObjectContainer = objectContainerStub;
            _sut._enabled = true;

            // Act
            await _sut.OnEventAsync(new FeatureStartedEvent(featureContextMock.Object));

            // Assert
            _sut._startedFeatures.Count.Should().Be(1);
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Never);

        }

        // FeatureStartedEvent causes a FeatureTracker to be instantiated and Static Messages to be published to the Broker
        [Fact]
        public async Task FeatureStartedEvent_Should_InstantiateAFeatureTrackerAndPublishStaticMessages()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterInstanceAs<ITraceListener>(new Mock<ITraceListener>().Object);
            objectContainerStub.RegisterInstanceAs<IClock>(_clockMock.Object);
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDEF", null);
            var sourceFunc = () =>
            {
                return new Source("uri", "Feature test", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN);
            };
            var gherkinDocFunc = () =>
            {
                return new GherkinDocument("", new Feature(new Location(1, 1), [], "en", "Feature", "Feature test", "description", []), []);
            };
            var picklesFunc = () =>
            {
                return new List<Pickle>();
            };
            var featureMessagesStub = new FeatureLevelCucumberMessages(sourceFunc, gherkinDocFunc, picklesFunc, null);
            featureInfoStub.FeatureCucumberMessages = featureMessagesStub;
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);
            featureContextMock.Setup(fc => fc.FeatureContainer).Returns(objectContainerStub);

            IList<Envelope> publishedEnvelopes = new List<Envelope>();
            _brokerMock.Setup(b => b.PublishAsync(It.IsAny<Envelope>())).Returns<Envelope>(
                    (e) =>
                    {
                        publishedEnvelopes.Add(e);
                        return Task.CompletedTask;
                    });


            _sut._broker = _brokerMock.Object;
            _sut._testThreadObjectContainer = objectContainerStub;
            _sut._bindingCaches = new BindingMessagesGenerator(_idGeneratorMock.Object);
            _sut._bindingCaches.StepDefinitionIdByMethodSignaturePatternCache = new ConcurrentDictionary<string, string>();
            _sut._enabled = true;

            // Act
            await _sut.OnEventAsync(new FeatureStartedEvent(featureContextMock.Object));

            // Assert
            _sut._startedFeatures.Count.Should().Be(1);
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Exactly(2));
            publishedEnvelopes.Count.Should().Be(2);
            publishedEnvelopes[0].Content().Should().BeOfType<Source>();
            publishedEnvelopes[1].Content().Should().BeOfType<GherkinDocument>();
        }

        // FeatureFinishedEvent delegates to the FeatureTracker and pulls Execution messages to the Messages collection
        [Fact]
        public async Task FeatureFinished_Should_GatherExecutionMessagestotheMessagesCollection()
        {
            // Arrange
            var featureTrackerMock = new Mock<IFeatureTracker>();
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");
            var fakeMessages = new List<Envelope>
            {
                Envelope.Create(new TestCaseStarted(0, "id", "testCaseId", "", new Timestamp(1, 0)))
            };

            featureTrackerMock.Setup(ft => ft.RuntimeGeneratedMessages).Returns(fakeMessages);
            featureTrackerMock.Setup(ft => ft.Enabled).Returns(true);
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);
            var featureFinishedEvent = new FeatureFinishedEvent(featureContextMock.Object);

            _sut._startedFeatures.TryAdd("ABCDE", featureTrackerMock.Object);
            _sut._enabled = true;

            // Act
            await _sut.OnEventAsync(featureFinishedEvent);

            // Assert
            _sut._messages.Should().Contain(fakeMessages);
        }

        // ScenarioStartedEvent handles not finding a matching FeatureTracker
        [Fact]
        public async Task ScenarioStarted_Should_HandleNoMatchingFeatureTracker()
        {
            // Arrange
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");

            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);

            var tlMock = new Mock<ITraceListener>();
            var containerStub = new ObjectContainer();
            containerStub.RegisterInstanceAs<ITraceListener>(tlMock.Object);

            var scenarioContextMock = new Mock<IScenarioContext>();
            var scenarioStartedEvent = new ScenarioStartedEvent(featureContextMock.Object, scenarioContextMock.Object);
            _sut._testThreadObjectContainer = containerStub;
            _sut._enabled = true;

            // Act 
            var act = async () => await _sut.OnEventAsync(scenarioStartedEvent);

            // Assert
            await act.Should().ThrowAsync<ApplicationException>();
            tlMock.Verify(tl => tl.WriteTestOutput(It.IsAny<string>()));

        }

        // ScenarioStartedEvent forwards event to the FeatureTracker
        [Theory]
        [InlineData(typeof(ScenarioStartedEvent))]
        [InlineData(typeof(ScenarioFinishedEvent))]
        [InlineData(typeof(StepStartedEvent))]
        [InlineData(typeof(StepFinishedEvent))]
        public async Task ScenarioEvents_Should_DelegateToTheFeatureTracker(Type eventType)
        {
            // Arrange
            var featureTrackerMock = new Mock<IFeatureTracker>();
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");

            featureTrackerMock.Setup(ft => ft.Enabled).Returns(true);
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);

            var tlMock = new Mock<ITraceListener>();
            var containerStub = new ObjectContainer();
            containerStub.RegisterInstanceAs<ITraceListener>(tlMock.Object);

            var scenarioContextMock = new Mock<IScenarioContext>();
            var stepContextMock = new Mock<IScenarioStepContext>();

            var evnt = eventType switch
            {
                Type t when t == typeof(ScenarioStartedEvent) => (IExecutionEvent)Activator.CreateInstance(eventType, featureContextMock.Object, scenarioContextMock.Object),
                Type t when t == typeof(ScenarioFinishedEvent) => (IExecutionEvent)Activator.CreateInstance(eventType, featureContextMock.Object, scenarioContextMock.Object),
                Type t when t == typeof(StepStartedEvent) => (IExecutionEvent)Activator.CreateInstance(eventType, featureContextMock.Object, scenarioContextMock.Object, stepContextMock.Object),
                Type t when t == typeof(StepFinishedEvent) => (IExecutionEvent)Activator.CreateInstance(eventType, featureContextMock.Object, scenarioContextMock.Object, stepContextMock.Object),
                _ => throw new NotImplementedException()
            };

            _sut._startedFeatures.TryAdd("ABCDE", featureTrackerMock.Object);
            _sut._testThreadObjectContainer = containerStub;
            _sut._enabled = true;

            // Act 
            await _sut.OnEventAsync(evnt);

            // Assert
            tlMock.Verify(tl => tl.WriteTestOutput(It.IsAny<string>()), Times.Never);

            switch (eventType)
            {
                case Type t1 when t1 == typeof(ScenarioStartedEvent):
                    featureTrackerMock.Verify(ftm => ftm.ProcessEvent(It.Is<ScenarioStartedEvent>(e => e == evnt)));
                    break;
                case Type t1 when t1 == typeof(ScenarioFinishedEvent):
                    featureTrackerMock.Verify(ftm => ftm.ProcessEvent(It.Is<ScenarioFinishedEvent>(e => e == evnt)));
                    break;
                case Type t1 when t1 == typeof(StepStartedEvent):
                    featureTrackerMock.Verify(ftm => ftm.ProcessEvent(It.Is<StepStartedEvent>(e => e == evnt)));
                    break;
                case Type t1 when t1 == typeof(StepFinishedEvent):
                    featureTrackerMock.Verify(ftm => ftm.ProcessEvent(It.Is<StepFinishedEvent>(e => e == evnt)));
                    break;
                default:
                    break;
            }
        }

        private class HookBindingTest_CucumberMessageFactoryInnerStub : CucumberMessageFactoryInner
        {
            internal override TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
            {
                return new TestRunStarted(new Timestamp(1, 0), "");
            }
            internal override Meta ToMeta(IObjectContainer container)
            {
                return new Meta("", new Product("", ""), new Product("", ""), new Product("", ""), new Product("", ""), null);
            }

            internal override Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator)
            {
                return new Hook("HookTypeName.HookMethodName()", "Sample Hook", new SourceReference("", new JavaMethod("HookTypeName", "HookMethodName", []), null, new Location(1, 0)), "", Io.Cucumber.Messages.Types.HookType.AFTER_TEST_CASE);
            }

            internal override string CanonicalizeHookBinding(IHookBinding hookBinding)
            {
                return "HookTypeName.HookMethodName()";
            }
        }
        // HookBindingStartedEvent for non-Scenario related hooks: causes a new HookTracker to be created and added to the hookTrackers collection and a HookStarted message to be added
        [Theory]
        [InlineData(Reqnroll.Bindings.HookType.BeforeTestRun)]
        [InlineData(Reqnroll.Bindings.HookType.BeforeFeature)]
        [InlineData(Reqnroll.Bindings.HookType.AfterFeature)]
        [InlineData(Reqnroll.Bindings.HookType.AfterTestRun)]
        public async Task HookBindingStartedEvent_ForNonScenarioHooks_Should_CreateAHookTracker(Reqnroll.Bindings.HookType hookType)
        {
            // Arrange
            // Hack: Re-using code from a prior test to invoke the PublisherStartup and get the sut set-up for this test.
            var objectContainerStub = new ObjectContainer();
            var beforeTestRunArgs = new RuntimePluginBeforeTestRunEventArgs(objectContainerStub);
            // nb. This approach required as Moq can't mock types from Io.Cucumber.Messages.Types (as they're not visibleTo and sealed)
            CucumberMessageFactory.inner = new HookBindingTest_CucumberMessageFactoryInnerStub();

            _brokerMock.Setup(b => b.Enabled).Returns(true);
            objectContainerStub.RegisterInstanceAs<ICucumberMessageBroker>(_brokerMock.Object);
            objectContainerStub.RegisterInstanceAs<IIdGenerator>(_idGeneratorMock.Object);
            objectContainerStub.RegisterInstanceAs<IClock>(_clockMock.Object);
            objectContainerStub.RegisterInstanceAs<IBindingRegistry>(_bindingRegistryMock.Object);
            _idGeneratorMock.Setup(ig => ig.GetNewId()).Returns("x");
            _clockMock.Setup(c => c.GetNowDateAndTime()).Returns(DateTime.UtcNow);

            var hookBindingMock = new Mock<IHookBinding>();
            hookBindingMock.Setup(hb => hb.HookType).Returns( hookType);
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");

            _sut._brokerFactory = new Lazy<ICucumberMessageBroker>(() => _brokerMock.Object);
            _sut._testThreadObjectContainer = objectContainerStub;

            _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns(new List<IStepDefinitionBinding>());
            _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns(new List<IStepArgumentTransformationBinding>());
            _bindingRegistryMock.Setup(br => br.GetHooks()).Returns(new List<IHookBinding>([hookBindingMock.Object]));

            _sut.PublisherStartup(null, beforeTestRunArgs);
            var cmMock = new Mock<IContextManager>();
            var featureContextStub = new FeatureContext(null, featureInfoStub, ConfigurationLoader.GetDefault());
            cmMock.Setup(cm => cm.FeatureContext).Returns(featureContextStub);

            // Act
            var hookBindingStarted = new HookBindingStartedEvent(hookBindingMock.Object, cmMock.Object);
            await _sut.OnEventAsync(hookBindingStarted);

            // Assert
            _sut._testRunHookTrackers.Should().HaveCount(1);

        }

        // HookBindingFinishedEvent for non-Scenario related hooks: causes a HookFinished message to be added
        [Theory]
        [InlineData(Reqnroll.Bindings.HookType.BeforeTestRun)]
        [InlineData(Reqnroll.Bindings.HookType.BeforeFeature)]
        [InlineData(Reqnroll.Bindings.HookType.AfterFeature)]
        [InlineData(Reqnroll.Bindings.HookType.AfterTestRun)]
        public async Task HookBindingFinishedEvent_ForNonScenarioHooks_Should_EmitAHookFinishedMessage(Reqnroll.Bindings.HookType hookType)
        {
            // Arrange
            // Hack: Re-using code from a prior test to invoke the PublisherStartup and get the sut set-up for this test.
            var objectContainerStub = new ObjectContainer();
            CucumberMessageFactory.inner = new HookBindingTest_CucumberMessageFactoryInnerStub();

            _brokerMock.Setup(b => b.Enabled).Returns(true);
            IList<Envelope> publishedEnvelopes = new List<Envelope>();
            _brokerMock.Setup(b => b.PublishAsync(It.IsAny<Envelope>())).Returns<Envelope>(
                    (e) =>
                    {
                        publishedEnvelopes.Add(e);
                        return Task.CompletedTask;
                    });

            objectContainerStub.RegisterInstanceAs<ICucumberMessageBroker>(_brokerMock.Object);
            objectContainerStub.RegisterInstanceAs<IIdGenerator>(_idGeneratorMock.Object);
            objectContainerStub.RegisterInstanceAs<IClock>(_clockMock.Object);
            objectContainerStub.RegisterInstanceAs<IBindingRegistry>(_bindingRegistryMock.Object);
            _idGeneratorMock.Setup(ig => ig.GetNewId()).Returns("x");
            _clockMock.Setup(c => c.GetNowDateAndTime()).Returns(DateTime.UtcNow);

            var hookBindingMock = new Mock<IHookBinding>();
            hookBindingMock.Setup(hb => hb.HookType).Returns(hookType);
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");

            _sut._brokerFactory = new Lazy<ICucumberMessageBroker>(() => _brokerMock.Object);
            _sut._testThreadObjectContainer = objectContainerStub;
            _sut._enabled = true;

            _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns(new List<IStepDefinitionBinding>());
            _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns(new List<IStepArgumentTransformationBinding>());
            _bindingRegistryMock.Setup(br => br.GetHooks()).Returns(new List<IHookBinding>([hookBindingMock.Object]));

            var cmMock = new Mock<IContextManager>();
            var featureContextStub = new FeatureContext(null, featureInfoStub, ConfigurationLoader.GetDefault());
            cmMock.Setup(cm => cm.FeatureContext).Returns(featureContextStub);
            var dur = new TimeSpan(2);

            var hookTracker = new TestRunHookTracker("HookTypeName.HookMethodName()", "1",  DateTime.UtcNow, "0");
            _sut._testRunHookTrackers.TryAdd("HookTypeName.HookMethodName()", hookTracker);
            var hookBindingFinished = new HookBindingFinishedEvent(hookBindingMock.Object, dur, cmMock.Object);
 
            // Act
            await _sut.OnEventAsync(hookBindingFinished);

            // Assert
            _sut._messages.Should().HaveCount(1);
            _sut._messages[0].Content().Should().BeOfType<TestRunHookFinished>();
        }


        // HookBinddingStartedEvent for Scenario-related hooks: forwards to the FeatureTracker
        // HookBindingFinishedEvent for Scenario-related hooks: forwards to the FeatureTracker
        [Theory]
        [InlineData(typeof(HookBindingStartedEvent), Reqnroll.Bindings.HookType.BeforeScenario)]
        [InlineData(typeof(HookBindingStartedEvent), Reqnroll.Bindings.HookType.AfterScenario)]
        [InlineData(typeof(HookBindingStartedEvent), Reqnroll.Bindings.HookType.BeforeScenarioBlock)]
        [InlineData(typeof(HookBindingStartedEvent), Reqnroll.Bindings.HookType.AfterScenarioBlock)]
        [InlineData(typeof(HookBindingStartedEvent), Reqnroll.Bindings.HookType.BeforeStep)]
        [InlineData(typeof(HookBindingStartedEvent), Reqnroll.Bindings.HookType.AfterStep)]
        [InlineData(typeof(HookBindingFinishedEvent), Reqnroll.Bindings.HookType.BeforeScenario)]
        [InlineData(typeof(HookBindingFinishedEvent), Reqnroll.Bindings.HookType.AfterScenario)]
        [InlineData(typeof(HookBindingFinishedEvent), Reqnroll.Bindings.HookType.BeforeScenarioBlock)]
        [InlineData(typeof(HookBindingFinishedEvent), Reqnroll.Bindings.HookType.AfterScenarioBlock)]
        [InlineData(typeof(HookBindingFinishedEvent), Reqnroll.Bindings.HookType.BeforeStep)]
        [InlineData(typeof(HookBindingFinishedEvent), Reqnroll.Bindings.HookType.AfterStep)]

        public async Task HookBindingEvents_WithScenarioOrInnerHookTypes_Should_ForwardToFeatureTracker(Type eventType, Reqnroll.Bindings.HookType hookType)
        {
            // Arrange
            var featureTrackerMock = new Mock<IFeatureTracker>();
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");

            featureTrackerMock.Setup(ft => ft.Enabled).Returns(true);
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);

            var hookBindingMock = new Mock<IHookBinding>();
            hookBindingMock.Setup(hb => hb.HookType).Returns(hookType);
            var cmMock = new Mock<IContextManager>();
            var featureContextStub = new FeatureContext(null, featureInfoStub, ConfigurationLoader.GetDefault());
            cmMock.Setup(cm => cm.FeatureContext).Returns(featureContextStub);
            var dur = new TimeSpan();

            var evnt = eventType switch
            {
                Type t when t == typeof(HookBindingStartedEvent) => (IExecutionEvent)Activator.CreateInstance(eventType, hookBindingMock.Object, cmMock.Object),
                Type t when t == typeof(HookBindingFinishedEvent) => (IExecutionEvent)Activator.CreateInstance(eventType, hookBindingMock.Object, dur, cmMock.Object, null),
                _ => throw new NotImplementedException()
            };

            _sut._startedFeatures.TryAdd("ABCDE", featureTrackerMock.Object);
            _sut._enabled = true;

            // Act 
            await _sut.OnEventAsync(evnt);

            // Assert
            switch (eventType)
            {
                case Type t1 when t1 == typeof(HookBindingStartedEvent):
                    featureTrackerMock.Verify(ftm => ftm.ProcessEvent(It.Is<HookBindingStartedEvent>(e => e == evnt)));
                    break;
                case Type t1 when t1 == typeof(HookBindingFinishedEvent):
                    featureTrackerMock.Verify(ftm => ftm.ProcessEvent(It.Is<HookBindingFinishedEvent>(e => e == evnt)));
                    break;
                default:
                    break;
            }
        }

        // AttachmentAddedEvent for Scenario-related attachments: forwards to the FeatureTracker
        // OutputAddedEvent for Scenario-related output: forwards to the FeatureTracker
        [Theory]
        [InlineData(typeof(AttachmentAddedEvent))]
        [InlineData(typeof(OutputAddedEvent))]
        public async Task AttachmentAndOutputEvents_ForScenarioRelatedContent_Should_ForwardToFeatureTracker(Type eventType)
        {
            // Arrange
            var featureTrackerMock = new Mock<IFeatureTracker>();
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");
            featureTrackerMock.Setup(ft => ft.Enabled).Returns(true);
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);
            var scenarioInfoStub = new ScenarioInfo("Scenario FGHIJK", "", [], new OrderedDictionary());

            var evnt = (IExecutionEvent)Activator.CreateInstance(eventType, "", featureInfoStub, scenarioInfoStub);

            _sut._startedFeatures.TryAdd("ABCDE", featureTrackerMock.Object);
            _sut._enabled = true;

            // Act 
            await _sut.OnEventAsync(evnt);

            // Assert
            switch (eventType)
            {
                case Type t1 when t1 == typeof(AttachmentAddedEvent):
                    featureTrackerMock.Verify(ftm => ftm.ProcessEvent(It.Is<AttachmentAddedEvent>(e => e == evnt)));
                    break;
                case Type t1 when t1 == typeof(OutputAddedEvent):
                    featureTrackerMock.Verify(ftm => ftm.ProcessEvent(It.Is<OutputAddedEvent>(e => e == evnt)));
                    break;
                default:
                    break;
            }
        }

        private class AttachmentMessageFactory : CucumberMessageFactoryInner
        {
            internal override Attachment ToAttachment(AttachmentAddedEventWrapper tracker)
            {
                return new Attachment("fake body", AttachmentContentEncoding.BASE64, tracker.AttachmentAddedEvent.FilePath, "dummy", new Source("", "source", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN), tracker.TestCaseStartedId, "", "", tracker.TestRunStartedId);
            }
            internal override Attachment ToAttachment(OutputAddedEventWrapper tracker)
            {
                return new Attachment(tracker.OutputAddedEvent.Text, AttachmentContentEncoding.IDENTITY, "", "dummy", new Source("", "source", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN), tracker.TestCaseStartedId, "", "", tracker.TestRunStartedId);
            }
        }

        // AttachmentAddedEvent - otherwise causes an Attachment message to be added to the messages collection
        // OutputAddedEvent - otherwise causes an Attachment message to be added to the messages collection
        [Theory]
        [InlineData(typeof(AttachmentAddedEvent))]
        [InlineData(typeof(OutputAddedEvent))]
        public async Task AttachmentAndOutputEvents_ForTestRunRelatedContent_Should_DirectlyPostMessagesTotheCollection(Type eventType)
        {
            // Arrange

            var evnt = (IExecutionEvent)Activator.CreateInstance(eventType, "attachment-path", null, null);

            _sut._enabled = true;
            CucumberMessageFactory.inner = new AttachmentMessageFactory();
            // Act 
            await _sut.OnEventAsync(evnt);

            // Assert
            CucumberMessageFactory.inner = new CucumberMessageFactoryInner();

            _sut._messages.Should().HaveCount(1);
            _sut._messages[0].Content().Should().BeOfType<Attachment>();
        }


    }
}
