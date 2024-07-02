using System.Collections.Immutable;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

/// <summary>
/// Provides a base for generating CSharp test fixtures.
/// </summary>
/// <param name="testFrameworkHandler">The test framework handler the generator is associated with.</param>
/// <typeparam name="TTestFixtureClass">The type of test fixture class produced by the generator.</typeparam>
/// <typeparam name="TTestMethod">The type of test method produced by the generator.</typeparam>
public abstract class CSharpTestFixtureGenerator<TTestFixtureClass, TTestMethod>(ITestFrameworkHandler testFrameworkHandler) :
    ITestFixtureGenerator<CSharpCompilationInformation>
    where TTestFixtureClass : CSharpTestFixtureClass
    where TTestMethod : CSharpTestMethod
{
    public ITestFrameworkHandler TestFrameworkHandler { get; } = testFrameworkHandler;

    protected abstract ImmutableArray<AttributeDescriptor> GenerateTestFixtureClassAttributes(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken);

    protected virtual ImmutableArray<ParameterDescriptor> GenerateTestMethodParameters(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        var scenario = context.ScenarioInformation;

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

        // Append the "example tags" parameter.
        parameters.Add(
            new ParameterDescriptor(CSharpSyntax.ExampleTagsParameterName, new ArrayTypeIdentifier(CommonTypes.String)));

        return parameters.ToImmutableArray();
    }

    protected abstract ImmutableArray<AttributeDescriptor> GenerateTestMethodAttributes(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken);

    public TTestMethod GenerateTestMethod(
        TestMethodGenerationContext<CSharpCompilationInformation> context, 
        CancellationToken cancellationToken = default)
    {
        var scenario = context.ScenarioInformation;
        var identifier = CSharpSyntax.GenerateTypeIdentifier(scenario.Name);

        var attributes = GenerateTestMethodAttributes(context, cancellationToken);
        var parameters = GenerateTestMethodParameters(context, cancellationToken);
        var scenarioParameters = GenerateScenarioParameters(context, cancellationToken);

        return CreateTestMethod(context, identifier, scenario, attributes, parameters, scenarioParameters);
    }

    protected virtual ImmutableArray<KeyValuePair<string, IdentifierString>> GenerateScenarioParameters(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        var scenario = context.ScenarioInformation;

        // In the case the scenario defines no examples, we don't pass any paramters.
        if (scenario.Examples.IsEmpty)
        {
            return ImmutableArray<KeyValuePair<string, IdentifierString>>.Empty;
        }

        var headings = scenario.Examples.First().Headings;

        return headings
            .Select(CSharpSyntax.GenerateParameterIdentifier)
            .Select(identifier => new KeyValuePair<string, IdentifierString>(identifier, identifier))
            .ToImmutableArray();
    }

    protected abstract TTestMethod CreateTestMethod(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        IdentifierString identifier,
        ScenarioInformation scenario,
        ImmutableArray<AttributeDescriptor> attributes,
        ImmutableArray<ParameterDescriptor> parameters,
        ImmutableArray<KeyValuePair<string, IdentifierString>> scenarioParameters);

    public TTestFixtureClass GenerateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context, 
        IEnumerable<TTestMethod> methods, 
        CancellationToken cancellationToken = default)
    {
        var feature = context.FeatureInformation;
        var featureTitle = feature.Name;
        if (!featureTitle.EndsWith(" Feature", StringComparison.OrdinalIgnoreCase))
        {
            featureTitle += " Feature";
        }

        var identifier = CSharpSyntax.GenerateTypeIdentifier(featureTitle);

        var attributes = GenerateTestFixtureClassAttributes(context, cancellationToken);
        var namedIdentitifer = new NamedTypeIdentifier(context.TestFixtureNamespace, identifier);

        var generationOptions = new CSharpRenderingOptions(
            UseNullableReferenceTypes: context.CompilationInformation.HasNullableReferencesEnabled);

        return CreateTestFixtureClass(
            context,
            namedIdentitifer,
            feature,
            attributes,
            methods.ToImmutableArray(),
            generationOptions);
    }

    protected abstract TTestFixtureClass CreateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        NamedTypeIdentifier identifier,
        FeatureInformation feature,
        ImmutableArray<AttributeDescriptor> attributes,
        ImmutableArray<TTestMethod> methods,
        CSharpRenderingOptions renderingOptions);

    TestFixtureClass ITestFixtureGenerator<CSharpCompilationInformation>.GenerateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        IEnumerable<TestMethod> methods,
        CancellationToken cancellationToken) => GenerateTestFixtureClass(context, methods.Cast<TTestMethod>(), cancellationToken);

    TestMethod ITestFixtureGenerator<CSharpCompilationInformation>.GenerateTestMethod(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken) => GenerateTestMethod(context, cancellationToken);

    public virtual bool CanGenerateForCompilation(CSharpCompilationInformation compilation) => true;
}
