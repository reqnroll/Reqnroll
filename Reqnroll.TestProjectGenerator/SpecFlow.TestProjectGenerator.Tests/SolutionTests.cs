using System;
using System.IO;
using FluentAssertions;
using Moq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter;
using Xunit;

namespace TechTalk.SpecFlow.TestProjectGenerator.Tests
{
    public class SolutionTests
    {
        [Fact]
        public void CreateEmptySolution()
        {
            string folder = Path.Combine(Path.GetTempPath(), "SpecFlow.TestProjectGenerator.Tests", Guid.NewGuid().ToString("N"));

            var solution = new Solution("SolutionName");

            var solutionWriter = new SolutionWriter(new Mock<IOutputWriter>().Object);

            solutionWriter.WriteToFileSystem(solution, folder);

            File.Exists(Path.Combine(folder, "SolutionName.sln")).Should().BeTrue();
        }


        [Theory]
        [InlineData(ProgrammingLanguage.CSharp, "csproj")]
        [InlineData(ProgrammingLanguage.FSharp, "fsproj")]
        [InlineData(ProgrammingLanguage.VB, "vbproj")]

        public void CreateSolutionWithProject(ProgrammingLanguage programmingLanguage, string expectedEnding)
        {
            string folder = Path.Combine(Path.GetTempPath(), "SpecFlow.TestProjectGenerator.Tests", Guid.NewGuid().ToString("N"));

            var solution = new Solution("SolutionName");
            var project = new Project("ProjectName", Guid.NewGuid(), programmingLanguage, TargetFramework.Net462, ProjectFormat.New);

            solution.AddProject(project);

            var solutionWriter = new SolutionWriter(new Mock<IOutputWriter>().Object);

            solutionWriter.WriteToFileSystem(solution, folder);

            File.Exists(Path.Combine(folder, "SolutionName.sln")).Should().BeTrue();
            File.Exists(Path.Combine(folder, "ProjectName", $"ProjectName.{expectedEnding}")).Should().BeTrue();
        }
    }
}
