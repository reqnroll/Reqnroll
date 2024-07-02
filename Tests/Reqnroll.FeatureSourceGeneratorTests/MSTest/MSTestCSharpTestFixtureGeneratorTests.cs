using Gherkin;
using Microsoft.CodeAnalysis;
using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.SourceModel;
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
        Generator = TestHandler.GetTestFixtureGenerator<CSharpCompilationInformation>()!;
    }

    private static readonly NamespaceString MSTestNamespace = new("Microsoft.VisualStudio.TestTools.UnitTesting");

    protected CSharpCompilationInformation Compilation { get; }

    protected MSTestHandler TestHandler { get; }

    protected ITestFixtureGenerator<CSharpCompilationInformation> Generator { get; }

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
                new AttributeDescriptor(new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestClass"))),
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
                new AttributeDescriptor(new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestMethod"))),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("Description")),
                    ["Sample Scenario"]),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestProperty")),
                    positionalArguments: ["FeatureTitle", "Sample"])
            ]);

        method.Should().HaveNoParameters();

        method.StepInvocations.Should().BeEquivalentTo(
            [
                new StepInvocation(StepType.Action, 6, "When", "foo happens")
            ]);
    }

    [Fact]
    public void GenerateTestMethod_CreatesMethodWithMSTestDataRowsAttributesWhenScenarioHasExamples()
    {
        var exampleSet1 = new ScenarioExampleSet(["what"], [["foo"], ["bar"]], ["example_tag"]);
        var exampleSet2 = new ScenarioExampleSet(["what"], [["baz"]], []);

        var scenarioInfo = new ScenarioInformation(
            "Sample Scenario Outline",
            22,
            [],
            [ new ScenarioStep(StepType.Action, "When", "<what> happens", 6) ],
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
                new AttributeDescriptor(new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestMethod"))),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("Description")),
                    ["Sample Scenario Outline"]),
                new AttributeDescriptor(
                    new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestProperty")),
                    positionalArguments: ["FeatureTitle", "Sample"]),
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
                    new IdentifierString("_exampleTags"),
                    new ArrayTypeIdentifier(new NamedTypeIdentifier(new NamespaceString("System"), new IdentifierString("String"))))
            ]);

        method.StepInvocations.Should().BeEquivalentTo(
            [
                new StepInvocation(StepType.Action, 6, "When", "{0} happens", [new IdentifierString("what")])
            ]);
    }
}
