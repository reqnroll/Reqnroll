using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.XUnit;
internal class XUnitCSharpTestMethod : CSharpTestMethod
{
    public XUnitCSharpTestMethod(
        IdentifierString identifier,
        ScenarioInformation scenario,
        ImmutableArray<StepInvocation> stepInvocations,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<ParameterDescriptor> parameters = default,
        ImmutableArray<KeyValuePair<string, IdentifierString>> scenarioParameters = default)
        : base(identifier, scenario, stepInvocations, attributes, parameters, scenarioParameters)
    {
    }

    public XUnitCSharpTestMethod(TestMethodDescriptor descriptor) : base(descriptor)
    {
    }

    protected override void RenderTestRunnerLookupTo(
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        // For xUnit test runners are scoped to the whole feature execution lifetime
        sourceBuilder.Append("global::Reqnroll.ITestRunner");

        if (renderingOptions.UseNullableReferenceTypes)
        {
            sourceBuilder.Append('?');
        }
        
        sourceBuilder.AppendLine(" testRunner = _lifetime.TestRunner;");

        sourceBuilder
            .AppendLine("if (testRunner == null)")
            .BeginBlock("{")
            .AppendLine("throw new global::System.InvalidOperationException(\"The test fixture lifecycle has not been initialized.\");")
            .EndBlock("}");
    }
}
