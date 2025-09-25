using System.Collections.Generic;
using System.IO;
using Reqnroll.Generator;
using Reqnroll.Utils;

namespace Reqnroll.Tools.MsBuild.Generation;

public class ReqnrollProjectInfo(
    IReadOnlyCollection<GeneratorPluginInfo> generatorPlugins,
    IReadOnlyCollection<ReqnrollFeatureFileInfo> featureFiles,
    string projectPath,
    string projectFolder,
    string projectGuid,
    string projectAssemblyName,
    string outputPath,
    string rootNamespace,
    string targetFrameworks,
    string currentTargetFramework)
{
    public IReadOnlyCollection<GeneratorPluginInfo> GeneratorPlugins { get; } = generatorPlugins;
    public IReadOnlyCollection<ReqnrollFeatureFileInfo> FeatureFiles { get; } = featureFiles ?? [];
    public string ProjectPath { get; } = projectPath;
    public string ProjectFolder { get; } = projectFolder;
    public string ProjectGuid { get; } = projectGuid;
    public string ProjectAssemblyName { get; } = projectAssemblyName;
    public string OutputPath { get; } = outputPath;
    public string RootNamespace { get; } = rootNamespace;
    public string TargetFrameworks { get; } = targetFrameworks;
    public string CurrentTargetFramework { get; } = currentTargetFramework;

    public string GetFullPathAndNormalize(string projectRelativePath) => 
        FileSystemHelper.NormalizeDirectorySeparators(Path.GetFullPath(Path.Combine(ProjectFolder, projectRelativePath)));
}