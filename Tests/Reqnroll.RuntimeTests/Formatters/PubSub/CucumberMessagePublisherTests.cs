using FluentAssertions;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Events;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.Time;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.PubSub
{
    public class CucumberMessagePublisherTests
    {
        private readonly Mock<IObjectContainer> _objectContainerMock;
        private readonly Mock<ICucumberMessageBroker> _brokerMock;
        private readonly Mock<IBindingRegistry> _bindingRegistryMock;
        private readonly Mock<IIdGenerator> _idGeneratorMock;
        private readonly Mock<IClock> _clockMock;
        private readonly Mock<ITestThreadExecutionEventPublisher> _eventPublisherMock;
        private readonly Mock<IBindingMessagesGenerator> _bindingMessagesGeneratorMock;
        private readonly Mock<IFormatterLog> _formatterLoggerMock;
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
            _bindingMessagesGeneratorMock = new Mock<IBindingMessagesGenerator>();
            _formatterLoggerMock = new Mock<IFormatterLog>();

            _runtimePluginEvents = new RuntimePluginEvents();
            _runtimePluginParameters = new RuntimePluginParameters();
            _unitTestProviderConfiguration = new UnitTestProviderConfiguration();

            _sut = new CucumberMessagePublisher(_bindingMessagesGeneratorMock.Object, _formatterLoggerMock.Object);
        }
        private ObjectContainer CreateObjectContainerWithBroker(bool brokerEnabled = true)
        {
            var container = new ObjectContainer();
            _brokerMock.Setup(b => b.Enabled).Returns(brokerEnabled);
            container.RegisterInstanceAs(_brokerMock.Object);
            container.RegisterTypeAs<IFormatterLog>(typeof(FormatterLog));
            return container;
        }

        private void SetupCommonMocks(ObjectContainer container)
        {
            _clockMock.Setup(c => c.GetNowDateAndTime()).Returns(DateTime.UtcNow);

            container.RegisterInstanceAs(_idGeneratorMock.Object);
            container.RegisterInstanceAs(_clockMock.Object);
            container.RegisterInstanceAs(_bindingRegistryMock.Object);
            _bindingRegistryMock.SetupGet<bool>(br => br.Ready).Returns(true);
        }
        
        [Fact]
        public void Initialize_Should_Setup_TestThread_Dependencies()
        {
            // Arrange
            var objectContainerStub = new ObjectContainer();
            objectContainerStub.RegisterInstanceAs(_eventPublisherMock.Object);
            objectContainerStub.RegisterInstanceAs(_brokerMock.Object);

            // Act
            _sut.Initialize(_runtimePluginEvents, _runtimePluginParameters, _unitTestProviderConfiguration);
            _runtimePluginEvents.RaiseCustomizeTestThreadDependencies(objectContainerStub);

            // Assert
            _eventPublisherMock.Verify(e => e.AddListener(_sut), Times.Once);
        }

        // BrokerReady/BeforeTestRun causes no side-effects when broker is disabled
        [Fact]
        public async Task PublisherStartup_Should_Not_Perform_Actions_When_Broker_Is_Disabled()
        {
            // Arrange
            var objectContainerStub = CreateObjectContainerWithBroker(false);
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _sut._broker = _brokerMock.Object;

            // Act
            await _sut.PublisherStartup(new TestRunStartedEvent());

            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Never);
        }

        class PublisherStartup_FactoryStub : CucumberMessageFactory
        {
            public override TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
            {
                return new TestRunStarted(new Timestamp(1, 0), "");
            }
            public override Meta ToMeta(IObjectContainer container)
            {
                return new Meta("", new Product("", ""), new Product("", ""), new Product("", ""), new Product("", ""), null);
            }

        }

        // BrokerReady populates binding caches and publishes startup messages
        [Fact]
        public async Task PublisherStartup_Should_Populate_BindingCaches_And_Publish_Startup_Messages()
        {
            // Arrange
            var objectContainerStub = CreateObjectContainerWithBroker(true);
            SetupCommonMocks(objectContainerStub);
            var beforeTestRunArgs = new RuntimePluginBeforeTestRunEventArgs(objectContainerStub);
            // nb. This approach required as Moq can't mock types from Io.Cucumber.Messages.Types (as they're not visibleTo as well as sealed)
            var msgFactory = new PublisherStartup_FactoryStub();
            objectContainerStub.RegisterInstanceAs<ICucumberMessageFactory>(msgFactory);
            var bmg = new BindingMessagesGenerator(_idGeneratorMock.Object, msgFactory, _bindingRegistryMock.Object);
            objectContainerStub.RegisterInstanceAs<IBindingMessagesGenerator>(bmg);
            _sut._bindingCaches = bmg;
            _sut._broker = _brokerMock.Object;
            _sut._globalObjectContainer = objectContainerStub;

            _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns(new List<IStepDefinitionBinding>());
            _bindingRegistryMock.Setup(br => br.GetHooks()).Returns(new List<IHookBinding>());
            _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns(new List<IStepArgumentTransformationBinding>());
            // Act
            bmg.OnBindingRegistryReady(null, null);
            
            await _sut.PublisherStartup(new TestRunStartedEvent());

            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.AtLeast(2)); // TestRunStarted and Meta messages
            _bindingRegistryMock.Verify(br => br.GetStepDefinitions(), Times.Exactly(2));
            _bindingRegistryMock.Verify(br => br.GetHooks(), Times.Once);
            _bindingRegistryMock.Verify(br => br.GetStepTransformations(), Times.Once);
        }

        // PublisherTestRunComplete/AfterTestRun causes no side-effects when Publisher is disabled
        [Fact]
        public async Task PublisherTestRunComplete_Should_Not_Perform_Actions_When_Broker_Is_Disabled()
        {
            // Arrange
            var objectContainerStub = CreateObjectContainerWithBroker(false);
            SetupCommonMocks(objectContainerStub);
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _sut._broker = _brokerMock.Object;

            // Act
            await _sut.PublisherTestRunCompleteAsync(new TestRunFinishedEvent());


            // Assert
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Never);
        }

        // PublisherTestRunComplete calculates test run status as Success when all started features are complete and succeeded
        [Fact]
        public async Task PublisherTestRunComplete_Should_CalculateStatusWhenAllFeaturesSucceed()
        {
            // Arrange
            var objectContainerStub = CreateObjectContainerWithBroker(true);
            SetupCommonMocks(objectContainerStub);
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _idGeneratorMock.Setup(g => g.GetNewId()).Returns("1");

            var feature1TrackerMock = new Mock<IFeatureExecutionTracker>();
            feature1TrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(true);
            feature1TrackerMock.Setup(f => f.Enabled).Returns(true);
            var feature2TrackerMock = new Mock<IFeatureExecutionTracker>();
            feature2TrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(true);
            feature2TrackerMock.Setup(f => f.Enabled).Returns(true);



            var f1 = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "feature1", "");
            var f2 = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "feature2", "");
            var f1Context = new Mock<IFeatureContext>();
            f1Context.Setup(x => x.FeatureInfo).Returns(f1);
            var f2Context = new Mock<IFeatureContext>();
            f2Context.Setup(x => x.FeatureInfo).Returns(f2);

            _sut._broker = _brokerMock.Object;
            _sut._startedFeatures.TryAdd(f1, feature1TrackerMock.Object);
            _sut._startedFeatures.TryAdd(f2, feature2TrackerMock.Object);
            _sut._enabled = true;
            _sut._messageFactory = new CucumberMessageFactory();
            _sut._clock = _clockMock.Object;

            // Act
            await _sut.FeatureFinishedEventHandler(new FeatureFinishedEvent(f1Context.Object));
            _sut._allFeaturesPassed.Should().BeTrue();
            await _sut.FeatureFinishedEventHandler(new FeatureFinishedEvent(f2Context.Object));

            // Assert
            _sut._allFeaturesPassed.Should().BeTrue();
        }

        // PublisherTestRunComplete calculates test run status as Failed when any started feature is are Failed
        [Fact]
        public async Task PublisherFeatureFinished_Should_CalculateStatusWhenAFeatureFails()
        {
            // Arrange
            var objectContainerStub = CreateObjectContainerWithBroker(true);
            SetupCommonMocks(objectContainerStub);
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _idGeneratorMock.Setup(g => g.GetNewId()).Returns("1");

            var feature1TrackerMock = new Mock<IFeatureExecutionTracker>();
            feature1TrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(true);
            feature1TrackerMock.Setup(f => f.Enabled).Returns(true);
            var feature2TrackerMock = new Mock<IFeatureExecutionTracker>();
            feature2TrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(false);
            feature2TrackerMock.Setup(f => f.Enabled).Returns(true);



            var f1 = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "feature1", "");
            var f2 = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "feature2", "");
            var f1Context = new Mock<IFeatureContext>();
            f1Context.Setup(x => x.FeatureInfo).Returns(f1);
            var f2Context = new Mock<IFeatureContext>();
            f2Context.Setup(x => x.FeatureInfo).Returns(f2);

            _sut._broker = _brokerMock.Object;
            _sut._startedFeatures.TryAdd(f1, feature1TrackerMock.Object);
            _sut._startedFeatures.TryAdd(f2, feature2TrackerMock.Object);
            _sut._enabled = true;
            _sut._messageFactory = new CucumberMessageFactory();
            _sut._clock = _clockMock.Object;

            // Act
            await _sut.FeatureFinishedEventHandler(new FeatureFinishedEvent(f1Context.Object));
            _sut._allFeaturesPassed.Should().BeTrue();
            await _sut.FeatureFinishedEventHandler(new FeatureFinishedEvent(f2Context.Object));

            // Assert
            _sut._allFeaturesPassed.Should().BeFalse();
        }

        // PublisherTestRunComplete publishes TestCase messages for Pickles, then Execution messages, followed by the TestRunFinished message
        [Fact]
        public async Task PublisherTestRunComplete_Should_PublishTestCase_and_Exec_Messages()
        {
            // Arrange
            var objectContainerStub = CreateObjectContainerWithBroker(true);
            SetupCommonMocks(objectContainerStub);
            objectContainerStub.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            _idGeneratorMock.Setup(g => g.GetNewId()).Returns("1");

            var featureTrackerMock = new Mock<IFeatureExecutionTracker>();
            featureTrackerMock.Setup(f => f.FeatureExecutionSuccess).Returns(true);
            featureTrackerMock.Setup(f => f.Enabled).Returns(true);


            var f1 = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "feature1", "");
            var f1Context = new Mock<IFeatureContext>();
            f1Context.Setup(x => x.FeatureInfo).Returns(f1);

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
            _sut._startedFeatures.TryAdd(f1, featureTrackerMock.Object);
            _sut._enabled = true;
            _sut._messages = messages;
            _sut._messageFactory = new CucumberMessageFactory();
            _sut._clock = _clockMock.Object;

            // Act
            await _sut.FeatureFinishedEventHandler(new FeatureFinishedEvent(f1Context.Object));
            await _sut.PublisherTestRunCompleteAsync(new TestRunFinishedEvent());

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
            objectContainerStub.RegisterInstanceAs(new Mock<ITraceListener>().Object);
            var featureContextMock = new Mock<IFeatureContext>();

            _sut._enabled = false;
            _sut._broker = _brokerMock.Object;
            _sut._globalObjectContainer = objectContainerStub;
            _sut._startupCompleted = true;

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
            var objectContainerStub = CreateObjectContainerWithBroker(true);
            SetupCommonMocks(objectContainerStub);
            objectContainerStub.RegisterInstanceAs(new Mock<ITraceListener>().Object);
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDEF", null);
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);

            var existingFeatureTrackerMock = new Mock<IFeatureExecutionTracker>();
            _sut._startedFeatures.TryAdd(featureInfoStub, existingFeatureTrackerMock.Object);
            _sut._broker = _brokerMock.Object;
            _sut._globalObjectContainer = objectContainerStub;
            _sut._enabled = true;
            _sut._startupCompleted = true;

            // Act
            await _sut.OnEventAsync(new FeatureStartedEvent(featureContextMock.Object));

            // Assert
            _sut._startedFeatures.Count.Should().Be(1);
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Never);

        }

        // FeatureStartedEvent causes a FeatureExecutionTracker to be instantiated and Static Messages to be published to the Broker
        [Fact]
        public async Task FeatureStartedEvent_Should_InstantiateAFeatureTrackerAndPublishStaticMessages()
        {
            // Arrange
            var objectContainerStub = CreateObjectContainerWithBroker(true);
            SetupCommonMocks(objectContainerStub);
            objectContainerStub.RegisterInstanceAs(new Mock<ITraceListener>().Object);

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
            var featureMessagesStub = new FeatureLevelCucumberMessages(sourceFunc, gherkinDocFunc, picklesFunc);
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
            var messageFactory = new CucumberMessageFactory();
            _sut._messageFactory = messageFactory;
            _sut._broker = _brokerMock.Object;
            _sut._globalObjectContainer = objectContainerStub;
            var bmg =  new BindingMessagesGenerator(_idGeneratorMock.Object, messageFactory, _bindingRegistryMock.Object);
            _sut._bindingCaches = bmg;
            bmg.OnBindingRegistryReady(null, null);

            //_sut._bindingCaches.StepDefinitionIdByBinding.Clear();
            _sut._enabled = true;
            _sut._startupCompleted = true;

            // Act
            await _sut.OnEventAsync(new FeatureStartedEvent(featureContextMock.Object));

            // Assert
            _sut._startedFeatures.Count.Should().Be(1);
            _brokerMock.Verify(b => b.PublishAsync(It.IsAny<Envelope>()), Times.Exactly(2));
            publishedEnvelopes.Count.Should().Be(2);
            publishedEnvelopes[0].Content().Should().BeOfType<Source>();
            publishedEnvelopes[1].Content().Should().BeOfType<GherkinDocument>();
        }

        // FeatureFinishedEvent delegates to the FeatureExecutionTracker and pulls Execution messages to the Messages collection
        [Fact]
        public async Task FeatureFinished_Should_GatherExecutionMessagestotheMessagesCollection()
        {
            // Arrange
            var featureTrackerMock = new Mock<IFeatureExecutionTracker>();
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
            _sut._messageFactory = new CucumberMessageFactory();

            _sut._startedFeatures.TryAdd(featureInfoStub, featureTrackerMock.Object);
            _sut._enabled = true;

            // Act
            await _sut.OnEventAsync(featureFinishedEvent);

            // Assert
            _sut._messages.Should().Contain(fakeMessages);
        }


        // ScenarioStartedEvent forwards event to the FeatureExecutionTracker
        [Theory]
        [InlineData(typeof(ScenarioStartedEvent))]
        [InlineData(typeof(ScenarioFinishedEvent))]
        [InlineData(typeof(StepStartedEvent))]
        [InlineData(typeof(StepFinishedEvent))]
        public async Task ScenarioEvents_Should_DelegateToTheFeatureTracker(Type eventType)
        {
            // Arrange
            var featureTrackerMock = new Mock<IFeatureExecutionTracker>();
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");

            featureTrackerMock.Setup(ft => ft.Enabled).Returns(true);
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);

            var tlMock = new Mock<ITraceListener>();
            var containerStub = new ObjectContainer();
            containerStub.RegisterInstanceAs(tlMock.Object);

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

            _sut._messageFactory = new CucumberMessageFactory();
            _sut._startedFeatures.TryAdd(featureInfoStub, featureTrackerMock.Object);
            _sut._globalObjectContainer = containerStub;
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

        private class HookBindingTest_CucumberMessageFactoryStub : CucumberMessageFactory
        {
            public override TestRunStarted ToTestRunStarted(DateTime timestamp, string id)
            {
                return new TestRunStarted(new Timestamp(1, 0), "");
            }
            public override Meta ToMeta(IObjectContainer container)
            {
                return new Meta("", new Product("", ""), new Product("", ""), new Product("", ""), new Product("", ""), null);
            }

            public override Hook ToHook(IHookBinding hookBinding, IIdGenerator iDGenerator)
            {
                return new Hook("HookTypeName.HookMethodName()", "Sample Hook", new SourceReference("", new JavaMethod("HookTypeName", "HookMethodName", []), null, new Location(1, 0)), "", Io.Cucumber.Messages.Types.HookType.AFTER_TEST_CASE);
            }

            public override string CanonicalizeHookBinding(IHookBinding hookBinding)
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
            var objectContainerStub = CreateObjectContainerWithBroker(true);
            SetupCommonMocks(objectContainerStub);
            var msgFactory = new HookBindingTest_CucumberMessageFactoryStub();
            objectContainerStub.RegisterInstanceAs<ICucumberMessageFactory>(msgFactory);

            var traceListen = new Mock<ITraceListener>();
            objectContainerStub.RegisterInstanceAs(traceListen.Object);
            _idGeneratorMock.Setup(ig => ig.GetNewId()).Returns("x");

            var hookBindingMock = new Mock<IHookBinding>();
            hookBindingMock.Setup(hb => hb.HookType).Returns(hookType);
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");

            _sut._globalObjectContainer = objectContainerStub;
            _sut._broker = _brokerMock.Object;

            _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns(new List<IStepDefinitionBinding>());
            _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns(new List<IStepArgumentTransformationBinding>());
            _bindingRegistryMock.Setup(br => br.GetHooks()).Returns(new List<IHookBinding>([hookBindingMock.Object]));
            var bmg = new BindingMessagesGenerator(_idGeneratorMock.Object, msgFactory, _bindingRegistryMock.Object);
            objectContainerStub.RegisterInstanceAs<IBindingMessagesGenerator>(bmg);
            _sut._bindingCaches = bmg;
            bmg.OnBindingRegistryReady(null, null);

            await _sut.PublisherStartup(new TestRunStartedEvent());
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
            // Hack: Re-using code from a prior test to invoke the BrokerReady and get the sut set-up for this test.
            var objectContainerStub = CreateObjectContainerWithBroker(true);
            SetupCommonMocks(objectContainerStub);
            var messageFactory = new HookBindingTest_CucumberMessageFactoryStub();

            IList<Envelope> publishedEnvelopes = new List<Envelope>();
            _brokerMock.Setup(b => b.PublishAsync(It.IsAny<Envelope>())).Returns<Envelope>(
                    (e) =>
                    {
                        publishedEnvelopes.Add(e);
                        return Task.CompletedTask;
                    });

            _idGeneratorMock.Setup(ig => ig.GetNewId()).Returns("x");

            var hookBindingMock = new Mock<IHookBinding>();
            hookBindingMock.Setup(hb => hb.HookType).Returns(hookType);
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");

            _sut._globalObjectContainer = objectContainerStub;
            _sut._enabled = true;
            _sut._messageFactory = messageFactory;

            _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns(new List<IStepDefinitionBinding>());
            _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns(new List<IStepArgumentTransformationBinding>());
            _bindingRegistryMock.Setup(br => br.GetHooks()).Returns(new List<IHookBinding>([hookBindingMock.Object]));

            var cmMock = new Mock<IContextManager>();
            var featureContextStub = new FeatureContext(null, featureInfoStub, ConfigurationLoader.GetDefault());
            cmMock.Setup(cm => cm.FeatureContext).Returns(featureContextStub);
            var dur = new TimeSpan(2);

            var hookTracker = new TestRunHookExecutionTracker("2", "1", "0", messageFactory);
            _sut._testRunHookTrackers.TryAdd(hookBindingMock.Object, hookTracker);
            var hookBindingFinished = new HookBindingFinishedEvent(hookBindingMock.Object, dur, cmMock.Object);

            // Act
            await _sut.OnEventAsync(hookBindingFinished);

            // Assert
            _sut._messages.Should().HaveCount(1);
            _sut._messages[0].Content().Should().BeOfType<TestRunHookFinished>();
        }


        // HookBinddingStartedEvent for Scenario-related hooks: forwards to the FeatureExecutionTracker
        // HookBindingFinishedEvent for Scenario-related hooks: forwards to the FeatureExecutionTracker
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
            var featureTrackerMock = new Mock<IFeatureExecutionTracker>();
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

            _sut._startedFeatures.TryAdd(featureInfoStub, featureTrackerMock.Object);
            _sut._enabled = true;
            _sut._messageFactory = new CucumberMessageFactory();

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

        // AttachmentAddedEvent for Scenario-related attachments: forwards to the FeatureExecutionTracker
        // OutputAddedEvent for Scenario-related output: forwards to the FeatureExecutionTracker
        [Theory]
        [InlineData(typeof(AttachmentAddedEvent))]
        [InlineData(typeof(OutputAddedEvent))]
        public async Task AttachmentAndOutputEvents_ForScenarioRelatedContent_Should_ForwardToFeatureTracker(Type eventType)
        {
            // Arrange
            var featureTrackerMock = new Mock<IFeatureExecutionTracker>();
            var featureContextMock = new Mock<IFeatureContext>();
            var featureInfoStub = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "ABCDE", "desc");
            featureTrackerMock.Setup(ft => ft.Enabled).Returns(true);
            featureContextMock.Setup(fc => fc.FeatureInfo).Returns(featureInfoStub);
            var scenarioInfoStub = new ScenarioInfo("Scenario FGHIJK", "", [], new OrderedDictionary());

            var evnt = (IExecutionEvent)Activator.CreateInstance(eventType, "", featureInfoStub, scenarioInfoStub);

            _sut._startedFeatures.TryAdd(featureInfoStub, featureTrackerMock.Object);
            _sut._enabled = true;
            _sut._messageFactory = new CucumberMessageFactory();

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

        private class AttachmentMessageFactory : CucumberMessageFactory
        {
            public override Attachment ToAttachment(AttachmentTracker tracker)
            {
                return new Attachment("fake body", AttachmentContentEncoding.BASE64, tracker.FilePath, "dummy", new Source("", "source", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN), tracker.TestCaseStartedId, "", "", tracker.TestRunStartedId);
            }
            public override Attachment ToAttachment(OutputMessageTracker tracker)
            {
                return new Attachment(tracker.Text, AttachmentContentEncoding.IDENTITY, "", "dummy", new Source("", "source", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN), tracker.TestCaseStartedId, "", "", tracker.TestRunStartedId);
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
            _sut._messageFactory = new AttachmentMessageFactory();
            // Act 
            await _sut.OnEventAsync(evnt);

            // Assert

            _sut._messages.Should().HaveCount(1);
            _sut._messages[0].Content().Should().BeOfType<Attachment>();
        }
    }
}
