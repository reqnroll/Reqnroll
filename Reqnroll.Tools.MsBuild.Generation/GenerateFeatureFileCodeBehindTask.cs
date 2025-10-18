using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Reqnroll.CommonModels;
using Reqnroll.Generator;
using Reqnroll.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Reqnroll.Tools.MsBuild.Generation;

public class GenerateFeatureFileCodeBehindTask : Task
{
    public const string CodeBehindFileMetadata = "CodeBehindFile"; //in
    public const string MessagesFileMetadata = "MessagesFile"; //in,out
    public const string MessagesResourceNameMetadata = "MessagesResourceName"; //out

    public IGenerateFeatureFileCodeBehindTaskDependencyCustomizations DependencyCustomizations { get; set; }

    [Required]
    public string ProjectPath { get; set; }

    public string RootNamespace { get; set; }

    public string ProjectFolder => Path.GetDirectoryName(ProjectPath);
    public string OutputPath { get; set; }
    public ITaskItem[] FeatureFiles { get; set; }

    public ITaskItem[] GeneratorPlugins { get; set; }

    [Output]
    public ITaskItem[] GeneratedFiles { get; private set; }

    public string MSBuildVersion { get; set; }
    public string AssemblyName { get; set; }
    public string TargetFrameworks { get; set; }
    public string TargetFramework { get; set; }
    public string ProjectGuid { get; set; }

    public bool LaunchDebugger { get; set; }

    public override bool Execute()
    {
        if (LaunchDebugger) Debugger.Launch();

        var generateFeatureFileCodeBehindTaskContainerBuilder = new GenerateFeatureFileCodeBehindTaskContainerBuilder();
        var generatorPlugins = GeneratorPlugins?.Select(gp => new GeneratorPluginInfo(gp.ItemSpec, GetPluginParameters(gp))).ToArray() ?? [];
        var featureFiles = FeatureFiles?
                           .Where(i => FileFilter.IsValidFile(i.ItemSpec))
                           .Select(i => new ReqnrollFeatureFileInfo(i.ItemSpec, i.GetMetadata(CodeBehindFileMetadata), i.GetMetadata(MessagesFileMetadata)))
                           .ToArray() ?? [];

        var msbuildInformationProvider = new MSBuildInformationProvider(MSBuildVersion);
        var reqnrollProjectInfo = new ReqnrollProjectInfo(generatorPlugins, featureFiles, ProjectPath, ProjectFolder, ProjectGuid, AssemblyName, OutputPath, RootNamespace, TargetFrameworks, TargetFramework);
        var dependencyCustomizations = DependencyCustomizations ?? new NullGenerateFeatureFileCodeBehindTaskDependencyCustomizations();

        using var taskRootContainer = generateFeatureFileCodeBehindTaskContainerBuilder.BuildRootContainer(Log, reqnrollProjectInfo, msbuildInformationProvider, dependencyCustomizations);
        var assemblyResolveLoggerFactory = taskRootContainer.Resolve<IAssemblyResolveLoggerFactory>();

        using (assemblyResolveLoggerFactory.Build())
        {
            var taskExecutor = taskRootContainer.Resolve<IGenerateFeatureFileCodeBehindTaskExecutor>();
            var executeResult = taskExecutor.Execute();

            if (executeResult is not ISuccess<IReadOnlyCollection<ITaskItem>> success)
            {
                return false;
            }

            GeneratedFiles = success.Result.ToArray();

            return true;
        }
    }

    private Dictionary<string, string> GetPluginParameters(ITaskItem pluginItem)
    {
        var pluginParameters = new Dictionary<string, string>();
        foreach (var parameterName in pluginItem.MetadataNames.OfType<string>())
        {
            const string pluginParameterPrefix = "Parameter_";
            if (parameterName.StartsWith(pluginParameterPrefix))
            {
                var parameterValue = pluginItem.GetMetadata(parameterName);
                pluginParameters[parameterName.Substring(pluginParameterPrefix.Length)] = parameterValue;
            }
        }
        return pluginParameters;
    }
}