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

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddNuGetPackageToProjectInNewFormat()
        {
            
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);


            project.AddNuGetPackage("Reqnroll", "2.3.1", new NuGetPackageAssembly("Reqnroll, Version=2.3.1.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL", "net45\\Reqnroll.dll"));


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<PackageReference Include=\"Reqnroll\" Version=\"2.3.1\" />");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddNuGetPackageToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);


            project.AddNuGetPackage("Reqnroll", "2.3.1", new NuGetPackageAssembly("Reqnroll, Version=2.3.1.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL", "net45\\Reqnroll.dll"));


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<Import Project=\"..\\packages\\Reqnroll.2.3.1\\build\\Reqnroll.targets\" Condition=\"Exists(\'..\\packages\\Reqnroll.2.3.1\\build\\Reqnroll.targets\')\" />");
            projectFileContent.Should().Match("*<Reference Include=\"Reqnroll, Version=2.3.1.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL\">*<HintPath>..\\packages\\Reqnroll.2.3.1\\lib\\net45\\Reqnroll.dll</HintPath>*</Reference>*");
        }


        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddNuGetPackageWithMSBuildFilesToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);


            project.AddNuGetPackage(
                "Reqnroll.Tools.MsBuild.Generation",
                "2.3.2-preview20180328");


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<Import Project=\"..\\packages\\Reqnroll.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\Reqnroll.Tools.MsBuild.Generation.props\" Condition=\"Exists(\'..\\packages\\Reqnroll.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\Reqnroll.Tools.MsBuild.Generation.props\')\" />");
            projectFileContent.Should().Contain("<Import Project=\"..\\packages\\Reqnroll.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\Reqnroll.Tools.MsBuild.Generation.targets\" Condition=\"Exists(\'..\\packages\\Reqnroll.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\Reqnroll.Tools.MsBuild.Generation.targets\')\" />");
            
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddReferenceToProjectInNewFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);


            project.AddReference("System.Configuration");


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<Reference Include=\"System.Configuration\" />");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddReferenceToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);


            project.AddReference("System.Configuration");


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<Reference Include=\"System.Configuration\" />");
        }


        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddFileToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);

            var projectFile = new ProjectFile("File.cs", "Compile", "//no code");

            project.AddFile(projectFile);


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);
            var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "File.cs");


            projectFileContent.Should().Contain("<Compile Include=\"File.cs\" />");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Contain("//no code");

        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddFileToProjectInNewFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);

            var projectFile = new ProjectFile("File.cs", "Compile", "//no code");

            project.AddFile(projectFile);


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);
            var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "File.cs");


            projectFileContent.Should().NotContain("<Compile");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Contain("//no code");

        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddFileInFolderToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp);

            var projectFile = new ProjectFile(Path.Combine("Folder","File.cs"), "Compile", "//no code");

            project.AddFile(projectFile);


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);
            var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "Folder", "File.cs");


            projectFileContent.Should().Contain("<Compile Include=\"Folder\\File.cs\" />");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Contain("//no code");

        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void AddFileInFolderToProjectInNewFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);

            var projectFile = new ProjectFile(Path.Combine("Folder", "File.cs"), "Compile", "//no code");

            project.AddFile(projectFile);


            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);
            var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "Folder", "File.cs");


            projectFileContent.Should().NotContain("<Compile");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Contain("//no code");

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

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should().Contain("<Project Sdk=\"Microsoft.NET.Sdk\">");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpCore3_1ProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, TargetFramework.Netcoreapp31);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>netcoreapp3.1</TargetFramework>\r\n  </PropertyGroup>\r\n</Project>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpNet50ProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp73, TargetFramework.Net50);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net5.0</TargetFramework>\r\n    <LangVersion>7.3</LangVersion>\r\n  </PropertyGroup>\r\n</Project>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpNet60ProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, TargetFramework.Net60);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                              .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net6.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n</Project>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpNet70ProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, TargetFramework.Net70);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                              .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net7.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n</Project>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpNet80ProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, TargetFramework.Net80);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                              .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net8.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n</Project>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpNet481ProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp73, TargetFramework.Net481);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                              .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net481</TargetFramework>\r\n    <LangVersion>7.3</LangVersion>\r\n  </PropertyGroup>\r\n</Project>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpNet462ProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp73, TargetFramework.Net462);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                              .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net462</TargetFramework>\r\n    <LangVersion>7.3</LangVersion>\r\n  </PropertyGroup>\r\n</Project>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyCSharpNet472ProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp73, TargetFramework.Net472);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                              .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <TargetFramework>net472</TargetFramework>\r\n    <LangVersion>7.3</LangVersion>\r\n  </PropertyGroup>\r\n</Project>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyFSharpProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.FSharp);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);
            projectFileContent.Should().Contain("<Project Sdk=\"Microsoft.NET.Sdk\">");
            projectFileContent.Should().Contain("<TargetFramework>net462</TargetFramework>");
            projectFileContent.Should().Contain("<ItemGroup>\r\n    <Compile Include=\"Library.fs\" />\r\n  </ItemGroup>");
        }

        [SkippableFact(typeof(DotNetSdkNotInstalledException))]
        public void CreateEmptyVbProjectInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.VB);

            new SolutionWriter(new Mock<IOutputWriter>().Object).WriteToFileSystem(solution, solutionFolder);

            string projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should()
                              .Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <RootNamespace>ProjectName</RootNamespace>\r\n    <TargetFramework>net462</TargetFramework>\r\n  </PropertyGroup>\r\n</Project>");
        }
    }

    
}
