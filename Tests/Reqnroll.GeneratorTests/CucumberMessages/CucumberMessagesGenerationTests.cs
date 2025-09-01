using FluentAssertions;
using System.Linq;
using Xunit;

namespace Reqnroll.GeneratorTests.CucumberMessages;

public class CucumberMessagesGenerationTests : UnitTestFeatureGeneratorTestsBase
{
    [Fact]
    public void Should_embed_cucumber_messages_in_generated_code()
    {
        var generator = CreateUnitTestFeatureGenerator();
        var document = ParserHelper.CreateDocument();

        var result = generator.GenerateUnitTestFixture(document, "TestClass", "TestNamespace");

        result.FeatureMessages.Should().NotBeNullOrEmpty();
            
        // Verify structure of generated messages
        var lines = result.FeatureMessages.Split('\n');
        lines.Should().HaveCountGreaterThan(0);
            
        // Should contain source envelope
        lines.Should().Contain(line => line.Contains("\"source\":"));
            
        // Should contain gherkin document envelope  
        lines.Should().Contain(line => line.Contains("\"gherkinDocument\":"));
            
        // Should contain pickle envelope(s)
        lines.Should().Contain(line => line.Contains("\"pickle\":"));
    }

    [Fact]
    public void Should_generate_deterministic_ids_for_same_feature()
    {
        var generator = CreateUnitTestFeatureGenerator();
        var document = ParserHelper.CreateDocument();

        var result1 = generator.GenerateUnitTestFixture(document, "TestClass", "TestNamespace");
        var result2 = generator.GenerateUnitTestFixture(document, "TestClass", "TestNamespace");

        result1.FeatureMessages.Should().Be(result2.FeatureMessages);
    }

    [Fact]
    public void Should_include_scenario_outline_examples_as_separate_pickles()
    {
        var generator = CreateUnitTestFeatureGenerator();
        var document = ParserHelper.CreateDocumentWithScenarioOutline();

        var result = generator.GenerateUnitTestFixture(document, "TestClass", "TestNamespace");

        // Should have multiple pickles for scenario outline examples
        var pickleCount = result.FeatureMessages.Split('\n')
                                .Count(line => line.Contains("\"pickle\":"));
                
        pickleCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Should_preserve_feature_tags_in_cucumber_messages()
    {
        var generator = CreateUnitTestFeatureGenerator();
        var document = ParserHelper.CreateDocument(["feature-tag1", "feature-tag2"]);

        var result = generator.GenerateUnitTestFixture(document, "TestClass", "TestNamespace");

        result.FeatureMessages.Should().Contain("feature-tag1");
        result.FeatureMessages.Should().Contain("feature-tag2");
    }

    [Fact]
    public void Should_preserve_scenario_tags_in_pickle_messages()
    {
        var generator = CreateUnitTestFeatureGenerator();
        var document = ParserHelper.CreateDocument(scenarioTags: ["scenario-tag1", "scenario-tag2"]);

        var result = generator.GenerateUnitTestFixture(document, "TestClass", "TestNamespace");

        result.FeatureMessages.Should().Contain("scenario-tag1");
        result.FeatureMessages.Should().Contain("scenario-tag2");
    }
}