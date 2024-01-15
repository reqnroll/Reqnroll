using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Dotnet;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class SolutionWriter
    {
        private readonly IOutputWriter _outputWriter;
        private readonly ProjectWriterFactory _projectWriterFactory;
        private readonly FileWriter _fileWriter;
        private readonly NetCoreSdkInfoProvider _netCoreSdkInfoProvider;

        public SolutionWriter(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
            _projectWriterFactory = new ProjectWriterFactory(outputWriter, new TargetFrameworkMonikerStringBuilder(), new TargetFrameworkVersionStringBuilder());
            _fileWriter = new FileWriter();
            _netCoreSdkInfoProvider = new NetCoreSdkInfoProvider();
        }

        public string WriteToFileSystem(Solution solution, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
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
                ? new NetCoreSdkInfo(solution.SdkVersion) 
                : _netCoreSdkInfoProvider.GetSdkFromTargetFramework(targetFramework);

            if (targetFramework != 0 && sdk != null)
            {
                var globalJsonBuilder = new GlobalJsonBuilder().WithSdk(sdk);

                var globalJsonFile = globalJsonBuilder.ToProjectFile();
                _fileWriter.Write(globalJsonFile, outputPath);
            }

            AllowNet6ToTestOlderFrameworks(targetFramework);

            var createSolutionCommand = DotNet.New(_outputWriter).Solution().InFolder(outputPath).WithName(solution.Name).Build();
            createSolutionCommand.ExecuteWithRetry(1, TimeSpan.FromSeconds(1),
                (innerException) => new ProjectCreationNotPossibleException("Could not create solution.", innerException) );
            string solutionFilePath = Path.Combine(outputPath, $"{solution.Name}.sln");

            WriteProjects(solution, outputPath, solutionFilePath);

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
        /// (santa) 2021.10.28 In .Net6 prerelease there is an issue when using 'dotnet' commands from tests
        /// The environment variable is set to MSBuildSDKsPath=C:\Program Files\dotnet\sdk\6.0.100-rc.1.21463.6\Sdks
        /// As a result the dotnet restore command fails
        /// The workaround is to remove this environment variable
        /// </summary>
        /// <param name="targetFramework"></param>
        private static void AllowNet6ToTestOlderFrameworks(TargetFramework targetFramework)
        {
            if (targetFramework is TargetFramework.Net461 or TargetFramework.Netcoreapp31 or TargetFramework.Netcoreapp21 or TargetFramework.Net50 or TargetFramework.Net462) 
                Environment.SetEnvironmentVariable("MSBuildSDKsPath", null);
        }

        private void WriteProjects(Solution solution, string outputPath, string solutionFilePath)
        {
            var projectPathMappings = new Dictionary<Project, string>();
            foreach (var project in solution.Projects)
            {
                var formatProjectWriter = _projectWriterFactory.FromProjectFormat(project.ProjectFormat);
                string pathToProjectFile = WriteProject(project, outputPath, formatProjectWriter, solutionFilePath);
                projectPathMappings.Add(project, pathToProjectFile);
            }

            foreach (var project in solution.Projects)
            {
                var formatProjectWriter = _projectWriterFactory.FromProjectFormat(project.ProjectFormat);
                formatProjectWriter.WriteReferences(project, projectPathMappings[project]);
            }
        }

        private string WriteProject(Project project, string outputPath, IProjectWriter formatProjectWriter, string solutionFilePath)
        {
            string projPath = formatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));

            var addProjCommand = DotNet.Sln(_outputWriter).AddProject().Project(projPath).ToSolution(solutionFilePath).Build().Execute();
            if (addProjCommand.ExitCode != 0)
            {
                throw new ProjectCreationNotPossibleException("Could not add project to solution.");
            }

            return projPath;
        }
    }
}
