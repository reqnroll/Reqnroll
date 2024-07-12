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
        return new XUnitCSharpTestFixtureClass(descriptor, methods, renderingOptions);
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
        var scenario = context.ScenarioInformation;
        var feature = context.FeatureInformation;

        var attributes = new List<AttributeDescriptor>();

        if (scenario.Examples.IsEmpty)
        {
            attributes.Add(XUnitSyntax.SkippableFactAttribute(context.ScenarioInformation.Name));
        }
        else
        {
            attributes.Add(XUnitSyntax.SkippableTheoryAttribute(context.ScenarioInformation.Name));
        }

        attributes.Add(XUnitSyntax.TraitAttribute("FeatureTitle", context.FeatureInformation.Name));
        attributes.Add(XUnitSyntax.TraitAttribute("Description", context.ScenarioInformation.Name));

        foreach (var tag in scenario.Tags)
        {
            attributes.Add(XUnitSyntax.TraitAttribute("Category", tag));
        }

        foreach (var set in scenario.Examples)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var example in set)
            {
                var values = example.Select(example => (object?)example.Value).ToList();

                values.Add(set.Tags);

                attributes.Add(XUnitSyntax.InlineDataAttribute(values.ToImmutableArray()));
            }
        }

        return attributes.ToImmutableArray();
    }
}
