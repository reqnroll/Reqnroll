using Reqnroll.FeatureSourceGenerator.MSTest;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.MSTest;

/// <summary>
/// Performs generation of MSTest 2.x test fixtures in the C# language.
/// </summary>
internal class MSTest2CSharpTestFixtureGenerator(MSTestHandler frameworkHandler) : 
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

                // DataRow's constructor is DataRow(object? data, params object?[] moreData)
                // Because we often pass an array of strings as a second argument, we always wrap moreData
                // in an explicit array to avoid the compiler mistaking our string array as the moreData value.
                var values = example.Select(item => item.Value).ToList<object?>();
                var data = values.First();
                var moreData = values.Skip(1).ToList();

                moreData.Add(set.Tags);

                var arguments = moreData.Count > 0 ? ImmutableArray.Create(data, moreData.ToImmutableArray()) : ImmutableArray.Create(data);

                attributes.Add(MSTestSyntax.DataRowAttribute(arguments));
            }
        }

        return attributes.ToImmutableArray();
    }
}
