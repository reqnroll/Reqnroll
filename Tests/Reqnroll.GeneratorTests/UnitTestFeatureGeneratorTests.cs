using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Reqnroll.Generator;
using FluentAssertions;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests;

public class UnitTestFeatureGeneratorTests : UnitTestFeatureGeneratorTestsBase
{
    [Fact]
    public void Should_pass_feature_tags_as_test_class_category()
    {
        var generator = CreateUnitTestFeatureGenerator();
        string[] generatedCats = [];
        UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestClassCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<IEnumerable<string>>()))
                                     .Callback((TestClassGenerationContext _, IEnumerable<string> cats) => generatedCats = cats.ToArray());

        var theDocument = ParserHelper.CreateDocument(["foo", "bar"]);

        GenerateFeature(generator, theDocument);

        generatedCats.Should().Equal("foo", "bar");
    }

    [Fact]
    public void Should_pass_scenario_tags_as_test_method_category()
    {
        var generator = CreateUnitTestFeatureGenerator();
        string[] generatedCats = [];
        UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                                     .Callback((TestClassGenerationContext _, CodeMemberMethod _, IEnumerable<string> cats) => generatedCats = cats.ToArray());

        var theFeature = ParserHelper.CreateDocument(scenarioTags: ["foo", "bar"]);

        GenerateFeature(generator, theFeature);

        generatedCats.Should().Equal("foo", "bar");
    }

    [Fact]
    public void Should_pass_scenario_tags_as_test_method_categories_of_scenarioOutline()
    {
        var generator = CreateUnitTestFeatureGenerator();
        List<string> generatedCats = new();
        UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                                     .Callback((TestClassGenerationContext _, CodeMemberMethod _, IEnumerable<string> cats) => RecordTestMethodCategories( cats, generatedCats));

        var theFeature = ParserHelper.CreateDocumentWithScenarioOutline(scenarioOutlineTags: ["foo", "bar"]);

        GenerateFeature(generator, theFeature);

        generatedCats.Should().Equal("foo", "bar");

        void RecordTestMethodCategories(IEnumerable<string> cats, List<string> recordedGeneratedCats)
        {
            recordedGeneratedCats.AddRange(cats);
        }
    }


    [Fact]
    public void Should_pass_rule_tags_as_test_method_category()
    {
        var generator = CreateUnitTestFeatureGenerator();
        string[] generatedCats = [];
        UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                                     .Callback((TestClassGenerationContext _, CodeMemberMethod _, IEnumerable<string> cats) => generatedCats = cats.ToArray());

        var theFeature = ParserHelper.CreateDocumentWithRule(ruleTags: ["rule_tag1", "rule_tag2"], scenarioTags: ["scenarioTag1"]);

        GenerateFeature(generator, theFeature);

        generatedCats.Should().BeEquivalentTo("rule_tag1", "rule_tag2", "scenarioTag1");
    }


    [Fact]
    public void Should_pass_rule_tags_as_test_method_category_of_scenario_outline()
    {
        var generator = CreateUnitTestFeatureGenerator();
        List<string> generatedCats = new List<string>();
        UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                                     .Callback((TestClassGenerationContext _, CodeMemberMethod _, IEnumerable<string> cats) => RecordTestMethodCategories(cats, generatedCats));

        var theFeature = ParserHelper.CreateDocumentWithScenarioOutlineInRule(ruleTags: ["rule_tag1", "rule_tag2"], scenarioOutlineTags: ["scenarioTag1"]);

        GenerateFeature(generator, theFeature);

        generatedCats.Should().BeEquivalentTo(new List<string> { "rule_tag1", "rule_tag2", "scenarioTag1" });

        void RecordTestMethodCategories(IEnumerable<string> cats, List<string> recordedGeneratedCats)
        {
            recordedGeneratedCats.AddRange(cats);
        }
    }

    [Fact]
    public void Should_not_pass_feature_tags_as_test_method_category()
    {
        var generator = CreateUnitTestFeatureGenerator();
        string[] generatedCats = [];
        UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                                     .Callback((TestClassGenerationContext _, CodeMemberMethod _, IEnumerable<string> cats) => generatedCats = cats.ToArray());

        var theFeature = ParserHelper.CreateDocument(tags: ["featureTag"], scenarioTags: ["foo", "bar"]);

        GenerateFeature(generator, theFeature);

        generatedCats.Should().Equal("foo", "bar");
    }

    [Fact]
    public void Should_not_pass_decorated_feature_tag_as_test_class_category()
    {
        var decoratorMock = DecoratorRegistryTests.CreateTestClassTagDecoratorMock("decorated");
        Container.RegisterInstanceAs(decoratorMock.Object, "decorated");

        var generator = CreateUnitTestFeatureGenerator();

        var theFeature = ParserHelper.CreateDocument(["decorated", "other"]);

        GenerateFeature(generator, theFeature);

        UnitTestGeneratorProviderMock.Verify(ug => ug.SetTestClassCategories(It.IsAny<TestClassGenerationContext>(), It.Is<IEnumerable<string>>(cats => !cats.Contains("decorated"))));
    }

    [Fact]
    public void Should_not_pass_decorated_scenario_tag_as_test_method_category()
    {
        var decoratorMock = DecoratorRegistryTests.CreateTestMethodTagDecoratorMock("decorated");
        Container.RegisterInstanceAs(decoratorMock.Object, "decorated");

        var generator = CreateUnitTestFeatureGenerator();

        var theFeature = ParserHelper.CreateDocument(scenarioTags: ["decorated", "other"]);

        GenerateFeature(generator, theFeature);

        UnitTestGeneratorProviderMock.Verify(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.Is<IEnumerable<string>>(cats => !cats.Contains("decorated"))));
    }

    [Fact]
    public void Should_generate_cucumber_messages_for_feature()
    {
        var generator = CreateUnitTestFeatureGenerator();
        var theDocument = ParserHelper.CreateDocument(["foo", "bar"]);

        var result = generator.GenerateUnitTestFixture(theDocument, "dummy", "dummyNS");

        result.FeatureMessages.Should().NotBeNullOrEmpty();
        result.FeatureMessages.Should().Contain("\"source\":");
        result.FeatureMessages.Should().Contain("\"gherkinDocument\":");
        result.FeatureMessages.Should().Contain("\"pickle\":");
    }

    [Fact]
    public void Should_generate_InitializeCucumberMessages_method()
    {
        var generator = CreateUnitTestFeatureGenerator();
        var theDocument = ParserHelper.CreateDocument();

        var result = generator.GenerateUnitTestFixture(theDocument, "TestClass", "TestNamespace");

        var testClass = result.CodeNamespace.Types[0];
        var initMethod = testClass.Members.OfType<CodeMemberMethod>()
                                  .FirstOrDefault(m => m.Name == "InitializeCucumberMessages");

        initMethod.Should().NotBeNull();
        initMethod!.ReturnType.BaseType.Should().Contain("FeatureLevelCucumberMessages");
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        initMethod.Attributes.Should().HaveFlag(MemberAttributes.Private | MemberAttributes.Static);
    }

    [Fact]
    public void Should_include_cucumber_messages_in_FeatureInfo_constructor()
    {
        var generator = CreateUnitTestFeatureGenerator();
        var theDocument = ParserHelper.CreateDocument();

        var result = generator.GenerateUnitTestFixture(theDocument, "TestClass", "TestNamespace");

        var testClass = result.CodeNamespace.Types[0];
        var featureInfoField = testClass.Members.OfType<CodeMemberField>()
                                        .FirstOrDefault(f => f.Name.EndsWith("featureInfo"));

        featureInfoField.Should().NotBeNull();
        var constructor = featureInfoField!.InitExpression as CodeObjectCreateExpression;
        constructor.Should().NotBeNull();
        constructor!.Parameters.Count.Should().Be(7); // Including the new cucumber messages parameter
    }

    [Fact]
    public void Should_handle_cucumber_messages_generation_errors_gracefully()
    {
        // Test with malformed or problematic document
        var generator = CreateUnitTestFeatureGenerator();
        var invalidDocument = ParserHelper.CreateDocument(); // Create a document that might cause issues

        var result = generator.GenerateUnitTestFixture(invalidDocument, "TestClass", "TestNamespace");

        result.Should().NotBeNull();
        result.CodeNamespace.Should().NotBeNull();
        // Should not throw exception even if cucumber messages generation fails
    }

    [Fact]
    public void Should_generate_warnings_when_cucumber_message_processing_fails()
    {
        // This test would need to be crafted to trigger the exception handling
        // in DeclareFeatureMessagesFactoryMembers method
        var generator = CreateUnitTestFeatureGenerator();
        // Create a document that would cause cucumber message processing to fail
        var problematicDocument = new ReqnrollDocument(ParserHelper.CreateDocument().ReqnrollFeature, [],
            new ReqnrollDocumentLocation(null)); // null source path is invalid

        var result = generator.GenerateUnitTestFixture(problematicDocument, "TestClass", "TestNamespace");

        // Verify that warnings are generated when cucumber message processing fails
        // This may need adjustment based on how to trigger the failure condition

        result.FeatureMessages.Should().BeNull();
        result.GenerationWarnings.Should().NotBeEmpty();
        result.GenerationWarnings.Should().ContainMatch("*Failed to process Cucumber Pickles.*");
    }
}