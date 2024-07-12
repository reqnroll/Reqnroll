using Reqnroll.FeatureSourceGenerator.NUnit;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.NUnit;
internal class NUnitCSharpTestFixtureGenerator(NUnitHandler frameworkHandler) :
    CSharpTestFixtureGenerator<NUnitCSharpTestFixtureClass, NUnitCSharpTestMethod>(frameworkHandler)
{
    protected override NUnitCSharpTestFixtureClass CreateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        TestFixtureDescriptor descriptor,
        ImmutableArray<NUnitCSharpTestMethod> methods,
        CSharpRenderingOptions renderingOptions)
    {
        return new NUnitCSharpTestFixtureClass(descriptor, methods, renderingOptions);
    }

    protected override NUnitCSharpTestMethod CreateTestMethod(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        TestMethodDescriptor descriptor)
    {
        return new NUnitCSharpTestMethod(descriptor);
    }

    protected override ImmutableArray<AttributeDescriptor> GenerateTestFixtureClassAttributes(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        return ImmutableArray.Create(NUnitSyntax.DescriptionAttribute(context.FeatureInformation.Name));
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
            attributes.Add(NUnitSyntax.TestAttribute());
        }

        attributes.Add(NUnitSyntax.DescriptionAttribute(context.ScenarioInformation.Name));

        foreach (var tag in scenario.Tags)
        {
            attributes.Add(NUnitSyntax.CategoryAttribute(tag));
        }

        if (scenario.Tags.Contains("ignore"))
        {
            attributes.Add(NUnitSyntax.IgnoreAttribute("Ignored scenario"));
        }

        foreach (var set in scenario.Examples)
        {
            foreach (var example in set)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var values = example.Select(example => (object?)example.Value).ToList();

                values.Add(set.Tags);

                string? category = set.Tags.IsEmpty ? null : string.Join(",", set.Tags);
                string? ignore = set.Tags.Contains("ignore", StringComparer.OrdinalIgnoreCase) ? "Ignored by @ignore tag" : null;

                attributes.Add(NUnitSyntax.TestCaseAttribute(values.ToImmutableArray(), category, ignore));
            }
        }

        return attributes.ToImmutableArray();
    }
}
