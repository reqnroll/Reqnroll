
using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.XUnit;

internal class XUnitCSharpTestFixtureGenerator(XUnitHandler testFrameworkHandler) :
    CSharpTestFixtureGenerator<CSharpTestFixtureClass, CSharpTestMethod>(testFrameworkHandler)
{
    protected override CSharpTestFixtureClass CreateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        NamedTypeIdentifier identifier,
        FeatureInformation feature,
        ImmutableArray<AttributeDescriptor> attributes,
        ImmutableArray<CSharpTestMethod> methods,
        CSharpRenderingOptions renderingOptions)
    {
        throw new NotImplementedException();
    }

    protected override CSharpTestMethod CreateTestMethod(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        IdentifierString identifier,
        ScenarioInformation scenario,
        ImmutableArray<AttributeDescriptor> attributes,
        ImmutableArray<ParameterDescriptor> parameters,
        ImmutableArray<KeyValuePair<string, IdentifierString>> scenarioParameters)
    {
        throw new NotImplementedException();
    }

    protected override ImmutableArray<AttributeDescriptor> GenerateTestFixtureClassAttributes(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override ImmutableArray<AttributeDescriptor> GenerateTestMethodAttributes(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
