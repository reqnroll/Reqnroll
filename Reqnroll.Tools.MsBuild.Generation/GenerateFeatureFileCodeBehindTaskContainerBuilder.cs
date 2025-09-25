using Reqnroll.BoDi;
using Microsoft.Build.Utilities;
using Reqnroll.Analytics;
using Reqnroll.Analytics.AppInsights;
using Reqnroll.Analytics.UserId;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Generator.Project;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class GenerateFeatureFileCodeBehindTaskContainerBuilder
    {
        public IObjectContainer BuildRootContainer(
            TaskLoggingHelper taskLoggingHelper,
            ReqnrollProjectInfo reqnrollProjectInfo,
            IMSBuildInformationProvider msbuildInformationProvider,
            IGenerateFeatureFileCodeBehindTaskDependencyCustomizations dependencyCustomizations)
        {
            var objectContainer = new ObjectContainer();

            // singletons
            objectContainer.RegisterInstanceAs(taskLoggingHelper);
            objectContainer.RegisterInstanceAs(reqnrollProjectInfo);
            objectContainer.RegisterInstanceAs(msbuildInformationProvider);
            objectContainer.RegisterInstanceAs(dependencyCustomizations);

            // types
            objectContainer.RegisterTypeAs<ReqnrollTaskLoggingHelper, IReqnrollTaskLoggingHelper>();
            objectContainer.RegisterTypeAs<ReqnrollProjectProvider, IReqnrollProjectProvider>();
            objectContainer.RegisterTypeAs<MSBuildProjectReader, IMSBuildProjectReader>();
            objectContainer.RegisterTypeAs<ProcessInfoDumper, IProcessInfoDumper>();
            objectContainer.RegisterTypeAs<AssemblyResolveLoggerFactory, IAssemblyResolveLoggerFactory>();
            objectContainer.RegisterTypeAs<GenerateFeatureFileCodeBehindTaskExecutor, IGenerateFeatureFileCodeBehindTaskExecutor>();
            objectContainer.RegisterTypeAs<MSBuildTaskAnalyticsTransmitter, IMSBuildTaskAnalyticsTransmitter>();
            objectContainer.RegisterTypeAs<ExceptionTaskLogger, IExceptionTaskLogger>();

            objectContainer.RegisterTypeAs<FileUserIdStore, IUserUniqueIdStore>();
            objectContainer.RegisterTypeAs<FileService, IFileService>();
            objectContainer.RegisterTypeAs<DirectoryService, IDirectoryService>();
            objectContainer.RegisterTypeAs<EnvironmentWrapper, IEnvironmentWrapper>();
            objectContainer.RegisterTypeAs<EnvironmentInfoProvider, IEnvironmentInfoProvider>();
            objectContainer.RegisterTypeAs<EnvironmentReqnrollTelemetryChecker, IEnvironmentReqnrollTelemetryChecker>();
            objectContainer.RegisterTypeAs<AnalyticsTransmitter, IAnalyticsTransmitter>();
            objectContainer.RegisterTypeAs<HttpClientAnalyticsTransmitterSink, IAnalyticsTransmitterSink>();
            objectContainer.RegisterTypeAs<AppInsightsEventSerializer, IAppInsightsEventSerializer>();
            objectContainer.RegisterTypeAs<HttpClientWrapper, HttpClientWrapper>();
            objectContainer.RegisterTypeAs<AnalyticsEventProvider, IAnalyticsEventProvider>();
            objectContainer.RegisterTypeAs<ConfigurationLoader, IConfigurationLoader>();
            objectContainer.RegisterTypeAs<ProjectReader, IReqnrollProjectReader>();
            objectContainer.RegisterTypeAs<ReqnrollJsonLocator, IReqnrollJsonLocator>();

            dependencyCustomizations.CustomizeTaskContainerDependencies(objectContainer);

            return objectContainer;
        }
    }
}
