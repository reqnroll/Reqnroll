using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.MSTest;
public class MSTestCSharpTestMethod(
    IdentifierString identifier,
    ScenarioInformation scenario,
    ImmutableArray<AttributeDescriptor> attributes = default,
    ImmutableArray<ParameterDescriptor> parameters = default,
    ImmutableArray<KeyValuePair<string, IdentifierString>> scenarioParameters = default) : 
    CSharpTestMethod(identifier, scenario, attributes, parameters, scenarioParameters)
{
    protected override void RenderTestRunnerLookupTo(
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        // For MSTest we use a test-runner assigned to the class.
        sourceBuilder
            .AppendLine("global::Reqnroll.ITestRunner testRunner;")
            .AppendLine("if (TestRunner == null)")
            .BeginBlock("{")
            .AppendLine("throw new global::System.InvalidOperationException(\"TestRunner has not been assigned to the test fixture.\");")
            .EndBlock("}")
            .AppendLine("testRunner = TestRunner;");
    }
}
