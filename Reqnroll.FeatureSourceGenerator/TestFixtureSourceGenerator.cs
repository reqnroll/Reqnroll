using Gherkin;
using Gherkin.Ast;
using Reqnroll.FeatureSourceGenerator.Gherkin;
using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Reqnroll.FeatureSourceGenerator;

using Location = Microsoft.CodeAnalysis.Location;

/// <summary>
/// Defines the basis of a source-generator which processes Gherkin feature files into test fixtures.
/// </summary>
public abstract class TestFixtureSourceGenerator<TCompilationInformation>(
    ImmutableArray<ITestFrameworkHandler> testFrameworkHandlers) : IIncrementalGenerator
    where TCompilationInformation : CompilationInformation
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

        // Find compatible generator and choose a default based on referenced assemblies.
        var generatorInformation = compilationInformation
            .Select((compilationInfo, cancellationToken) =>
            {
                var compatibleGenerators = _testFrameworkHandlers
                    .Select(handler => handler.GetTestFixtureGenerator(compilationInfo)!)
                    .Where(generator => generator != null)
                    .ToImmutableArray();

                if (!compatibleGenerators.Any())
                {
                    // This condition should only be possible if Roslyn is compiling a language we have produced a generator for
                    // without also including a compatible test framework handler; it should never occur in practice.
                    throw new InvalidOperationException(
                        $"No test framework handlers are available which can generate code for the current compilation.");
                }

                var useableGenerators = compatibleGenerators
                    .Where(generator => generator.TestFrameworkHandler.IsTestFrameworkReferenced(compilationInfo))
                    .ToImmutableArray();

                var defaultGenerator = useableGenerators.FirstOrDefault();

                return new GeneratorInformation<TCompilationInformation>(
                    compatibleGenerators,
                    useableGenerators,
                    defaultGenerator);
            });

        // Obtain the options which will influence the generation of features and combine with all other information
        // to produce parsed syntax ready for translation into test fixtures.
        var testFixtureGenerationContextsOrErrors = featureFiles
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Combine(compilationInformation)
            .Combine(generatorInformation)
            .SelectMany(static IEnumerable<StepResult<TestFixtureGenerationContext<TCompilationInformation>>> (input, cancellationToken) =>
            {
                var (((featureFile, optionsProvider), compilationInfo), generatorInformation) = input;

                var options = optionsProvider.GetOptions(featureFile);

                // Launch a debugger if configured.
                if (options.GetBooleanValue(
                    "reqnroll_feature_source_generator.launch_debugger",
                    "build_property.ReqnrollDebugGenerator") ?? false)
                {
                    Debugger.Launch();
                }

                var source = featureFile.GetText(cancellationToken);

                // If there is no source text, we can skip this file completely.
                if (source == null)
                {
                    return [];
                }

                // Select the generator from the following sources:
                // 1. The reqnroll_feature_source_generator.target_test_framework value from .editorconfig
                // 2. The ReqnrollTargetTestFramework from the build system properties (MSBuild project files or command-line argument)
                // 3. The assemblies referenced by the compilation indicating the presence of a test framework.
                ITestFixtureGenerator<TCompilationInformation>? generator;
                var targetTestFrameworkIdentifier = options.GetStringValue(
                    "reqnroll_feature_source_generator.target_test_framework",
                    "build_property.ReqnrollTargetTestFramework");
                if(!string.IsNullOrEmpty(targetTestFrameworkIdentifier))
                {
                    // Select the target framework from the option specified.
                    generator = generatorInformation.CompatibleGenerators
                        .SingleOrDefault(generator => string.Equals(
                            generator.TestFrameworkHandler.TestFrameworkName,
                            targetTestFrameworkIdentifier,
                            StringComparison.OrdinalIgnoreCase));

                    if (generator == null)
                    {
                        // The properties specified a test framework which is not recognised or not supported for this language.
                        var frameworkNames = generatorInformation.CompatibleGenerators
                            .Select(generator => generator.TestFrameworkHandler.TestFrameworkName);
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
                else if (generatorInformation.DefaultGenerator != null)
                {
                    // Use the default handler.
                    generator = generatorInformation.DefaultGenerator;
                }
                else
                {
                    // Report that no suitable target test framework could be determined.
                    var frameworkNames = generatorInformation.CompatibleGenerators
                        .Select(generator => generator.TestFrameworkHandler.TestFrameworkName);
                    var frameworks = string.Join(", ", frameworkNames);

                    return
                    [
                        Diagnostic.Create(
                            NoTestFrameworkFound,
                            Location.None,
                            frameworks)
                    ];
                }

                // Determine the hint path and namespace for the generated test fixtured based on the project and the relative file path.
                var rootNamespace = optionsProvider.GlobalOptions.GetStringValue("build_property.RootNamespace") ?? 
                    compilationInfo.AssemblyName ?? 
                    "ReqnrollFeatures";

                var featureHintName = Path.GetFileNameWithoutExtension(featureFile.Path);
                var testFixtureNamespace = rootNamespace;
                var relativeDir = options.GetStringValue("build_metadata.AdditionalFiles.RelativeDir");
                if (!string.IsNullOrEmpty(relativeDir))
                {
                    var testFixtureNamespaceParts = relativeDir!
                        .Replace(Path.DirectorySeparatorChar, '.')
                        .Replace(Path.AltDirectorySeparatorChar, '.')
                        .Split(['.'], StringSplitOptions.RemoveEmptyEntries)
                        .Select(part => DotNetSyntax.CreateIdentifier(part))
                        .ToList();

                    testFixtureNamespaceParts.Insert(0, testFixtureNamespace);

                    testFixtureNamespace = string.Join(".", testFixtureNamespaceParts);

                    featureHintName = relativeDir + featureHintName;
                }

                // Parse the feature file and output the result.
                var parser = new Parser { StopAtFirstError = false };

                cancellationToken.ThrowIfCancellationRequested();

                GherkinDocument document;

                try
                {
                    // CONSIDER: Using a parser that doesn't throw exceptions for syntax errors.
                    // CONSIDER: Using a parser that uses Roslyn text data-types.
                    // CONSIDER: Using a parser that supports incremental parsing.
                    // CONSIDER: Using a parser that provides human-readable syntax errors.
                    document = parser.Parse(new SourceTokenScanner(source));
                }
                catch (CompositeParserException ex)
                {
                    // We can't report errors for specific files using the built-in diagnostic system
                    // https://github.com/dotnet/roslyn/issues/49531
                    // Instead we will report the error by writing it as output.
                    // Use the diagnostic to convey the feature hint name we'll use to write the error.
                    var diagnostics = ex.Errors
                        .Select(error => GherkinSyntaxParser.CreateGherkinDiagnostic(error, source, featureFile.Path));

                    return [.. diagnostics];
                }

                // Determine whether we should include ignored examples in our sample sets.
                var emitIgnoredExamples = options.GetBooleanValue(
                    "reqnroll_feature_source_generator.emit_ignored_examples",
                    "build_metadata.AdditionalFiles.EmitIgnoredExamples",
                    "build_property.ReqnrollEmitIgnoredExamples") ?? false;

                var feature = document.Feature;

                var featureInformation = new FeatureInformation(
                    feature.Name,
                    feature.Description,
                    feature.Language,
                    feature.Tags.Select(tag => tag.Name.TrimStart('@')).ToImmutableArray(),
                    featureFile.Path);

                var scenarioInformations = CreateScenarioInformations(featureFile.Path, feature, emitIgnoredExamples, cancellationToken);

                return
                [
                    new TestFixtureGenerationContext<TCompilationInformation>(
                        featureInformation,
                        scenarioInformations.ToImmutableArray(),
                        featureHintName,
                        new NamespaceString(testFixtureNamespace),
                        compilationInfo,
                        generator)
                ];
            });
                

        // Filter contexts and errors.
        var testFixtureGenerationContexts = testFixtureGenerationContextsOrErrors
            .Where(result => result.IsSuccess)
            .Select((result, _) => (TestFixtureGenerationContext<TCompilationInformation>)result);
        var errors = testFixtureGenerationContextsOrErrors
            .Where(result => !result.IsSuccess)
            .Select((result, _) => (Diagnostic)result);

        // Set up test method generation contexts from feature generation contexts.
        var testMethodGenerationContexts = testFixtureGenerationContexts
            .SelectMany(static (context, cancellationToken) => 
                context.ScenarioInformations.Select(scenario => 
                    new TestMethodGenerationContext<TCompilationInformation>(scenario, context)));

        // Generate test methods for each scenario.
        var methods = testMethodGenerationContexts
            .Select(static (context, cancellationToken) => 
                (Method: context.TestFixtureGenerator.GenerateTestMethod(context, cancellationToken),
                Context: context));

        // Generate test fixtures for each feature.
        var fixtures = methods.Collect()
            .WithComparer(ImmutableArrayEqualityComparer<(TestMethod Method, TestMethodGenerationContext<TCompilationInformation> Context)>.Default)
            .SelectMany(static (methods, cancellationToken) =>
                methods
                    .GroupBy(item => item.Context.TestFixtureGenerationContext, item => item.Method)
                    .Select(group => new TestFixtureComposition<TCompilationInformation>(group.Key, group.ToImmutableArray())))
            .Select(static (composition, cancellationToken) => 
                composition.Context.TestFixtureGenerator.GenerateTestFixtureClass(
                    composition.Context,
                    composition.Methods,
                    cancellationToken));

        // Emit errors.
        context.RegisterSourceOutput(
            errors,
            static (context, error) =>
            {
                // We can't report errors for specific files using the built-in diagnostic system
                // https://github.com/dotnet/roslyn/issues/49531
                // Instead we will report the error by writing it as output.

                //if (error.Location == Location.None)
                //{
                context.ReportDiagnostic(error);
                //}
                //else
                //{
                //    context.AddSource(error.Location.GetLineSpan().Path, SourceText.From(""))
                //}
            });

        // Emit source files for fixtures.
        context.RegisterSourceOutput(
            fixtures, 
            static (context, fixture) => context.AddSource(fixture.HintName, fixture.Render(context.CancellationToken)));
    }

    private static IEnumerable<ScenarioInformation> CreateScenarioInformations(
        string sourceFilePath,
        Feature feature,
        bool emitIgnoredExamples,
        CancellationToken cancellationToken)
    {
        var children = feature.Children.ToList();

        var scenarios = new List<ScenarioInformation>();
        var backgroundSteps = new List<SourceModel.Step>();

        if (children.FirstOrDefault() is Background background)
        {
            PopulateSteps(backgroundSteps, sourceFilePath, background, cancellationToken);
        }

        foreach (var child in children)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (child)
            {
                case Scenario scenario:
                    scenarios.Add(
                        CreateScenarioInformation(sourceFilePath, scenario, backgroundSteps, emitIgnoredExamples, cancellationToken));
                    break;

                case Rule rule:
                    scenarios.AddRange(
                        CreateScenarioInformations(sourceFilePath, rule, backgroundSteps, emitIgnoredExamples, cancellationToken));
                    break;
            }
        }

        return scenarios;
    }

    private static IEnumerable<ScenarioInformation> CreateScenarioInformations(
        string sourceFilePath,
        Rule rule,
        IReadOnlyList<SourceModel.Step> backgroundSteps,
        bool emitIgnoredExamples,
        CancellationToken cancellationToken)
    {
        var tags = rule.Tags.Select(tag => tag.Name.TrimStart('@')).ToImmutableArray();

        var children = rule.Children.ToList();

        var scenarios = new List<ScenarioInformation>();
        var combinedBackgroundSteps = backgroundSteps.ToList();

        if (children.FirstOrDefault() is Background background)
        {
            PopulateSteps(combinedBackgroundSteps, sourceFilePath, background, cancellationToken);
        }

        foreach (var child in rule.Children)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (child)
            {
                case Scenario scenario:
                    yield return CreateScenarioInformation(
                        sourceFilePath,
                        scenario,
                        combinedBackgroundSteps,
                        new RuleInformation(rule.Name, tags),
                        emitIgnoredExamples,
                        cancellationToken);
                    break;
            }
        }
    }

    private static ScenarioInformation CreateScenarioInformation(
        string sourceFilePath,
        Scenario scenario,
        IReadOnlyList<SourceModel.Step> backgroundSteps,
        bool emitIgnoredExamples,
        CancellationToken cancellationToken) => CreateScenarioInformation(
            sourceFilePath,
            scenario,
            backgroundSteps,
            null,
            emitIgnoredExamples,
            cancellationToken);

    private static ScenarioInformation CreateScenarioInformation(
        string sourceFilePath,
        Scenario scenario,
        IReadOnlyList<SourceModel.Step> backgroundSteps,
        RuleInformation? rule,
        bool emitIgnoredExamples,
        CancellationToken cancellationToken)
    {
        var exampleSets = new List<ScenarioExampleSet>();

        foreach (var example in scenario.Examples)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var examples = new ScenarioExampleSet(
                example.TableHeader.Cells.Select(cell => cell.Value).ToImmutableArray(),
                example.TableBody.Select(row => row.Cells.Select(cell => cell.Value).ToImmutableArray()).ToImmutableArray(),
                example.Tags.Select(tag => tag.Name.TrimStart('@')).ToImmutableArray());

            if (!emitIgnoredExamples && examples.Tags.Contains("ignore", StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            exampleSets.Add(examples);
        }

        var steps = backgroundSteps.ToList();
        PopulateSteps(steps, sourceFilePath, scenario, cancellationToken);

        var keywordAndNameStartPosition = scenario.Location.ToLinePosition();
        // We assume as single character gap between keyword and scenario name; could be more.
        var keywordAndNameEndPosition = new LinePosition(
            scenario.Location.Line,
            scenario.Location.Column + scenario.Keyword.Length + scenario.Name.Length + 1);

        return new ScenarioInformation(
            scenario.Name,
            new FileLinePositionSpan(sourceFilePath, keywordAndNameStartPosition, keywordAndNameEndPosition),
            scenario.Tags.Select(tag => tag.Name.TrimStart('@')).ToImmutableArray(),
            steps.ToImmutableArray(),
            exampleSets.ToImmutableArray(),
            rule);
    }

    private static void PopulateSteps(
        List<SourceModel.Step> steps,
        string sourceFilePath,
        StepsContainer container,
        CancellationToken cancellationToken)
    {
        foreach (var step in container.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startPosition = step.Location.ToLinePosition();
            // We assume as single character gap between keyword and step text; could be more.
            var endPosition = new LinePosition(startPosition.Line, startPosition.Character + step.Keyword.Length + step.Text.Length + 1);
            var position = new FileLinePositionSpan(sourceFilePath, new LinePositionSpan(startPosition, endPosition));

            var scenarioStep = new SourceModel.Step(
                step.KeywordType switch
                {
                    StepKeywordType.Context => StepType.Context,
                    StepKeywordType.Action => StepType.Action,
                    StepKeywordType.Outcome => StepType.Outcome,
                    StepKeywordType.Conjunction => StepType.Conjunction,
                    _ => throw new NotSupportedException()
                },
                step.Keyword,
                step.Text,
                position);

            steps.Add(scenarioStep);
        }
    }

    protected abstract TCompilationInformation GetCompilationInformation(Compilation compilation);
}
