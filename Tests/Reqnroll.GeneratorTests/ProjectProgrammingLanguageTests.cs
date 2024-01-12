using FluentAssertions;
using Reqnroll.Generator;
using Reqnroll.Generator.Project;
using Xunit;

namespace Reqnroll.GeneratorTests
{
    public class ProjectProgrammingLanguageTests
    {
        [Fact]
        public void BuildalyzerLanguageReader_csproj_ShouldReturnCSharp()
        {
            // ARRANGE
            var reader = new ProjectLanguageReader();
            string expected = GenerationTargetLanguage.CSharp;
            string path = @"C:\project\project.csproj";

            // ACT
            string actual = reader.GetLanguage(path);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildalyzerLanguageReader_vbproj_ShouldReturnVB()
        {
            // ARRANGE
            var reader = new ProjectLanguageReader();
            string expected = GenerationTargetLanguage.VB;
            string path = @"C:\project\project.vbproj";

            // ACT
            string actual = reader.GetLanguage(path);

            // ASSERT
            actual.Should().Be(expected);
        }
    }
}
