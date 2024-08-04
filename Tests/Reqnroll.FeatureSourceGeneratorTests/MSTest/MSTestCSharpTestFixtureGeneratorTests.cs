using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.MSTest;
public class MSTestCSharpTestFixtureGeneratorTests() : CSharpTestFixtureGeneratorTestBase<MSTestHandler>(new MSTestHandler())
{
    private static readonly NamespaceString MSTestNamespace = new("Microsoft.VisualStudio.TestTools.UnitTesting");

    [Fact]
    public void GenerateTestFixture_CreatesClassForFeatureWithMsTestAttributes()
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
                new ScenarioStep(
                    StepType.Action,
                    "When",
                    "foo happens",
                    new FileLinePositionSpan("Sample.feature", new LinePosition(4, 4), new LinePosition(4, 20)))
            ]);

        var testFixtureGenerationContext = new TestFixtureGenerationContext<CSharpCompilationInformation>(
            featureInfo,
            [ scenarioInfo ],
            "Sample.feature",
            new NamespaceString("Reqnroll.Tests"),
            Compilation,
            Generator);

        var testFixture = Generator.GenerateTestFixtureClass(testFixtureGenerationContext, []);

        testFixture.Should().HaveAttribuesEquivalentTo(
            [
                new AttributeDescriptor(MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("TestClass"))),
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
                new ScenarioStep(
                    StepType.Action,
                    "When",
                    "foo happens",
                    new FileLinePositionSpan("Sample.feature", new LinePosition(4, 4), new LinePosition(4, 20)))
            ]);

        var testFixtureGenerationContext = new TestFixtureGenerationContext<CSharpCompilationInformation>(
            featureInfo,
            [ scenarioInfo ],
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
                new AttributeDescriptor(MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("TestMethod"))),
                new AttributeDescriptor(
                    MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("Description")),
                    ["Sample Scenario"]),
                new AttributeDescriptor(
                    MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("TestProperty")),
                    positionalArguments: ["FeatureTitle", "Sample"])
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
    public void GenerateTestMethod_CreatesMethodWithMSTestDataRowsAttributesWhenScenarioHasExamples()
    {
        var exampleSet1 = new ScenarioExampleSet(["what"], [["foo"], ["bar"]], ["example_tag"]);
        var exampleSet2 = new ScenarioExampleSet(["what"], [["baz"]], []);

        var scenarioInfo = new ScenarioInformation(
            "Sample Scenario Outline",
            new FileLinePositionSpan("Sample.feature", new LinePosition(3, 0), new LinePosition(3, 24)),
            [],
            [
                new ScenarioStep(
                    StepType.Action,
                    "When",
                    "<what> happens",
                    new FileLinePositionSpan("Sample.feature", new LinePosition(4, 4), new LinePosition(4, 20)))
            ],
            [ exampleSet1, exampleSet2 ]);

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
                new AttributeDescriptor(MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("TestMethod"))),
                new AttributeDescriptor(
                    MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("Description")),
                    ["Sample Scenario Outline"]),
                new AttributeDescriptor(
                    MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("TestProperty")),
                    positionalArguments: ["FeatureTitle", "Sample"]),
                new AttributeDescriptor(
                    MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("DataRow")),
                    ["foo", ImmutableArray.Create<object?>(ImmutableArray.Create("example_tag"))]),
                new AttributeDescriptor(
                    MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("DataRow")),
                    ["bar", ImmutableArray.Create<object?>(ImmutableArray.Create("example_tag"))]),
                new AttributeDescriptor(
                    MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("DataRow")),
                    ["baz", ImmutableArray.Create<object?>(ImmutableArray<string>.Empty)])
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
