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
        log.LogTaskDiagnosticMessage($"Project folder: {reqnrollProjectInfo.ProjectFolder}");
        foreach (var generatorPluginInfo in reqnrollProjectInfo.GeneratorPlugins)
            log.LogTaskDiagnosticMessage($"Generator plugin: {generatorPluginInfo.PathToGeneratorPluginAssembly}, parameters: {generatorPluginInfo.GetLegacyPluginParameters()}");
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
            .GenerateFilesForProject()
            .ToArray();

        var result = new List<ITaskItem>();
        foreach (var generatorResult in generatedFiles)
        {
            var taskItem = new TaskItem { ItemSpec = FileSystemHelper.GetRelativePath(generatorResult.CodeBehindFileFullPath, reqnrollProjectInfo.ProjectFolder) };
            var messagesFileRelativePath = generatorResult.MessagesFileFullPath == null ? null : FileSystemHelper.GetRelativePath(generatorResult.MessagesFileFullPath, reqnrollProjectInfo.ProjectFolder);
            taskItem.SetMetadata(GenerateFeatureFileCodeBehindTask.MessagesFileMetadata, messagesFileRelativePath);
            taskItem.SetMetadata(GenerateFeatureFileCodeBehindTask.MessagesResourceNameMetadata, generatorResult.MessagesResourceName);
            result.Add(taskItem);

            log.LogTaskDiagnosticMessage($"Output {taskItem.ItemSpec} ({generatorResult.CodeBehindFileFullPath})");
            log.LogTaskDiagnosticMessage($"  Messages: {messagesFileRelativePath} ({generatorResult.MessagesFileFullPath})");
            log.LogTaskDiagnosticMessage($"  MessagesResourceName: {generatorResult.MessagesResourceName}");
        }
        return result;
    }
}