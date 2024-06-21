using Gherkin;
using Microsoft.CodeAnalysis;
using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.Gherkin;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.MSTest;
public class MSTestCSharpTestFixtureGeneratorTests
{
    public MSTestCSharpTestFixtureGeneratorTests()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(AssemblyIdentity.FromAssemblyDefinition);

        Compilation = new CSharpCompilationInformation(
            "Test.dll",
            references.ToImmutableArray(),
            Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp11,
            true);

        TestHandler = new MSTestHandler();
        Generator = TestHandler.GetTestFixtureGenerator(Compilation);
    }

    private static readonly NamespaceString MSTestNamespace = new("Microsoft.VisualStudio.TestTools.UnitTesting");

    protected CSharpCompilationInformation Compilation { get; }

    protected MSTestHandler TestHandler { get; }

    protected ITestFixtureGenerator Generator { get; }

    [Fact]
    public void GenerateTestMethod_CreatesParameterlessMethodForScenarioWithoutExamples()
    {
        const string featureText =
            """
            #language: en
            @featureTag1
            Feature: Sample

            Scenario: Sample Scenario
                When foo happens
            """;

        var document = new Parser().Parse(new StringReader(featureText));
        var featureSyntax = new GherkinSyntaxTree(document, [], "Sample.feature");

        var featureInfo = new FeatureInformation(
            featureSyntax,
            "Sample.feature",
            "Reqnroll.Tests",
            Compilation,
            TestHandler,
            Generator);

        var scenarioInfo = new ScenarioInformation(
            featureInfo,
            "Sample Scenario",
            [],
            [new ScenarioStep(StepKeywordType.Action, "When", "foo happens", 6)]);

        var method = Generator.GenerateTestMethod(scenarioInfo);

        method.Should().HaveAttribuesEquivalentTo(
            [
                new AttributeDescriptor(new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestMethod"))),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("Description")),
                    ["Sample Scenario"]),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestProperty")),
                    namedArguments: new Dictionary<string, object?>{ { "FeatureTitle", "Sample" } }.ToImmutableDictionary())
            ]);

        method.Should().HaveNoParameters();
    }

        [Fact]
    public void GenerateTestMethod_CreatesMethodWithMSTestDataRowsAttributesWhenScenarioHasExamples()
    {
        const string featureText =
            """
            #language: en
            @featureTag1
            Feature: Sample

            Scenario Outline: Sample Scenario Outline
                When <what> happens
            @example_tag
            Examples:
                | what |
                | foo  |
                | bar  |
            Examples: Second example without tags - in this case the tag list is null.
                | what |
                | baz  |
            """;

        var document = new Parser().Parse(new StringReader(featureText));
        var featureSyntax = new GherkinSyntaxTree(document, [], "Sample.feature");

        var featureInfo = new FeatureInformation(
            featureSyntax,
            "Sample.feature",
            "Reqnroll.Tests",
            Compilation,
            TestHandler,
            Generator);

        var exampleSet1 = new ScenarioExampleSet(["what"], [["foo"], ["bar"]], ["example_tag"]);
        var exampleSet2 = new ScenarioExampleSet(["what"], [["baz"]], []);

        var scenarioInfo = new ScenarioInformation(
            featureInfo,
            "Sample Scenario Outline",
            [],
            [ new ScenarioStep(StepKeywordType.Action, "When", "<what> happens", 6) ],
            [ exampleSet1, exampleSet2 ]);

        var method = Generator.GenerateTestMethod(scenarioInfo);

        method.Should().HaveAttribuesEquivalentTo(
            [
                new AttributeDescriptor(new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestMethod"))),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("Description")),
                    ["Sample Scenario Outline"]),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestProperty")),
                    namedArguments: new Dictionary<string, object?>{ { "FeatureTitle", "Sample" } }.ToImmutableDictionary()),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("DataRow")),
                    ["foo", ImmutableArray.Create<object?>(ImmutableArray.Create("example_tag"))]),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("DataRow")),
                    ["bar", ImmutableArray.Create<object?>(ImmutableArray.Create("example_tag"))]),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("DataRow")),
                    ["baz", ImmutableArray.Create<object?>(ImmutableArray<string>.Empty)])
            ]);

        method.Should().HaveParametersEquivalentTo(
            [
                new ParameterDescriptor(
                    new IdentifierString("what"), 
                    new NamedTypeIdentifier(new NamespaceString("System"), new IdentifierString("String"))),

                new ParameterDescriptor(
                    new IdentifierString("_tags"),
                    new ArrayTypeIdentifier(new NamedTypeIdentifier(new NamespaceString("System"), new IdentifierString("String"))))
            ]);
    }
}
