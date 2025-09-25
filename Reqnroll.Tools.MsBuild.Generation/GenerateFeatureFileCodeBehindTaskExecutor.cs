#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.BoDi;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Reqnroll.CommonModels;
using Reqnroll.Utils;
using Task = System.Threading.Tasks.Task;

namespace Reqnroll.Tools.MsBuild.Generation;

public class GenerateFeatureFileCodeBehindTaskExecutor(
    IProcessInfoDumper processInfoDumper,
    IReqnrollTaskLoggingHelper log,
    IReqnrollProjectProvider reqnrollProjectProvider,
    ReqnrollProjectInfo reqnrollProjectInfo,
    WrappedGeneratorContainerBuilder wrappedGeneratorContainerBuilder,
    IObjectContainer rootObjectContainer,
    IMSBuildTaskAnalyticsTransmitter msbuildTaskAnalyticsTransmitter,
    IExceptionTaskLogger exceptionTaskLogger)
    : IGenerateFeatureFileCodeBehindTaskExecutor
{
    public IResult<IReadOnlyCollection<ITaskItem>> Execute()
    {
        processInfoDumper.DumpProcessInfo();
        log.LogTaskMessage("Starting GenerateFeatureFileCodeBehind task");

        try
        {
            var reqnrollProject = reqnrollProjectProvider.GetReqnrollProject();

            using var generatorContainer = wrappedGeneratorContainerBuilder.BuildGeneratorContainer(
                reqnrollProject.ProjectSettings.ConfigurationHolder,
                reqnrollProject.ProjectSettings,
                reqnrollProjectInfo.GeneratorPlugins,
                rootObjectContainer);
            var featureFileCodeBehindGenerator = generatorContainer.Resolve<IFeatureFileCodeBehindGenerator>();

            _ = Task.Run(msbuildTaskAnalyticsTransmitter.TryTransmitProjectCompilingEventAsync);

            var returnValue = GenerateCodeBehindFilesForProject(featureFileCodeBehindGenerator);

            if (log.HasLoggedErrors)
            {
                return Result<IReadOnlyCollection<ITaskItem>>.Failure("Feature file code-behind generation has failed with errors.");
            }

            return Result.Success(returnValue);
        }
        catch (Exception e)
        {
            exceptionTaskLogger.LogException(e);
            processInfoDumper.DumpLoadedAssemblies();
            return Result<IReadOnlyCollection<ITaskItem>>.Failure(e);
        }
        finally
        {
            processInfoDumper.DumpLoadedAssemblies();
        }
    }

    private IReadOnlyCollection<ITaskItem> GenerateCodeBehindFilesForProject(IFeatureFileCodeBehindGenerator featureFileCodeBehindGenerator)
    {
        var generatedFiles = featureFileCodeBehindGenerator
            .GenerateFilesForProject();

        return generatedFiles
               .Select(item =>
               {
                   var result = new TaskItem { ItemSpec = FileSystemHelper.GetRelativePath(item.CodeBehindFileFullPath, reqnrollProjectInfo.ProjectFolder) };
                   result.SetMetadata(GenerateFeatureFileCodeBehindTask.MessagesFileMetadata, item.MessagesFileFullPath == null ? null : FileSystemHelper.GetRelativePath(item.MessagesFileFullPath, reqnrollProjectInfo.ProjectFolder));
                   result.SetMetadata(GenerateFeatureFileCodeBehindTask.MessagesResourceNameMetadata, item.MessagesResourceName);
                   return result;
               })
               .Cast<ITaskItem>()
               .ToArray();
    }
}