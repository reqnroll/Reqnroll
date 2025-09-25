using Gherkin.CucumberMessages;
using Reqnroll.Analytics;
using Reqnroll.Analytics.AppInsights;
using Reqnroll.Analytics.UserId;
using Reqnroll.Bindings;
using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Bindings.Provider;
using Reqnroll.BindingSkeletons;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.ErrorHandling;
using Reqnroll.Events;
using Reqnroll.FileAccess;
using Reqnroll.Formatters;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.Html;
using Reqnroll.Formatters.Message;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.PlatformCompatibility;
using Reqnroll.Plugins;
using Reqnroll.TestFramework;
using Reqnroll.Time;
using Reqnroll.Tracing;
using Reqnroll.Utils;
using System;

namespace Reqnroll.Infrastructure
{
    //NOTE: Please update https://github.com/reqnroll/Reqnroll/wiki/Available-Containers-&-Registrations if you change registration defaults

    public class DefaultDependencyProvider : IDefaultDependencyProvider
    {
        public virtual void RegisterGlobalContainerDefaults(ObjectContainer container)
        {
            container.RegisterTypeAs<DefaultTestRunContext, ITestRunContext>();
            container.RegisterTypeAs<DefaultRuntimeConfigurationProvider, IRuntimeConfigurationProvider>();

            container.RegisterTypeAs<TestRunnerManager, ITestRunnerManager>();

            container.RegisterTypeAs<StepFormatter, IStepFormatter>();
            container.RegisterTypeAs<TestTracer, ITestTracer>();
            container.RegisterTypeAs<ColorOutputTheme, IColorOutputTheme>();
            container.RegisterTypeAs<ColorOutputHelper, IColorOutputHelper>();

            container.RegisterTypeAs<DefaultListener, ITraceListener>();
            container.RegisterTypeAs<TraceListenerQueue, ITraceListenerQueue>();

            container.RegisterTypeAs<ErrorProvider, IErrorProvider>();
            container.RegisterTypeAs<RuntimeBindingSourceProcessor, IRuntimeBindingSourceProcessor>();
            container.RegisterTypeAs<RuntimeBindingRegistryBuilder, IRuntimeBindingRegistryBuilder>();
            container.RegisterTypeAs<ReqnrollAttributesFilter, IReqnrollAttributesFilter>();
            container.RegisterTypeAs<BindingRegistry, IBindingRegistry>();
            container.RegisterTypeAs<BindingFactory, IBindingFactory>();
            container.RegisterTypeAs<CucumberExpressionStepDefinitionBindingBuilderFactory, ICucumberExpressionStepDefinitionBindingBuilderFactory>();
            container.RegisterTypeAs<CucumberExpressionDetector, ICucumberExpressionDetector>();
            container.RegisterTypeAs<StepDefinitionRegexCalculator, IStepDefinitionRegexCalculator>();
            container.RegisterTypeAs<MatchArgumentCalculator, IMatchArgumentCalculator>();
            container.RegisterTypeAs<BindingInvoker, IAsyncBindingInvoker>();
            container.RegisterTypeAs<BindingDelegateInvoker, IBindingDelegateInvoker>();
            container.RegisterTypeAs<TestObjectResolver, ITestObjectResolver>();
            container.RegisterTypeAs<BindingProviderService, IBindingProviderService>();

            container.RegisterTypeAs<StepDefinitionSkeletonProvider, IStepDefinitionSkeletonProvider>();
            container.RegisterTypeAs<DefaultSkeletonTemplateProvider, ISkeletonTemplateProvider>();
            container.RegisterTypeAs<StepTextAnalyzer, IStepTextAnalyzer>();

            PlatformHelper.RegisterPluginAssemblyLoader(container);
            container.RegisterTypeAs<RuntimePluginLoader, IRuntimePluginLoader>();
            container.RegisterTypeAs<RuntimePluginLocator, IRuntimePluginLocator>();
            container.RegisterTypeAs<RuntimePluginLocationMerger, IRuntimePluginLocationMerger>();

            container.RegisterTypeAs<BindingAssemblyLoader, IBindingAssemblyLoader>();

            container.RegisterTypeAs<ConfigurationLoader, IConfigurationLoader>();

            container.RegisterTypeAs<ObsoleteStepHandler, IObsoleteStepHandler>();

            container.RegisterTypeAs<EnvironmentWrapper, IEnvironmentWrapper>();
            container.RegisterTypeAs<EnvironmentOptions, IEnvironmentOptions>();
            container.RegisterTypeAs<EnvironmentInfoProvider, IEnvironmentInfoProvider>();
            container.RegisterTypeAs<BuildMetadataProvider, IBuildMetadataProvider>();
            container.RegisterTypeAs<BinaryFileAccessor, IBinaryFileAccessor>();
            container.RegisterTypeAs<TestPendingMessageFactory, ITestPendingMessageFactory>();
            container.RegisterTypeAs<TestUndefinedMessageFactory, ITestUndefinedMessageFactory>();
            container.RegisterTypeAs<DefaultTestRunSettingsProvider, ITestRunSettingsProvider>();

            container.RegisterTypeAs<ReqnrollPath, IReqnrollPath>();

            container.RegisterTypeAs<UtcDateTimeClock, IClock>();


            container.RegisterTypeAs<FileUserIdStore, IUserUniqueIdStore>();
            container.RegisterTypeAs<FileService, IFileService>();
            container.RegisterTypeAs<DirectoryService, IDirectoryService>();

            container.RegisterTypeAs<EnvironmentReqnrollTelemetryChecker, IEnvironmentReqnrollTelemetryChecker>();
            container.RegisterTypeAs<AnalyticsTransmitter, IAnalyticsTransmitter>();
            container.RegisterTypeAs<HttpClientAnalyticsTransmitterSink, IAnalyticsTransmitterSink>();
            container.RegisterTypeAs<AppInsightsEventSerializer, IAppInsightsEventSerializer>();
            container.RegisterTypeAs<HttpClientWrapper, HttpClientWrapper>();
            container.RegisterTypeAs<AnalyticsEventProvider, IAnalyticsEventProvider>();
            container.RegisterTypeAs<AnalyticsRuntimeTelemetryService, IAnalyticsRuntimeTelemetryService>();

            container.RegisterTypeAs<ReqnrollJsonLocator, IReqnrollJsonLocator>();

            container.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            container.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEventEmitter, IRuntimePluginTestExecutionLifecycleEventEmitter>();

            container.RegisterTypeAs<TestAssemblyProvider, ITestAssemblyProvider>();

            //Support for publishing Cucumber Messages
            container.RegisterTypeAs<NullFormatterLog, IFormatterLog>();
            container.RegisterTypeAs<FileSystem, IFileSystem>();
            container.RegisterTypeAs<FormattersDisabledOverrideProvider, IFormattersConfigurationDisableOverrideProvider>();
            container.RegisterTypeAs<FileBasedConfigurationResolver, IFileBasedConfigurationResolver>();
            container.RegisterTypeAs<JsonEnvironmentConfigurationResolver, IJsonEnvironmentConfigurationResolver>();
            container.RegisterTypeAs<KeyValueEnvironmentConfigurationResolver, IKeyValueEnvironmentConfigurationResolver>();
            container.RegisterTypeAs<FormattersConfigurationProvider, IFormattersConfigurationProvider>();
            container.RegisterTypeAs<MessageFormatter, ICucumberMessageFormatter>("message");
            container.RegisterTypeAs<HtmlFormatter, ICucumberMessageFormatter>("html");
            container.RegisterTypeAs<CucumberMessageBroker, ICucumberMessageBroker>();
            container.RegisterTypeAs<CucumberMessagePublisher, ICucumberMessagePublisher>();
            container.RegisterTypeAs<ShortGuidIdGenerator, IIdGenerator>();
            container.RegisterTypeAs<CucumberMessageFactory, ICucumberMessageFactory>();
            container.RegisterTypeAs<BindingMessagesGenerator, IBindingMessagesGenerator>();
            container.RegisterTypeAs<MetaMessageGenerator, IMetaMessageGenerator>();
            container.RegisterTypeAs<FeatureExecutionTrackerFactory, IFeatureExecutionTrackerFactory>();
            container.RegisterTypeAs<PickleExecutionTrackerFactory, IPickleExecutionTrackerFactory>();
            container.RegisterTypeAs<TestCaseExecutionTrackerFactory, ITestCaseExecutionTrackerFactory>();
            container.RegisterFactoryAs<IMessagePublisher>(() => container.Resolve<ICucumberMessageBroker>());
            container.RegisterTypeAs<StepTrackerFactory, IStepTrackerFactory>();
        }

        public virtual void RegisterTestThreadContainerDefaults(ObjectContainer testThreadContainer)
        {
            testThreadContainer.RegisterTypeAs<TestRunner, ITestRunner>();
            testThreadContainer.RegisterTypeAs<ContextManager, IContextManager>();
            testThreadContainer.RegisterTypeAs<TestExecutionEngine, ITestExecutionEngine>();

            testThreadContainer.RegisterTypeAs<TestThreadExecutionEventPublisher, ITestThreadExecutionEventPublisher>();

            testThreadContainer.RegisterTypeAs<ReqnrollOutputHelper, IReqnrollOutputHelper>();

            // needs to invoke methods so requires the context manager
            testThreadContainer.RegisterTypeAs<StepArgumentTypeConverter, IStepArgumentTypeConverter>();
            testThreadContainer.RegisterTypeAs<StepDefinitionMatchService, IStepDefinitionMatchService>();

            testThreadContainer.RegisterTypeAs<AsyncTraceListener, ITraceListener>();
            testThreadContainer.RegisterTypeAs<TestTracer, ITestTracer>();
        }

        public void RegisterScenarioContainerDefaults(ObjectContainer scenarioContainer)
        {
        }
    }
}