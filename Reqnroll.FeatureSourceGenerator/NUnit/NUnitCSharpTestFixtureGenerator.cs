using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

internal class NUnitCSharpTestFixtureGenerator(NUnitHandler testFrameworkHandler) : 
    CSharpTestFixtureGenerator<CSharpTestFixtureClass, CSharpTestMethod>(testFrameworkHandler)
{
    protected override CSharpTestFixtureClass CreateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        TestFixtureDescriptor descriptor,
        ImmutableArray<CSharpTestMethod> methods,
        CSharpRenderingOptions renderingOptions)
    {
        throw new NotImplementedException();
    }

    protected override CSharpTestMethod CreateTestMethod(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        TestMethodDescriptor descriptor)
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
