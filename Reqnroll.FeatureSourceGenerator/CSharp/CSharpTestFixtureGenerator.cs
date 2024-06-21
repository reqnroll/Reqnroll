using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

/// <summary>
/// Provides a base for generating CSharp test fixtures.
/// </summary>
public abstract class CSharpTestFixtureGenerator : ITestFixtureGenerator
{
    public TestFixtureClass GenerateTestFixture(
        FeatureInformation feature,
        IEnumerable<TestMethod> methods,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public TestMethod GenerateTestMethod(
        ScenarioInformation scenario,
        CancellationToken cancellationToken = default) => GenerateCSharpTestMethod(scenario, cancellationToken);

    protected virtual CSharpTestMethod GenerateCSharpTestMethod(
        ScenarioInformation scenario,
        CancellationToken cancellationToken)
    {
        var identifier = CSharpSyntax.GenerateTypeIdentifier(scenario.Name);

        var attributes = GenerateTestMethodAttributes(scenario, cancellationToken);
        var parameters = GenerateTestMethodParameters(scenario, cancellationToken);

        return new CSharpTestMethod(identifier, attributes, parameters);
    }

    protected virtual ImmutableArray<ParameterDescriptor> GenerateTestMethodParameters(
        ScenarioInformation scenario,
        CancellationToken cancellationToken)
    {
        // In the case the scenario defines no examples, we don't pass any paramters.
        if (scenario.Examples.IsEmpty)
        {
            return ImmutableArray<ParameterDescriptor>.Empty;
        }

        // In the case there are examples, we expect to pass arguments to the method,
        // the last one being any tags which apply to the example.
        // We're assuming that every set defines the same columns, which means we'll only look at the first set.
        var headings = scenario.Examples.First().Headings;
        var parameters = headings
            .Select(heading => new ParameterDescriptor(CSharpSyntax.GenerateParameterIdentifier(heading), CommonTypes.String))
            .ToList();

        // Append the tags parameter.
        parameters.Add(
            new ParameterDescriptor(new IdentifierString("_tags"), new ArrayTypeIdentifier(CommonTypes.String)));

        return parameters.ToImmutableArray();
    }

    protected abstract ImmutableArray<AttributeDescriptor> GenerateTestMethodAttributes(
        ScenarioInformation scenario,
        CancellationToken cancellationToken);
}
