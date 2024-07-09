using Reqnroll.FeatureSourceGenerator.SourceModel;
using Reqnroll.FeatureSourceGenerator.XUnit;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.XUnit;

internal class XUnitCSharpTestFixtureGenerator(XUnitHandler testFrameworkHandler) :
    CSharpTestFixtureGenerator<XUnitCSharpTestFixtureClass, XUnitCSharpTestMethod>(testFrameworkHandler)
{
    protected override XUnitCSharpTestFixtureClass CreateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        TestFixtureDescriptor descriptor,
        ImmutableArray<XUnitCSharpTestMethod> methods,
        CSharpRenderingOptions renderingOptions)
    {
        return new XUnitCSharpTestFixtureClass(
            descriptor,
            methods.Cast<CSharpTestMethod>().ToImmutableArray(),
            renderingOptions);
    }

    protected override XUnitCSharpTestMethod CreateTestMethod(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        TestMethodDescriptor descriptor)
    {
        return new XUnitCSharpTestMethod(descriptor);
    }

    protected override ImmutableArray<AttributeDescriptor> GenerateTestMethodAttributes(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        return ImmutableArray.Create(
            XUnitSyntax.SkippableFactAttribute(context.ScenarioInformation.Name),
            XUnitSyntax.TraitAttribute("FeatureTitle", context.FeatureInformation.Name),
            XUnitSyntax.TraitAttribute("Description", context.ScenarioInformation.Name));
    }
}
