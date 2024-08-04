using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.XUnit;
public class XUnitCSharpTestMethod : CSharpTestMethod
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
        CSharpSourceTextWriter writer,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        // For xUnit test runners are scoped to the whole feature execution lifetime
        writer.Write("global::Reqnroll.ITestRunner");

        if (renderingOptions.UseNullableReferenceTypes)
        {
            writer.Write('?');
        }
        
        writer.WriteLine(" testRunner = _lifetime.TestRunner;");

        writer
            .WriteLine("if (testRunner == null)")
            .BeginBlock("{")
            .WriteLine("throw new global::System.InvalidOperationException(\"The test fixture lifecycle has not been initialized.\");")
            .EndBlock("}");
    }
}
