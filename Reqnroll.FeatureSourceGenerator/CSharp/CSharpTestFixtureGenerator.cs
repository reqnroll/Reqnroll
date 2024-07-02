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
        var descriptor = new TestMethodDescriptor
        {
            Scenario = context.ScenarioInformation,
            Identifier = CSharpSyntax.GenerateTypeIdentifier(context.ScenarioInformation.Name),
            StepInvocations = GenerateStepInvocations(context, cancellationToken),
            Attributes = GenerateTestMethodAttributes(context, cancellationToken),
            Parameters = GenerateTestMethodParameters(context, cancellationToken),
            ScenarioParameters = GenerateScenarioParameters(context, cancellationToken)
        };

        return CreateTestMethod(context, descriptor);
    }

    protected virtual ImmutableArray<StepInvocation> GenerateStepInvocations(
        TestMethodGenerationContext<CSharpCompilationInformation> context,
        CancellationToken cancellationToken)
    {
        var scenario = context.ScenarioInformation;

        // In the case the scenario defines no examples, we don't pass any paramters to steps.
        if (scenario.Examples.IsEmpty)
        {
            return scenario.Steps
                .Select(step => new StepInvocation(step.StepType, step.LineNumber, step.Keyword, step.Text))
                .ToImmutableArray();
        }

        var headings = scenario.Examples.First().Headings;
        var argumentMap = headings.ToDictionary(heading => heading, CSharpSyntax.GenerateParameterIdentifier);

        var invocations = new List<StepInvocation>();

        // Translate the steps into invocations with arguments where required.
        foreach (var step in scenario.Steps)
        {
            // Look for any example placeholders in the step text to be converted to a format string with arguments.
            var arguments = new List<IdentifierString>();
            var text = step.Text;

            foreach (var (heading, parameter) in argumentMap)
            {
                var placeholder = $"<{heading}>";

                if (text.Contains(placeholder))
                {
                    var index = arguments.Count;
                    text = text.Replace(placeholder, $"{{{index}}}");
                    arguments.Add(parameter);
                }
            }

            invocations.Add(new StepInvocation(step.StepType, step.LineNumber, step.Keyword, text, arguments.ToImmutableArray()));
        }

        return invocations.ToImmutableArray();
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
        TestMethodDescriptor descriptor);

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
        
        var descriptor = new TestFixtureDescriptor
        {
            Identifier = new NamedTypeIdentifier(context.TestFixtureNamespace, identifier),
            Feature = feature,
            Attributes = GenerateTestFixtureClassAttributes(context, cancellationToken),
            HintName = context.FeatureHintName
        };

        var generationOptions = new CSharpRenderingOptions(
            UseNullableReferenceTypes: context.CompilationInformation.HasNullableReferencesEnabled);

        return CreateTestFixtureClass(
            context,
            descriptor,
            methods.ToImmutableArray(),
            generationOptions);
    }

    protected abstract TTestFixtureClass CreateTestFixtureClass(
        TestFixtureGenerationContext<CSharpCompilationInformation> context,
        TestFixtureDescriptor descriptor,
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
