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
        CSharpSourceTextWriter writer,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        // For NUnit we use a test-runner assigned to the class.
        writer.Write("global::Reqnroll.ITestRunner");

        if (renderingOptions.UseNullableReferenceTypes)
        {
            writer.Write('?');
        }

        writer.WriteLine(" testRunner = _testRunner;");

        writer
            .WriteLine("if (testRunner == null)")
            .BeginBlock("{")
            .WriteLine("throw new global::System.InvalidOperationException(\"TestRunner has not been assigned to the test fixture.\");")
            .EndBlock("}");
    }
}
