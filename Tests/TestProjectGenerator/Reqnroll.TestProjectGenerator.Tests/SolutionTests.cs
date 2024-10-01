using System;
using System.IO;
using FluentAssertions;
using Moq;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.FilesystemWriter;
using Xunit;

namespace Reqnroll.TestProjectGenerator.Tests
{
    public class SolutionTests
    {
        private SolutionWriter CreateSolutionWriter() => new SolutionWriter(new Mock<IOutputWriter>().Object);

        [Fact]
        public void CreateEmptySolution()
        {
            string folder = Path.Combine(Path.GetTempPath(), "Reqnroll.TestProjectGenerator.Tests", Guid.NewGuid().ToString("N"));

            var solution = new Solution("SolutionName");

            var solutionWriter = CreateSolutionWriter();

            solutionWriter.WriteToFileSystem(solution, folder);

            File.Exists(Path.Combine(folder, "SolutionName.sln")).Should().BeTrue();
        }


        [SkippableTheory]
        [InlineData(ProgrammingLanguage.CSharp, "csproj")]
        [InlineData(ProgrammingLanguage.FSharp, "fsproj")]
        [InlineData(ProgrammingLanguage.VB, "vbproj")]

        public void CreateSolutionWithProject(ProgrammingLanguage programmingLanguage, string expectedEnding)
        {
            try
            {
                string folder = Path.Combine(Path.GetTempPath(), "Reqnroll.TestProjectGenerator.Tests", Guid.NewGuid().ToString("N"));

                var solution = new Solution("SolutionName");
                var project = new Project("ProjectName", Guid.NewGuid(), programmingLanguage, TargetFramework.Net462, ProjectFormat.New);

                solution.AddProject(project);

                var solutionWriter = CreateSolutionWriter();

                solutionWriter.WriteToFileSystem(solution, folder);

                File.Exists(Path.Combine(folder, "SolutionName.sln")).Should().BeTrue();
                File.Exists(Path.Combine(folder, "ProjectName", $"ProjectName.{expectedEnding}")).Should().BeTrue();
            }
            catch (DotNetSdkNotInstalledException ex)
            {
                Skip.IfNot(new ConfigurationDriver().PipelineMode, ex.ToString());
            }
        }
    }
}
