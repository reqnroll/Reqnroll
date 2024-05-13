using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Reqnroll.FeatureSourceGenerator.Gherkin;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Defines the basis of a source-generator which processes Gherkin feature files into test fixtures.
/// </summary>
public abstract class TestFixtureSourceGenerator<TLanguage>(
    ImmutableArray<ITestFrameworkHandler> testFrameworkHandlers) : IIncrementalGenerator
    where TLanguage : LanguageInformation
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
            .Select((compilation, cancellationToken) =>
            {
                return new CompilationInformation<TLanguage>(
                    AssemblyName: compilation.AssemblyName,
                    Language: GetLanguageInformation(compilation),
                    ReferencedAssemblies: compilation.ReferencedAssemblyNames.ToImmutableArray());
            });

        // Find compatible test frameworks and choose a default based on referenced assemblies.
        var testFrameworkInformation = compilationInformation
            .Select((compilationInfo, cancellationToken) =>
            {
                var compatibleHandlers = _testFrameworkHandlers
                    .Where(handler => handler.CanGenerateLanguage(compilationInfo.Language))
                    .ToImmutableArray();

                if (!compatibleHandlers.Any())
                {
                    // This condition should only be possible if Roslyn is compiling a language we have produced a generator for
                    // without also including a compatible test framework handler; it should never occur in practice.
                    throw new InvalidOperationException(
                        $"No test framework handlers are available which can generate {compilationInfo.Language}.");
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
                            compilationInfo.Language,
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
                    featureNamespace = relativeDir
                        .Replace(Path.DirectorySeparatorChar, '.')
                        .Replace(Path.AltDirectorySeparatorChar, '.');

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

        // Emit source files for each feature by invoking the generator.
        context.RegisterSourceOutput(featureInformationOrErrors, static (context, featureOrError) =>
        {
            // If an error, report diagnostic.
            if (!featureOrError.IsSuccess)
            {
                var error = (Diagnostic)featureOrError;
                context.ReportDiagnostic(error);
                return;
            }

            var feature = (FeatureInformation)featureOrError;

            // Report any syntax errors in the parsing of the document.
            foreach (var diagnostic in feature.FeatureSyntax.GetDiagnostics())
            {
                context.ReportDiagnostic(diagnostic);
            }

            // Generate the test fixture source.
            var source = feature.TestFrameworkHandler.GenerateTestFixture(feature);

            if (source != null)
            {
                context.AddSource(feature.FeatureHintName, source);
            }
        });
    }

    protected abstract TLanguage GetLanguageInformation(Compilation compilation);
}
