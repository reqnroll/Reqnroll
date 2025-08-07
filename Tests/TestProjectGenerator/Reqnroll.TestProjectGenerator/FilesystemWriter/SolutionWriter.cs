using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Dotnet;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public class SolutionWriter
    {
        private readonly IOutputWriter _outputWriter;
        private readonly ProjectWriterFactory _projectWriterFactory;
        private readonly FileWriter _fileWriter;
        private readonly DotNetSdkInfoProvider _netCoreSdkInfoProvider;

        public SolutionWriter(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
            _projectWriterFactory = new ProjectWriterFactory(outputWriter, new TargetFrameworkMonikerStringBuilder(), new TargetFrameworkVersionStringBuilder());
            _fileWriter = new FileWriter();
            _netCoreSdkInfoProvider = new DotNetSdkInfoProvider();
        }

        public string WriteToFileSystem(Solution solution, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath!);
            }

            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var targetFramework =
                solution.Projects
                        .Select(p => p.TargetFrameworks)
                        .FirstOrDefault();
            
            var sdk = !string.IsNullOrWhiteSpace(solution.SdkVersion) 
                ? new DotNetSdkInfo(solution.SdkVersion) 
                : _netCoreSdkInfoProvider.GetSdkFromTargetFramework(targetFramework);

            if (targetFramework != 0 && sdk != null)
            {
                var globalJsonBuilder = new GlobalJsonBuilder().WithSdk(sdk);

                var globalJsonFile = globalJsonBuilder.ToProjectFile();
                _fileWriter.Write(globalJsonFile, outputPath);
            }

            DisableUsingSdkFromEnvironmentVariable();

            var createSolutionCommand = DotNet.New(_outputWriter, sdk).Solution().InFolder(outputPath).WithName(solution.Name).Build();
            createSolutionCommand.ExecuteWithRetry(1, TimeSpan.FromSeconds(1), (innerException) =>
            {
                if (sdk != null && innerException is AggregateException aggregateException && aggregateException.InnerExceptions.Any(x => x.InnerException?.Message.Contains("Install the [" + sdk.Version) ?? false))
                    return new DotNetSdkNotInstalledException($"Sdk Version \"{sdk.Version}\" is not installed", innerException);
                return new ProjectCreationNotPossibleException("Could not create solution.", innerException);
            });
            string solutionFilePath = Path.Combine(outputPath, $"{solution.Name}.sln");

            WriteProjects(sdk, solution, outputPath, solutionFilePath);

            if (solution.NugetConfig != null)
            {
                _fileWriter.Write(solution.NugetConfig, outputPath);
            }

            foreach (var file in solution.Files)
            {
                _fileWriter.Write(file, outputPath);
            }

            return solutionFilePath;
        }

        /// <summary>
        /// During test execution, the MSBuildSDKsPath environment variable is set to the SDK of the test execution,
        /// e.g. C:\Program Files\dotnet\sdk\8.0.101\Sdks.
        /// This causes issues with dotnet restore if working with a project of a different target framework.
        /// The error might be for example 'error MSB4062: The "CheckIfPackageReferenceShouldBeFrameworkReference" task could not be loaded from the assembly[...]'.
        /// The workaround is to remove this environment variable.
        /// </summary>
        private static void DisableUsingSdkFromEnvironmentVariable()
        {
            if (Environment.GetEnvironmentVariable("MSBuildSDKsPath") != null)
                Environment.SetEnvironmentVariable("MSBuildSDKsPath", null);
        }

        private void WriteProjects(DotNetSdkInfo sdk, Solution solution, string outputPath, string solutionFilePath)
        {
            var projectPathMappings = new Dictionary<Project, string>();
            foreach (var project in solution.Projects)
            {
                var formatProjectWriter = _projectWriterFactory.FromProjectFormat(project.ProjectFormat);
                string pathToProjectFile = WriteProject(sdk, project, outputPath, formatProjectWriter, solutionFilePath);
                projectPathMappings.Add(project, pathToProjectFile);
            }

            foreach (var project in solution.Projects)
            {
                var formatProjectWriter = _projectWriterFactory.FromProjectFormat(project.ProjectFormat);
                formatProjectWriter.WriteReferences(project, projectPathMappings[project]);
            }
        }

        private string WriteProject(DotNetSdkInfo sdk, Project project, string outputPath, IProjectWriter formatProjectWriter, string solutionFilePath)
        {
            string projPath = formatProjectWriter.WriteProject(sdk, project, Path.Combine(outputPath, project.Name));

            var addProjCommand = DotNet.Sln(_outputWriter).AddProject().Project(projPath).ToSolution(solutionFilePath).Build().Execute();
            if (addProjCommand.ExitCode != 0)
            {
                throw new ProjectCreationNotPossibleException("Could not add project to solution.");
            }

            return projPath;
        }
    }
}
