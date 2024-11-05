using FluentAssertions;
using Xunit;

namespace Reqnroll.TestProjectGenerator.Tests
{
    public class FeatureFileGeneratorTests
    {
        private readonly FeatureFileGenerator _featureFileGenerator = new FeatureFileGenerator();

        [Fact]
        public void FeatureFileGenerator_ShouldGenerateFeatureFile()
        {
            // ARRANGE
            const string featureFileContent = @"
Feature: Passing Feature
Scenario: Passing Scenario
    Given I have a passing step";

            // ACT
            var featureFile = _featureFileGenerator.Generate(featureFileContent);

            // ASSERT
            featureFile.Content.Should().Be(featureFileContent);
        }

        [Fact]
        public void FeatureFileGenerator_Result_ShouldHaveNoneAction()
        {
            // ARRANGE
            const string featureFileContent = @"
Feature: Passing Feature
Scenario: Passing Scenario
    Given I have a passing step";

            // ACT
            var featureFile = _featureFileGenerator.Generate(featureFileContent);

            // ASSERT
            featureFile.BuildAction.Should().Be("None");
        }
    }
}
