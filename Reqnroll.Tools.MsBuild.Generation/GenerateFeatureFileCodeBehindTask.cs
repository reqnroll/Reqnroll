using Microsoft.Build.Framework;
#if NETFRAMEWORK
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
#endif
using Reqnroll.Analytics;
using Reqnroll.CommonModels;
using Reqnroll.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Reqnroll.Tools.MsBuild.Generation
{
    internal static class AsyncRunner
    {
#if NETFRAMEWORK
        private static readonly Lazy<JoinableTaskFactory> _factory = new(() =>
        {
            // If we're running inside VS, ThreadHelper.JoinableTaskFactory will be initialized
            // Otherwise, create our own context/factory for headless builds
            return TryGetVsFactory() ?? new JoinableTaskFactory(new JoinableTaskContext());
        });

        private static JoinableTaskFactory TryGetVsFactory()
        {
            try
            {
                // ThreadHelper.JoinableTaskFactory throws if not initialized
                return ThreadHelper.JoinableTaskFactory;
            }
            catch
            {
                return null;
            }
        }

        public static T RunAndJoin<T>(Func<Task<T>> func) => _factory.Value.Run(func);

#else

        public static T RunAndJoin<T>(Func<Task<T>> func) => func().GetAwaiter().GetResult();

#endif
    }

    public class GenerateFeatureFileCodeBehindTask : Microsoft.Build.Utilities.Task
    {
        public IFeatureFileCodeBehindGenerator CodeBehindGenerator { get; set; }
        public IAnalyticsTransmitter AnalyticsTransmitter { get; set; }

        [Required]
        public string ProjectPath { get; set; }

        public string RootNamespace { get; set; }

        public string ProjectFolder => Path.GetDirectoryName(ProjectPath);
        public string OutputPath { get; set; }
        public string IntermediateOutputPath { get; set; }
        public ITaskItem[] FeatureFiles { get; set; }

        public ITaskItem[] GeneratorPlugins { get; set; }

        [Output]
        public ITaskItem[] GeneratedFiles { get; private set; }

        public string MSBuildVersion { get; set; }
        public string AssemblyName { get; set; }
        public string TargetFrameworks { get; set; }
        public string TargetFramework { get; set; }
        public string ProjectGuid { get; set; }

        public override bool Execute() => AsyncRunner.RunAndJoin(ExecuteAsync);

        public async Task<bool> ExecuteAsync()
        {
            var generateFeatureFileCodeBehindTaskContainerBuilder = new GenerateFeatureFileCodeBehindTaskContainerBuilder();
            var generatorPlugins = GeneratorPlugins?.Select(gp => gp.ItemSpec).Select(p => new GeneratorPluginInfo(p)).ToArray() ?? [];
            var featureFiles = FeatureFiles?.Select(i => i.ItemSpec).ToArray() ?? [];

            var msbuildInformationProvider = new MSBuildInformationProvider(MSBuildVersion);
            var generateFeatureFileCodeBehindTaskConfiguration = new GenerateFeatureFileCodeBehindTaskConfiguration(AnalyticsTransmitter, CodeBehindGenerator);
            var generateFeatureFileCodeBehindTaskInfo = new ReqnrollProjectInfo(generatorPlugins, featureFiles, ProjectPath, ProjectFolder, ProjectGuid, AssemblyName, OutputPath, IntermediateOutputPath, RootNamespace, TargetFrameworks, TargetFramework);

            await using var taskRootContainer = generateFeatureFileCodeBehindTaskContainerBuilder.BuildRootContainer(
                Log,
                generateFeatureFileCodeBehindTaskInfo,
                msbuildInformationProvider,
                generateFeatureFileCodeBehindTaskConfiguration);

            var assemblyResolveLoggerFactory = taskRootContainer.Resolve<IAssemblyResolveLoggerFactory>();

            using (assemblyResolveLoggerFactory.Build())
            {
                var taskExecutor = taskRootContainer.Resolve<IGenerateFeatureFileCodeBehindTaskExecutor>();
                var executeResult = await taskExecutor.ExecuteAsync();

                if (executeResult is not ISuccess<IReadOnlyCollection<ITaskItem>> success)
                {
                    return false;
                }

                GeneratedFiles = [.. success.Result];
                return true;
            }
        }
    }
}
