using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.XUnit;
public class XUnitCSharpTestFixtureGeneratorTests() : CSharpTestFixtureGeneratorTestBase<XUnitHandler>(new XUnitHandler())
{
    private static readonly NamespaceString XUnitNamespace = new("Xunit");

    [Fact]
    public void GenerateTestFixture_CreatesClassForFeatureWithXUnitLifetimeInterface()
    {
        var featureInfo = new FeatureInformation(
            "Sample",
            null,
            "en",
            ["featureTag1"],
            null);

        var scenarioInfo = new ScenarioInformation(
            "Sample Scenario",
            new FileLinePositionSpan("Sample.feature", new LinePosition(3, 0), new LinePosition(3, 24)),
            [],
            [
                new Step(
                    StepType.Action,
                    "When",
                    "foo happens",
                    new FileLinePositionSpan("Sample.feature", new LinePosition(4, 4), new LinePosition(4, 20)))
            ]);

        var testFixtureGenerationContext = new TestFixtureGenerationContext<CSharpCompilationInformation>(
            featureInfo,
            [scenarioInfo],
            "Sample.feature",
            new NamespaceString("Reqnroll.Tests"),
            Compilation,
            Generator);

        var testFixture = Generator.GenerateTestFixtureClass(testFixtureGenerationContext, []);

        testFixture.Interfaces.Should().BeEquivalentTo(
            [
                XUnitNamespace + new GenericTypeIdentifier(
                    new IdentifierString("IClassFixture"),
                    [
                        new NestedTypeIdentifier(
                            new SimpleTypeIdentifier(new IdentifierString("SampleFeature")),
                            new SimpleTypeIdentifier(new IdentifierString("FeatureLifetime")))
                    ])
            ]);
    }

    [Fact]
    public void GenerateTestMethod_CreatesParameterlessMethodForScenarioWithoutExamples()
    {
        var featureInfo = new FeatureInformation(
            "Sample",
            null,
            "en",
            ["featureTag1"],
            null);

        var scenarioInfo = new ScenarioInformation(
            "Sample Scenario",
            new FileLinePositionSpan("Sample.feature", new LinePosition(3, 0), new LinePosition(3, 24)),
            [],
            [
                new Step(
                    StepType.Action,
                    "When",
                    "foo happens",
                    new FileLinePositionSpan("Sample.feature", new LinePosition(4, 4), new LinePosition(4, 20)))
            ]);

        var testFixtureGenerationContext = new TestFixtureGenerationContext<CSharpCompilationInformation>(
            featureInfo,
            [scenarioInfo],
            "Sample.feature",
            new NamespaceString("Reqnroll.Tests"),
            Compilation,
            Generator);

        var testMethodGenerationContext = new TestMethodGenerationContext<CSharpCompilationInformation>(
            scenarioInfo,
            testFixtureGenerationContext);

        var method = Generator.GenerateTestMethod(testMethodGenerationContext);

        method.Should().HaveAttribuesEquivalentTo(
            [
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("SkippableFact")),
                    namedArguments: new Dictionary<IdentifierString, object?>{ 
                        { new IdentifierString("DisplayName"), "Sample Scenario" } 
                    }.ToImmutableDictionary()),
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Trait")),
                    ["FeatureTitle", "Sample"]),
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Trait")),
                    ["Description", "Sample Scenario"])
            ]);

        method.Should().HaveNoParameters();

        method.StepInvocations.Should().BeEquivalentTo(
            [
                new StepInvocation(
                    StepType.Action,
                    new FileLinePositionSpan("Sample.feature", new LinePosition(4, 4), new LinePosition(4, 20)), 
                    "When", 
                    "foo happens")
            ]);
    }

    [Fact]
    public void GenerateTestMethod_CreatesMethodWithInlineDataAttributesWhenScenarioHasExamples()
    {
        var exampleSet1 = new ScenarioExampleSet(["what"], [["foo"], ["bar"]], ["example_tag"]);
        var exampleSet2 = new ScenarioExampleSet(["what"], [["baz"]], []);

        var scenarioInfo = new ScenarioInformation(
            "Sample Scenario Outline",
            new FileLinePositionSpan("Sample.feature", new LinePosition(3, 0), new LinePosition(3, 24)),
            [],
            [
                new Step(
                    StepType.Action,
                    "When",
                    "<what> happens",
                    new FileLinePositionSpan("Sample.feature", new LinePosition(4, 4), new LinePosition(4, 20)))
            ],
            [exampleSet1, exampleSet2]);

        var featureInfo = new FeatureInformation(
            "Sample",
            null,
            "en",
            ["featureTag1"],
            null);

        var testFixtureGenerationContext = new TestFixtureGenerationContext<CSharpCompilationInformation>(
            featureInfo,
            [scenarioInfo],
            "Sample.feature",
            new NamespaceString("Reqnroll.Tests"),
            Compilation,
            Generator);

        var testMethodGenerationContext = new TestMethodGenerationContext<CSharpCompilationInformation>(
            scenarioInfo,
            testFixtureGenerationContext);

        var method = Generator.GenerateTestMethod(testMethodGenerationContext);

        method.Should().HaveAttribuesEquivalentTo(
            [
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("SkippableTheory")),
                    namedArguments: new Dictionary<IdentifierString, object?>{
                        { new IdentifierString("DisplayName"), "Sample Scenario Outline" }
                    }.ToImmutableDictionary()),
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Trait")),
                    ["FeatureTitle", "Sample"]),
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Trait")),
                    ["Description", "Sample Scenario Outline"]),
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("InlineData")),
                    ["foo", ImmutableArray.Create("example_tag")]),
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("InlineData")),
                    ["bar", ImmutableArray.Create("example_tag")]),
                new AttributeDescriptor(
                    XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("InlineData")),
                    ["baz", ImmutableArray<string>.Empty])
            ]);

        method.Should().HaveParametersEquivalentTo(
            [
                new ParameterDescriptor(
                    new IdentifierString("what"),
                    new NamespaceString("System") + new SimpleTypeIdentifier(new IdentifierString("String"))),

                new ParameterDescriptor(
                    new IdentifierString("_exampleTags"),
                    new ArrayTypeIdentifier(new NamespaceString("System") + new SimpleTypeIdentifier(new IdentifierString("String"))))
            ]);

        method.StepInvocations.Should().BeEquivalentTo(
            [
                new StepInvocation(
                    StepType.Action,
                    new FileLinePositionSpan("Sample.feature", new LinePosition(4, 4), new LinePosition(4, 20)), 
                    "When", 
                    "{0} happens", 
                    [new IdentifierString("what")])
            ]);
    }
}
