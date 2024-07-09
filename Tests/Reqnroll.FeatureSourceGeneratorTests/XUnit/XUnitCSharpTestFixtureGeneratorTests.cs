using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.XUnit;
public class XUnitCSharpTestFixtureGeneratorTests() : CSharpTestFixtureGeneratorTestBase<XUnitHandler>(new XUnitHandler())
{
    private static readonly NamespaceString XUnitNamespace = new("Xunit");

    [Fact]
    public void GenerateTestFixture_CreatesClassForFeatureWithXUnitLifecycleInterface()
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
                            new SimpleTypeIdentifier(new IdentifierString("Lifecycle")))
                    ])
            ]);
    }
}
