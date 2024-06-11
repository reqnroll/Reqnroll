using Gherkin;
using Gherkin.Ast;
using Microsoft.CodeAnalysis.VisualBasic;
using Reqnroll.FeatureSourceGenerator.Gherkin;
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Reqnroll.FeatureSourceGenerator;

using Location = Microsoft.CodeAnalysis.Location;

/// <summary>
/// Defines the basis of a source-generator which processes Gherkin feature files into test fixtures.
/// </summary>
public abstract class TestFixtureSourceGenerator(
    ImmutableArray<ITestFrameworkHandler> testFrameworkHandlers) : IIncrementalGenerator
{
    public static readonly DiagnosticDescriptor NoTestFrameworkFound = new(
        id: DiagnosticIds.NoTestFrameworkFound,
        title: TestFixtureSourceGeneratorResources.NoTestFrameworkFoundTitle,
        messageFormat: TestFixtureSourceGeneratorResources.NoTestFrameworkFoundMessage,
        "Reqnroll",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TestFrameworkNotSupported = new(
        id: DiagnosticIds.TestFrameworkNotSupported,
        title: TestFixtureSourceGeneratorResources.TestFrameworkNotSupportedTitle,
        messageFormat: TestFixtureSourceGeneratorResources.TestFrameworkNotSupportedMessage,
        "Reqnroll",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private readonly ImmutableArray<ITestFrameworkHandler> _testFrameworkHandlers = testFrameworkHandlers;

    protected TestFixtureSourceGenerator() : this(
        ImmutableArray.Create<ITestFrameworkHandler>(
            BuiltInTestFrameworkHandlers.NUnit,
            BuiltInTestFrameworkHandlers.MSTest,
            BuiltInTestFrameworkHandlers.XUnit))
    {
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all feature files in the solution.
        var featureFiles = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith(".feature", StringComparison.OrdinalIgnoreCase));

        // Extract information about the compilation.
        var compilationInformation = context.CompilationProvider
            .Select((compilation, _) => GetCompilationInformation(compilation));

        // Find compatible test frameworks and choose a default based on referenced assemblies.
        var testFrameworkInformation = compilationInformation
            .Select((compilationInfo, cancellationToken) =>
            {
                var compatibleHandlers = _testFrameworkHandlers
                    .Where(handler => handler.CanGenerateForCompilation(compilationInfo))
                    .ToImmutableArray();

                if (!compatibleHandlers.Any())
                {
                    // This condition should only be possible if Roslyn is compiling a language we have produced a generator for
                    // without also including a compatible test framework handler; it should never occur in practice.
                    throw new InvalidOperationException(
                        $"No test framework handlers are available which can generate code for the current compilation.");
                }

                var availableHandlers = compatibleHandlers
                    .Where(handler => handler.IsTestFrameworkReferenced(compilationInfo))
                    .ToImmutableArray();

                var defaultHandlers = availableHandlers.FirstOrDefault();

                return new TestFrameworkInformation(
                    compatibleHandlers,
                    availableHandlers,
                    defaultHandlers);
            });

        // Obtain the options which will influence the generation of features and combine with all other information
        // to produce parsed syntax ready for translation into test fixtures.
        var featureInformationOrErrors = featureFiles
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Combine(compilationInformation)
            .Combine(testFrameworkInformation)
            .SelectMany(static IEnumerable<StepResult<FeatureInformation>> (input, cancellationToken) =>
            {
                var (((featureFile, optionsProvider), compilationInfo), testFrameworkInformation) = input;

                var options = optionsProvider.GetOptions(featureFile); 
                
                var source = featureFile.GetText(cancellationToken);

                // If there is no source text, we can skip this file completely.
                if (source == null)
                {
                    return [];
                }

                // Select the test framework from the following sources:
                // 1. The reqnroll.target_test_framework value from .editorconfig
                // 2. The ReqnrollTargetTestFramework from the build system properties (MSBuild project files or command-line argument)
                // 3. The assemblies referenced by the compilation indicating the presence of a test framework.
                ITestFrameworkHandler? testFramework;
                if (options.TryGetValue("reqnroll.target_test_framework", out var targetTestFrameworkIdentifier)
                    || options.TryGetValue("build_property.ReqnrollTargetTestFramework", out targetTestFrameworkIdentifier))
                {
                    // Select the target framework from the option specified.
                    testFramework = testFrameworkInformation.CompatibleHandlers
                        .SingleOrDefault(handler => 
                            string.Equals(handler.FrameworkName, targetTestFrameworkIdentifier, StringComparison.OrdinalIgnoreCase));

                    if (testFramework == null)
                    {
                        // The properties specified a test framework which is not recognised or not supported for this language.
                        var frameworkNames = testFrameworkInformation.CompatibleHandlers.Select(framework => framework.FrameworkName);
                        var frameworks = string.Join(", ", frameworkNames);

                        return 
                        [ 
                            Diagnostic.Create(
                                TestFrameworkNotSupported,
                                Location.None,
                                targetTestFrameworkIdentifier,
                                frameworks) 
                        ];
                    }
                }
                else if (testFrameworkInformation.DefaultHandler != null)
                {
                    // Use the default handler.
                    testFramework = testFrameworkInformation.DefaultHandler;
                }
                else
                {
                    // Report that no suitable target test framework could be determined.
                    var frameworkNames = testFrameworkInformation.CompatibleHandlers.Select(framework => framework.FrameworkName);
                    var frameworks = string.Join(", ", frameworkNames);

                    return 
                    [ 
                        Diagnostic.Create(
                            NoTestFrameworkFound,
                            Location.None,
                            frameworks) 
                    ];
                }

                // Determine the namespace of the feature from the project.
                if (!optionsProvider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace))
                {
                    rootNamespace = compilationInfo.AssemblyName ?? "ReqnrollFeatures";
                }

                var featureHintName = Path.GetFileNameWithoutExtension(featureFile.Path);
                var featureNamespace = rootNamespace;
                if (options.TryGetValue("build_metadata.AdditionalFiles.RelativeDir", out var relativeDir))
                {
                    var featureNamespaceParts = relativeDir
                        .Replace(Path.DirectorySeparatorChar, '.')
                        .Replace(Path.AltDirectorySeparatorChar, '.')
                        .Split('.')
                        .Select(part => DotNetSyntax.CreateIdentifier(part));

                    featureNamespace = string.Join(".", featureNamespaceParts);

                    featureHintName = relativeDir + featureHintName;
                }

                // Parse the feature file and output the result.
                var parser = new GherkinSyntaxParser();

                var featureSyntax = parser.Parse(source, featureFile.Path, cancellationToken);

                return
                [
                    new FeatureInformation(
                        FeatureSyntax: featureSyntax,
                        FeatureHintName: featureHintName,
                        FeatureNamespace: featureNamespace,
                        CompilationInformation: compilationInfo,
                        TestFrameworkHandler: testFramework)
                ];
            });

        // Filter features and errors.
        var features = featureInformationOrErrors
            .Where(result => result.IsSuccess)
            .Select((result, _) => (FeatureInformation)result);
        var errors = featureInformationOrErrors
            .Where(result => !result.IsSuccess)
            .Select((result, _) => (Diagnostic)result);

        // Generate scenario information from features.
        var scenarios = features
            .SelectMany(static (feature, cancellationToken) =>
                feature.FeatureSyntax.GetRoot().Feature.Children.SelectMany(child => 
                    GetScenarioInformation(child, feature, cancellationToken)));

        // Generate scenario methods for each scenario.
        var methods = scenarios
            .Select(static (scenario, cancellationToken) => 
                (Method: scenario.Feature.TestFrameworkHandler.GenerateTestFixtureMethod(scenario, cancellationToken),
                Scenario: scenario));

        // Generate test fixtures for each feature.
        var fixtures = methods.Collect()
            .WithComparer(ImmutableArrayEqualityComparer<(TestFixtureMethod Method, ScenarioInformation Scenario)>.Default)
            .SelectMany(static (methods, cancellationToken) =>
                methods
                    .GroupBy(item => item.Scenario.Feature, item => item.Method)
                    .Select(group => new TestFixtureComposition(group.Key, group.ToImmutableArray())))
            .Select(static (composition, cancellationToken) => 
                composition.Feature.TestFrameworkHandler.GenerateTestFixture(
                    composition.Feature,
                    composition.Methods,
                    cancellationToken));

        // Emit errors.
        context.RegisterSourceOutput(errors, static (context, error) => context.ReportDiagnostic(error));

        // Emit parsing diagnostics.
        context.RegisterSourceOutput(features, static (context, feature) =>
        {
            foreach (var diagnostic in feature.FeatureSyntax.GetDiagnostics())
            {
                context.ReportDiagnostic(diagnostic);
            }
        });

        // Emit source files for fixtures.
        context.RegisterSourceOutput(
            fixtures, 
            static (context, fixture) => context.AddSource(fixture.HintName, fixture.Render()));
    }

    private static IEnumerable<ScenarioInformation> GetScenarioInformation(
        IHasLocation child,
        FeatureInformation feature,
        CancellationToken cancellationToken)
    {
        return child switch
        {
            Scenario scenario => [ GetScenarioInformation(scenario, feature, cancellationToken) ],
            Rule rule => GetScenarioInformation(rule, feature, cancellationToken),
            _ => []
        };
    }

    private static ScenarioInformation GetScenarioInformation(
        Scenario scenario,
        FeatureInformation feature,
        CancellationToken cancellationToken) => 
        GetScenarioInformation(scenario, null, ImmutableArray<string>.Empty, feature, cancellationToken);

    private static ScenarioInformation GetScenarioInformation(
        Scenario scenario,
        string? ruleName,
        ImmutableArray<string> ruleTags,
        FeatureInformation feature,
        CancellationToken cancellationToken)
    {
        var exampleSets = new List<ScenarioExampleSet>();

        foreach (var example in scenario.Examples)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var examples = new ScenarioExampleSet(
                example.TableHeader.Cells.Select(cell => cell.Value).ToImmutableArray(),
                example.TableBody.Select(row => row.Cells.Select(cell => cell.Value).ToImmutableArray()).ToImmutableArray(),
                example.Tags.Select(tag => tag.Name).ToImmutableArray());

            exampleSets.Add(examples);
        }

        var steps = new List<ScenarioStep>();

        foreach (var step in scenario.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var scenarioStep = new ScenarioStep(
                step.KeywordType,
                step.Keyword,
                step.Text,
                step.Location.Line);

            steps.Add(scenarioStep);
        }

        return new ScenarioInformation(
            feature,
            scenario.Name,
            scenario.Tags.Select(tag => tag.Name).ToImmutableArray(),
            steps.ToImmutableArray(),
            exampleSets.ToImmutableArray(),
            ruleName,
            ruleTags);
    }

    private static IEnumerable<ScenarioInformation> GetScenarioInformation(
        Rule rule,
        FeatureInformation feature,
        CancellationToken cancellationToken)
    {
        var tags = rule.Tags.Select(tag => tag.Name).ToImmutableArray();

        foreach (var child in rule.Children)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (child)
            {
                case Scenario scenario:
                    yield return GetScenarioInformation(scenario, rule.Name, tags, feature, cancellationToken);
                    break;
            }
        }
    }

    protected abstract CompilationInformation GetCompilationInformation(Compilation compilation);
}

public record ScenarioStep(StepKeywordType KeywordType, string Keyword, string Text, int LineNumber);

public class ScenarioExampleSet : IEnumerable<IEnumerable<(string Name, string Value)>>, IEquatable<ScenarioExampleSet?>
{
    public ScenarioExampleSet(
        ImmutableArray<string> headings,
        ImmutableArray<ImmutableArray<string>> values,
        ImmutableArray<string> tags)
    {
        foreach (var set in values)
        {
            if (set.Length != headings.Length)
            {
                throw new ArgumentException(
                    "Values must contain sets with the same number of values as the headings.",
                    nameof(values));
            }
        }

        Headings = headings;
        Values = values;
        Tags = tags;
    }

    public ImmutableArray<string> Headings { get; }

    public ImmutableArray<ImmutableArray<string>> Values { get; }

    public ImmutableArray<string> Tags { get; }

    public override bool Equals(object? obj) => Equals(obj as ScenarioExampleSet);

    public bool Equals(ScenarioExampleSet? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return (Headings.Equals(other.Headings) || Headings.SequenceEqual(other.Headings)) &&
            (Values.Equals(other.Values) || Values.SequenceEqual(other.Values, ImmutableArrayEqualityComparer<string>.Default)) &&
            (Tags.Equals(other.Tags) || Tags.SequenceEqual(other.Tags));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 43961407;

            hash *= 32360441 + Headings.GetSequenceHashCode();
            hash *= 32360441 + Values.GetSequenceHashCode(ImmutableArrayEqualityComparer<string>.Default);
            hash *= 32360441 + Tags.GetSequenceHashCode();

            return hash;
        }
    }

    public IEnumerator<IEnumerable<(string Name, string Value)>> GetEnumerator()
    {
        foreach (var set in Values)
        {
            yield return GetAsRow(set);
        }
    }

    private IEnumerable<(string Name, string Value)> GetAsRow(ImmutableArray<string> set)
    {
        for (var i = 0; i < Headings.Length; i++)
        {
            yield return (Name: Headings[i], Value: set[i]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
