using System;
using System.IO;
using FluentAssertions;
using Moq;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Extensions;
using Reqnroll.TestProjectGenerator.FilesystemWriter;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Reqnroll.TestProjectGenerator.Tests
{
    public class ProjectTests
    {
        private const TargetFramework ProjectTargetFramework = TargetFramework.Net462;
        private readonly TargetFrameworkMonikerStringBuilder _targetFrameworkMonikerStringBuilder;
        private readonly string TargetFrameworkMoniker;

        public ProjectTests()
        {
            _targetFrameworkMonikerStringBuilder = new TargetFrameworkMonikerStringBuilder();
            TargetFrameworkMoniker = _targetFrameworkMonikerStringBuilder.BuildTargetFrameworkMoniker(ProjectTargetFramework);
        }

        public (Solution, Project, string) CreateEmptySolutionAndProject(ProjectFormat projectFormat, ProgrammingLanguage programmingLanguage, TargetFramework targetFramework = ProjectTargetFramework)
        {
            var folder = Path.Combine(Path.GetTempPath(), "Reqnroll.TestProjectGenerator.Tests", Guid.NewGuid().ToString("N"));

            var solution = new Solution("SolutionName");
            var project = new Project("ProjectName", Guid.NewGuid(), programmingLanguage, targetFramework, projectFormat);

            solution.AddProject(project);

            return (solution, project, folder);
        }

        private SolutionWriter CreateSolutionWriter() => new SolutionWriter(new Mock<IOutputWriter>().Object);

        private void RunSkippableTest(Action test)
        {
            try
            {
                test();
            }
            catch (DotNetSdkNotInstalledException ex)
            {
                Skip.IfNot(new ConfigurationDriver().PipelineMode, ex.ToString());
            }
        }

        [SkippableFact]
        public void AddNuGetPackageToProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);


                project.AddNuGetPackage("Reqnroll", "2.3.1", new NuGetPackageAssembly("Reqnroll, Version=2.3.1.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL", "net45\\Reqnroll.dll"));


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);


                projectFileContent.Should().Contain("<PackageReference Include=\"Reqnroll\" Version=\"2.3.1\" />");
            });
        }

        [SkippableFact]
        public void AddNuGetPackageToProjectInOldFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);


                project.AddNuGetPackage("Reqnroll", "2.3.1", new NuGetPackageAssembly("Reqnroll, Version=2.3.1.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL", "net45\\Reqnroll.dll"));


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);


                projectFileContent.Should().Contain("<Import Project=\"..\\packages\\Reqnroll.2.3.1\\build\\Reqnroll.targets\" Condition=\"Exists(\'..\\packages\\Reqnroll.2.3.1\\build\\Reqnroll.targets\')\" />");
                projectFileContent.Should().Match("*<Reference Include=\"Reqnroll, Version=2.3.1.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL\">*<HintPath>..\\packages\\Reqnroll.2.3.1\\lib\\net45\\Reqnroll.dll</HintPath>*</Reference>*");
            });
        }


        [SkippableFact]
        public void AddNuGetPackageWithMSBuildFilesToProjectInOldFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);


                project.AddNuGetPackage(
                    "Reqnroll.Tools.MsBuild.Generation",
                    "2.3.2-preview20180328");


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);


                projectFileContent.Should().Contain("<Import Project=\"..\\packages\\Reqnroll.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\Reqnroll.Tools.MsBuild.Generation.props\" Condition=\"Exists(\'..\\packages\\Reqnroll.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\Reqnroll.Tools.MsBuild.Generation.props\')\" />");
                projectFileContent.Should().Contain("<Import Project=\"..\\packages\\Reqnroll.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\Reqnroll.Tools.MsBuild.Generation.targets\" Condition=\"Exists(\'..\\packages\\Reqnroll.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\Reqnroll.Tools.MsBuild.Generation.targets\')\" />");
            });
        }

        [SkippableFact]
        public void AddReferenceToProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);


                project.AddReference("System.Configuration");


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);


                projectFileContent.Should().Contain("<Reference Include=\"System.Configuration\" />");
            });
        }

        [SkippableFact]
        public void AddReferenceToProjectInOldFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);


                project.AddReference("System.Configuration");


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);


                projectFileContent.Should().Contain("<Reference Include=\"System.Configuration\" />");
            });
        }


        [SkippableFact]
        public void AddFileToProjectInOldFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);

                var projectFile = new ProjectFile("File.cs", "Compile", "//no code");

                project.AddFile(projectFile);


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);
                var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "File.cs");


                projectFileContent.Should().Contain("<Compile Include=\"File.cs\" />");
                File.Exists(filePath).Should().BeTrue();
                File.ReadAllText(filePath).Should().Contain("//no code");
            });
        }

        [SkippableFact]
        public void AddFileToProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);

                var projectFile = new ProjectFile("File.cs", "Compile", "//no code");

                project.AddFile(projectFile);


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);
                var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "File.cs");


                projectFileContent.Should().NotContain("<Compile");
                File.Exists(filePath).Should().BeTrue();
                File.ReadAllText(filePath).Should().Contain("//no code");
            });
        }

        [SkippableFact]
        public void AddFileInFolderToProjectInOldFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);

                var projectFile = new ProjectFile(Path.Combine("Folder", "File.cs"), "Compile", "//no code");

                project.AddFile(projectFile);


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);
                var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "Folder", "File.cs");


                projectFileContent.Should().Contain("<Compile Include=\"Folder\\File.cs\" />");
                File.Exists(filePath).Should().BeTrue();
                File.ReadAllText(filePath).Should().Contain("//no code");
            });
        }

        [SkippableFact]
        public void AddFileInFolderToProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);

                var projectFile = new ProjectFile(Path.Combine("Folder", "File.cs"), "Compile", "//no code");

                project.AddFile(projectFile);


                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                var projectFileContent = GetProjectFileContent(solutionFolder, project);
                var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "Folder", "File.cs");


                projectFileContent.Should().NotContain("<Compile");
                File.Exists(filePath).Should().BeTrue();
                File.ReadAllText(filePath).Should().Contain("//no code");
            });
        }

        private string GetProjectFileContent(string solutionFolder, Project project)
        {
            return File.ReadAllText(
                Path.Combine(
                    GetProjectFolderPath(solutionFolder, project),
                    $"{project.Name}.{project.ProgrammingLanguage.ToProjectFileExtension()}"));
        }

        private string GetProjectFolderPath(string solutionFolder, Project project)
        {
            return Path.Combine(solutionFolder, project.Name);
        }

        [SkippableFact]
        public void CreateEmptyCSharpProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);

                projectFileContent.Should().Contain("<Project Sdk=\"Microsoft.NET.Sdk\">");
            });
        }

        [SkippableFact]
        public void CreateEmptyCSharpNet60ProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, TargetFramework.Net60);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);

                projectFileContent.Should()
                                  .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net6.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>");
            });
        }

        [SkippableFact]
        public void CreateEmptyCSharpNet70ProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, TargetFramework.Net70);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);

                projectFileContent.Should()
                                  .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net7.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>");
            });
        }

        [SkippableFact]
        public void CreateEmptyCSharpNet80ProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, TargetFramework.Net80);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);

                projectFileContent.Should()
                                  .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net8.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>");
            });
        }

        [SkippableFact]
        public void CreateEmptyCSharpNet481ProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp73, TargetFramework.Net481);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);

                projectFileContent.Should()
                                  .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net481</TargetFramework>\r\n    <LangVersion>7.3</LangVersion>");
            });
        }

        [SkippableFact]
        public void CreateEmptyCSharpNet462ProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp73, TargetFramework.Net462);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);

                projectFileContent.Should()
                                  .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net462</TargetFramework>\r\n    <LangVersion>7.3</LangVersion>");
            });
        }

        [SkippableFact]
        public void CreateEmptyCSharpNet472ProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp73, TargetFramework.Net472);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);

                projectFileContent.Should()
                                  .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net472</TargetFramework>\r\n    <LangVersion>7.3</LangVersion>");
            });
        }

        [SkippableFact]
        public void CreateEmptyFSharpProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.FSharp);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);
                projectFileContent.Should().Contain("<Project Sdk=\"Microsoft.NET.Sdk\">");
                projectFileContent.Should().Contain("<TargetFramework>net462</TargetFramework>");
                projectFileContent.Should().Contain("<ItemGroup>\r\n    <Compile Include=\"Library.fs\" />\r\n  </ItemGroup>");
            });
        }

        [SkippableFact]
        public void CreateEmptyVbProjectInNewFormat()
        {
            RunSkippableTest(() =>
            {
                var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.VB);

                CreateSolutionWriter().WriteToFileSystem(solution, solutionFolder);

                string projectFileContent = GetProjectFileContent(solutionFolder, project);

                projectFileContent.Should()
                                  .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <RootNamespace>ProjectName</RootNamespace>\r\n    <TargetFramework>net462</TargetFramework>");
            });
        }
    }
}
