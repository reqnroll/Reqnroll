using Reqnroll.FeatureSourceGenerator.MSTest;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.MSTest;

/// <summary>
/// Performs generation of MSTest 3.x test fixtures in the C# language.
/// </summary>
internal class MSTest3CSharpTestFixtureGenerator(MSTestHandler frameworkHandler) :
    CSharpTestFixtureGenerator<MSTestCSharpTestFixtureClass, MSTestCSharpTestMethod>(frameworkHandler)
{
    protected override MSTestCSharpTestFixtureClass CreateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        TestFixtureDescriptor descriptor,
        ImmutableArray<MSTestCSharpTestMethod> methods,
        CSharpRenderingOptions renderingOptions)
    {
        return new MSTestCSharpTestFixtureClass(descriptor, methods, renderingOptions);
    }

    protected override MSTestCSharpTestMethod CreateTestMethod(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        TestMethodDescriptor descriptor)
    {
        return new MSTestCSharpTestMethod(descriptor);
    }

    protected override ImmutableArray<AttributeDescriptor> GenerateTestFixtureClassAttributes(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        return ImmutableArray.Create(MSTestSyntax.TestClassAttribute());
    }

    protected override ImmutableArray<AttributeDescriptor> GenerateTestMethodAttributes(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        var scenario = context.ScenarioInformation;
        var feature = context.FeatureInformation;

        var attributes = new List<AttributeDescriptor>
        {
            MSTestSyntax.TestMethodAttribute(),
            MSTestSyntax.DescriptionAttribute(scenario.Name),
            MSTestSyntax.TestPropertyAttribute("FeatureTitle", feature.Name)
        };

        foreach (var set in scenario.Examples)
        {
            foreach (var example in set)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var arguments = example.Select(item => item.Value).ToImmutableArray<object?>();

                attributes.Add(MSTestSyntax.DataRowAttribute(arguments));
            }
        }

        return attributes.ToImmutableArray();
    }
}
