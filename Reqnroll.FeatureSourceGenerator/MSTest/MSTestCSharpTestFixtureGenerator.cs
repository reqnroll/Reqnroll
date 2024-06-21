
using Reqnroll.FeatureSourceGenerator.CSharp;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

/// <summary>
/// Performs generation of MSTest test fixtures in the C# language.
/// </summary>
internal class MSTestCSharpTestFixtureGenerator : CSharpTestFixtureGenerator
{
    protected override ImmutableArray<AttributeDescriptor> GenerateTestMethodAttributes(
        ScenarioInformation scenario,
        CancellationToken cancellationToken)
    {
        var featureName = scenario.Feature.FeatureSyntax.GetRoot().Feature.Name;

        var attributes = new List<AttributeDescriptor>
        {
            MSTestSyntax.TestMethodAttribute(),
            MSTestSyntax.DescriptionAttribute(scenario.Name),
            MSTestSyntax.TestPropertyAttribute("FeatureTitle", featureName)
        };

        foreach (var set in scenario.Examples)
        {
            foreach (var example in set)
            {
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
