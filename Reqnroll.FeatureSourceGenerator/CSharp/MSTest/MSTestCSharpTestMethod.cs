using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.MSTest;
public class MSTestCSharpTestMethod : CSharpTestMethod
{
    public MSTestCSharpTestMethod(
        IdentifierString identifier,
        ScenarioInformation scenario,
        ImmutableArray<StepInvocation> stepInvocations,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<ParameterDescriptor> parameters = default,
        ImmutableArray<KeyValuePair<string, IdentifierString>> scenarioParameters = default) 
        : base(identifier, scenario, stepInvocations, attributes, parameters, scenarioParameters)
    {
    }

    public MSTestCSharpTestMethod(TestMethodDescriptor descriptor) : base(descriptor)
    {
    }

    protected override void RenderTestRunnerLookupTo(
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        // For MSTest we use a test-runner assigned to the class.
        sourceBuilder
            .AppendLine("global::Reqnroll.ITestRunner testRunner = TestRunner;")
            .AppendLine("if (testRunner == null)")
            .BeginBlock("{")
            .AppendLine("throw new global::System.InvalidOperationException(\"TestRunner has not been assigned to the test fixture.\");")
            .EndBlock("}");
    }
}
