using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reqnroll.TestProjectGenerator.Conventions;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Factories;

namespace Reqnroll.TestProjectGenerator.Driver
{
    public class SolutionDriver
    {
        public const string DefaultProjectName = "DefaultTestProject";

        private readonly NuGetConfigGenerator _nuGetConfigGenerator;
        private readonly TestRunConfiguration _testRunConfiguration;
        private readonly ProjectBuilderFactory _projectBuilderFactory;
        private readonly Folders _folders;
        private readonly ArtifactNamingConvention _artifactNamingConvention;
        private readonly IOutputWriter _outputWriter;
        private readonly Solution _solution;
        private readonly Dictionary<string, ProjectBuilder> _projects = new Dictionary<string, ProjectBuilder>();
        private ProjectBuilder _defaultProject;

        public SolutionDriver(
            NuGetConfigGenerator nuGetConfigGenerator,
            TestRunConfiguration testRunConfiguration,
            ProjectBuilderFactory projectBuilderFactory,
            Folders folders,
            TestProjectFolders testProjectFolders,
            ArtifactNamingConvention artifactNamingConvention,
            IOutputWriter outputWriter)
        {
            _nuGetConfigGenerator = nuGetConfigGenerator;
            _testRunConfiguration = testRunConfiguration;
            _projectBuilderFactory = projectBuilderFactory;
            _folders = folders;
            _artifactNamingConvention = artifactNamingConvention;
            _outputWriter = outputWriter;
            NuGetSources = new List<NuGetSource>
            {
                new("LocalReqnrollDevPackages", _folders.NuGetFolder),
                new("Reqnroll CI", "https://www.myget.org/F/reqnroll/api/v3/index.json"),
                new("Reqnroll Unstable", "https://www.myget.org/F/reqnroll-unstable/api/v3/index.json")
            };

            _solution = new Solution(SolutionName);
            testProjectFolders.PathToSolutionFile = Path.Combine(_folders.RunUniqueFolderToSaveGeneratedSolutions, SolutionName, $"{SolutionName}.sln");
        }

        public Guid SolutionGuid { get; } = Guid.NewGuid();

        public IList<NuGetSource> NuGetSources { get; }

        public string SolutionName => _artifactNamingConvention.GetSolutionName(SolutionGuid);

        public IReadOnlyDictionary<string, ProjectBuilder> Projects => _projects;

        public ProjectBuilder DefaultProject
        {
            get
            {
                if (_defaultProject == null)
                {
                    _defaultProject = _projectBuilderFactory.CreateProject(DefaultProjectName, _testRunConfiguration.ProgrammingLanguage);
                    _projects.Add(_defaultProject.ProjectName, _defaultProject);
                }

                return _defaultProject;
            }
        }

        public Solution GetSolution()
        {
            foreach (var projectBuilder in Projects.Values)
            {
                Project project = projectBuilder.Build();
                _solution.AddProject(project);
            }

            var customGlobalPackagesFolder = _folders.IsGlobalPackagesCustomized ? _folders.GlobalNuGetPackages : null;
            if (customGlobalPackagesFolder != null)
                _outputWriter.WriteLine($"Using custom global packages folder: {customGlobalPackagesFolder}");
            _solution.NugetConfig = _nuGetConfigGenerator?.Generate(NuGetSources.ToArray(), customGlobalPackagesFolder);
            return _solution;
        }

        public void AddProject(ProjectBuilder project)
        {
            if (_defaultProject == null)
            {
                _defaultProject = project;
            }

            _projects.Add(project.ProjectName, project);
        }

        public void AddFile(string name, string content)
        {
            _solution.Files.Add(new SolutionFile(name, content));
        }

        public string SdkVersion
        {
            get => _solution.SdkVersion;
            set => _solution.SdkVersion = value;
        }
    }
}
