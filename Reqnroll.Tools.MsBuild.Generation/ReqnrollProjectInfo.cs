using System.Collections.Generic;
using Reqnroll.Generator;
using Reqnroll.Utils;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class ReqnrollProjectInfo
    {
        public ReqnrollProjectInfo(
            IReadOnlyCollection<GeneratorPluginInfo> generatorPlugins,
            IReadOnlyCollection<string> featureFiles,
            string projectPath,
            string projectFolder,
            string projectGuid,
            string projectAssemblyName,
            string outputPath,
            string intermediateOutputPath,
            string rootNamespace,
            string targetFrameworks,
            string currentTargetFramework)
        {
            GeneratorPlugins = generatorPlugins;
            FeatureFiles = FileFilter.GetValidFiles(featureFiles);
            ProjectFolder = projectFolder;
            OutputPath = outputPath;
            IntermediateOutputPath = intermediateOutputPath;
            RootNamespace = rootNamespace;
            TargetFrameworks = targetFrameworks;
            CurrentTargetFramework = currentTargetFramework;
            ProjectGuid = projectGuid;
            ProjectAssemblyName = projectAssemblyName;
            ProjectPath = projectPath;
        }

        public IReadOnlyCollection<GeneratorPluginInfo> GeneratorPlugins { get; }

        public IReadOnlyCollection<string> FeatureFiles { get; }

        public string ProjectPath { get; }

        public string ProjectFolder { get; }

        public string ProjectGuid { get; }

        public string ProjectAssemblyName { get; }

        public string OutputPath { get; }
        public string IntermediateOutputPath { get; }

        public string RootNamespace { get; }

        public string TargetFrameworks { get; }

        public string CurrentTargetFramework { get; }

    }
}
