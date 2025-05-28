using Reqnroll.BoDi;
using Reqnroll.Analytics;
using Reqnroll.Analytics.AppInsights;
using Reqnroll.Analytics.UserId;
using Reqnroll.BindingSkeletons;
using Reqnroll.Bindings;
using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.ErrorHandling;
using Reqnroll.Events;
using Reqnroll.FileAccess;
using Reqnroll.Plugins;
using Reqnroll.TestFramework;
using Reqnroll.Time;
using Reqnroll.Tracing;
using Reqnroll.PlatformCompatibility;
using Reqnroll.InternalPlugins;

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
#pragma warning disable CS0618
            container.RegisterTypeAs<BindingInvoker, IBindingInvoker>();
#pragma warning restore CS0618
            container.RegisterTypeAs<BindingInvoker, IAsyncBindingInvoker>();
            container.RegisterTypeAs<BindingDelegateInvoker, IBindingDelegateInvoker>();
            container.RegisterTypeAs<TestObjectResolver, ITestObjectResolver>();

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
            container.RegisterTypeAs<EnvironmentInfoProvider, IEnvironmentInfoProvider>();
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

            container.RegisterTypeAs<ReqnrollJsonLocator, IReqnrollJsonLocator>();

            container.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEvents, RuntimePluginTestExecutionLifecycleEvents>();
            container.RegisterTypeAs<RuntimePluginTestExecutionLifecycleEventEmitter, IRuntimePluginTestExecutionLifecycleEventEmitter>();

            container.RegisterTypeAs<TestAssemblyProvider, ITestAssemblyProvider>();

            // Internal plugins
            container.RegisterTypeAs<DryRunBindingInvokerPlugin, IRuntimePlugin>("dryrun");
        }

        public virtual void RegisterTestThreadContainerDefaults(ObjectContainer testThreadContainer)
        {
            testThreadContainer.RegisterTypeAs<TestRunner, ITestRunner>();
            testThreadContainer.RegisterTypeAs<BlockingSyncTestRunner, ISyncTestRunner>();
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