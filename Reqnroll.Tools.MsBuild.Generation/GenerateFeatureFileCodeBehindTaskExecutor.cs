using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reqnroll.BoDi;
using Microsoft.Build.Framework;
using Reqnroll.CommonModels;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class GenerateFeatureFileCodeBehindTaskExecutor : IGenerateFeatureFileCodeBehindTaskExecutor
    {
        private readonly IProcessInfoDumper _processInfoDumper;
        private readonly ITaskLoggingWrapper _taskLoggingWrapper;
        private readonly IReqnrollProjectProvider _reqnrollProjectProvider;
        private readonly ReqnrollProjectInfo _reqnrollProjectInfo;
        private readonly WrappedGeneratorContainerBuilder _wrappedGeneratorContainerBuilder;
        private readonly IObjectContainer _rootObjectContainer;
        private readonly IMSBuildTaskAnalyticsTransmitter _msbuildTaskAnalyticsTransmitter;
        private readonly IExceptionTaskLogger _exceptionTaskLogger;

        public GenerateFeatureFileCodeBehindTaskExecutor(
            IProcessInfoDumper processInfoDumper,
            ITaskLoggingWrapper taskLoggingWrapper,
            IReqnrollProjectProvider reqnrollProjectProvider,
            ReqnrollProjectInfo reqnrollProjectInfo,
            WrappedGeneratorContainerBuilder wrappedGeneratorContainerBuilder,
            IObjectContainer rootObjectContainer,
            IMSBuildTaskAnalyticsTransmitter msbuildTaskAnalyticsTransmitter,
            IExceptionTaskLogger exceptionTaskLogger)
        {
            _processInfoDumper = processInfoDumper;
            _taskLoggingWrapper = taskLoggingWrapper;
            _reqnrollProjectProvider = reqnrollProjectProvider;
            _reqnrollProjectInfo = reqnrollProjectInfo;
            _wrappedGeneratorContainerBuilder = wrappedGeneratorContainerBuilder;
            _rootObjectContainer = rootObjectContainer;
            _msbuildTaskAnalyticsTransmitter = msbuildTaskAnalyticsTransmitter;
            _exceptionTaskLogger = exceptionTaskLogger;
        }

        public IResult<IReadOnlyCollection<ITaskItem>> Execute()
        {
            _processInfoDumper.DumpProcessInfo();
            _taskLoggingWrapper.LogMessage("Starting GenerateFeatureFileCodeBehind");

            try
            {
                var reqnrollProject = _reqnrollProjectProvider.GetReqnrollProject();
                reqnrollProject.ProjectSettings.FeatureFilesEmbedded = _reqnrollProjectInfo.FeatureFilesEmbedded;

                using var generatorContainer = _wrappedGeneratorContainerBuilder.BuildGeneratorContainer(
                    reqnrollProject.ProjectSettings.ConfigurationHolder,
                    reqnrollProject.ProjectSettings,
                    _reqnrollProjectInfo.GeneratorPlugins,
                    _rootObjectContainer);
                var projectCodeBehindGenerator = generatorContainer.Resolve<IProjectCodeBehindGenerator>();

                _ = Task.Run(_msbuildTaskAnalyticsTransmitter.TryTransmitProjectCompilingEventAsync);

                var returnValue = projectCodeBehindGenerator.GenerateCodeBehindFilesForProject();

                if (_taskLoggingWrapper.HasLoggedErrors())
                {
                    return Result<IReadOnlyCollection<ITaskItem>>.Failure("Feature file code-behind generation has failed with errors.");
                }

                return Result.Success(returnValue);
            }
            catch (Exception e)
            {
                _exceptionTaskLogger.LogException(e);
                _processInfoDumper.DumpLoadedAssemblies();
                return Result<IReadOnlyCollection<ITaskItem>>.Failure(e);
            }
            finally
            {
                _processInfoDumper.DumpLoadedAssemblies();
            }
        }
    }
}