using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.NUnit;
public class NUnitCSharpTestMethod : CSharpTestMethod
{
    public NUnitCSharpTestMethod(
        IdentifierString identifier,
        ScenarioInformation scenario,
        ImmutableArray<StepInvocation> stepInvocations,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<ParameterDescriptor> parameters = default,
        ImmutableArray<KeyValuePair<string, IdentifierString>> scenarioParameters = default)
        : base(identifier, scenario, stepInvocations, attributes, parameters, scenarioParameters)
    {
    }

    public NUnitCSharpTestMethod(TestMethodDescriptor descriptor) : base(descriptor)
    {
    }

    protected override void RenderTestRunnerLookupTo(
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        // For NUnit we use a test-runner assigned to the class.
        sourceBuilder.Append("global::Reqnroll.ITestRunner");

        if (renderingOptions.UseNullableReferenceTypes)
        {
            sourceBuilder.Append('?');
        }

        sourceBuilder.AppendLine(" testRunner = _testRunner;");

        sourceBuilder
            .AppendLine("if (testRunner == null)")
            .BeginBlock("{")
            .AppendLine("throw new global::System.InvalidOperationException(\"TestRunner has not been assigned to the test fixture.\");")
            .EndBlock("}");
    }
}
