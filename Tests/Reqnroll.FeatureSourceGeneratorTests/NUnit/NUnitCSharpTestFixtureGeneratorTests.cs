using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.NUnit;
public class NUnitCSharpTestFixtureGeneratorTests() : CSharpTestFixtureGeneratorTestBase<NUnitHandler>(new NUnitHandler())
{
    private static readonly NamespaceString NUnitNamespace = new("NUnit.Framework");

    [Fact]
    public void GenerateTestFixture_CreatesClassForFeatureWithNUnitAttributes()
    {
        var featureInfo = new FeatureInformation(
            "Sample",
            null,
            "en",
            ["featureTag1"],
            null);

        var scenarioInfo = new ScenarioInformation(
            "Sample Scenario",
            22,
            [],
            [new ScenarioStep(StepType.Action, "When", "foo happens", 6)]);

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
                new AttributeDescriptor(
                    NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Description")),
                    ["Sample"]),
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
            22,
            [],
            [new ScenarioStep(StepType.Action, "When", "foo happens", 6)]);

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
                new AttributeDescriptor(NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Test"))),
                new AttributeDescriptor(
                    NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Description")),
                    ["Sample Scenario"])
            ]);

        method.Should().HaveNoParameters();

        method.StepInvocations.Should().BeEquivalentTo(
            [
                new StepInvocation(StepType.Action, 6, "When", "foo happens")
            ]);
    }

    [Fact]
    public void GenerateTestMethod_CreatesMethodWithTestCaseAttributesWhenScenarioHasExamples()
    {
        var exampleSet1 = new ScenarioExampleSet(["what"], [["foo"], ["bar"]], ["example_tag"]);
        var exampleSet2 = new ScenarioExampleSet(["what"], [["baz"]], []);

        var scenarioInfo = new ScenarioInformation(
            "Sample Scenario Outline",
            22,
            [],
            [new ScenarioStep(StepType.Action, "When", "<what> happens", 6)],
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
                    NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Description")),
                    ["Sample Scenario Outline"]),
                new AttributeDescriptor(
                    NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("TestCase")),
                    ["foo", ImmutableArray.Create("example_tag")],
                    new Dictionary<IdentifierString, object?>{{ new IdentifierString("Category"), "example_tag" } }.ToImmutableDictionary()),
                new AttributeDescriptor(
                    NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("TestCase")),
                    ["bar", ImmutableArray.Create("example_tag")],
                    new Dictionary<IdentifierString, object?>{{ new IdentifierString("Category"), "example_tag" } }.ToImmutableDictionary()),
                new AttributeDescriptor(
                    NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("TestCase")),
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
                new StepInvocation(StepType.Action, 6, "When", "{0} happens", [new IdentifierString("what")])
            ]);
    }
}
