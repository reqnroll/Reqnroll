using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Defines the basis of a source-generator which processes Gherkin feature files into test fixtures.
/// </summary>
public abstract class TestFixtureSourceGenerator : IIncrementalGenerator
{
    public static readonly DiagnosticDescriptor NoTestFrameworkFound = new(
        id: DiagnosticIds.NoTestFrameworkFound,
        title: TestFixtureSourceGeneratorResources.NoTestFrameworkFoundTitle,
        messageFormat: TestFixtureSourceGeneratorResources.NoTestFrameworkFoundMessage,
        "Reqnroll",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private readonly ImmutableArray<ITestFrameworkHandler> _testFrameworkHandlers;

    protected TestFixtureSourceGenerator() : this(ImmutableArray.Create<ITestFrameworkHandler>(
        BuiltInTestFrameworkHandlers.NUnit,
        BuiltInTestFrameworkHandlers.MSTest,
        BuiltInTestFrameworkHandlers.XUnit))
    {
    }

    protected TestFixtureSourceGenerator(ImmutableArray<ITestFrameworkHandler> testFrameworkHandlers)
    {
        _testFrameworkHandlers = testFrameworkHandlers;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    { 
        // Get all feature files in the solution.
        var featureFiles = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith(".feature", StringComparison.OrdinalIgnoreCase));

        // Extract information about the compilation.
        var compilationInformation = context.CompilationProvider
            .Select(static (compilation, cancellationToken) =>
            {
                return new CompilationInformation(
                    AssemblyName: compilation.AssemblyName,
                    Language: compilation.Language,
                    ReferencedAssemblies: compilation.ReferencedAssemblyNames.ToImmutableArray());
            });

        // Select the test framework associated with the compilation.
        var testFrameworkHandler = compilationInformation
            .Select((compilationInfo, cancellationToken) =>
            {
                var compatibleHandlers = _testFrameworkHandlers
                    .Where(handler => handler.CanGenerateLanguage(compilationInfo.Language))
                    .ToList();

                if (!compatibleHandlers.Any())
                {
                    throw new InvalidOperationException(
                        $"No test framework handlers are available which can generate {compilationInfo.Language}.");
                }

                var availableHandlers = compatibleHandlers
                    .Where(handler => handler.IsTestFrameworkReferenced(compilationInfo));

                // CONSIDER: Should we produce a warning/error when multiple handlers match?
                var testFramework = compatibleHandlers.FirstOrDefault();

                if (testFramework == null)
                {
                    var frameworks = string.Join(", ", _testFrameworkHandlers.Select(framework => framework.FrameworkName));

                    return Result.Failure<ITestFrameworkHandler>(
                        Diagnostic.Create(NoTestFrameworkFound, Location.None, frameworks));
                }

                return Result.Success(testFramework);
            });

        // Report any failures from selecting a test framework handler.
        context.RegisterSourceOutput(testFrameworkHandler, static (context, result) =>
        {
            if (result.Error != null)
            {
                context.ReportDiagnostic(result.Error);
            }
        });

        // CONSIDER: Are there any options which could affect the parsing of feature-files?
        // Obtain the options which will influence the generation of features and combine with all other information
        // to produce parsed syntax ready for translation into test fixtures.
        var featureFilesToGenerateSources = featureFiles
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Combine(compilationInformation)
            .Combine(testFrameworkHandler)
            .SelectMany(static (input, cancellationToken) =>
            {
                var (((featureFile, optionsProvider), compilationInfo), testFrameworkHandlerResult) = input;

                // If we failed to select a test framework handler, we will not produce output.
                if (testFrameworkHandlerResult.IsFailure)
                {
                    return Array.Empty<FeatureInformation>();
                }

                var options = optionsProvider.GetOptions(featureFile);

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

                var source = featureFile.GetText(cancellationToken);

                if (source == null)
                {
                    return Array.Empty<FeatureInformation>();
                }

                var parser = new GherkinSyntaxParser();

                var featureSyntax = parser.Parse(source, featureFile.Path, cancellationToken);

                return new[] 
                {
                    new FeatureInformation(
                        FeatureSyntax: featureSyntax,
                        FeatureHintName: featureHintName,
                        FeatureNamespace: featureNamespace,
                        CompilationInformation: compilationInfo,
                        TestFrameworkHandler: testFrameworkHandlerResult.Value!)
                };
            });

        // Emit source files for each feature by invoking the generator.
        context.RegisterSourceOutput(featureFilesToGenerateSources, static (context, feature) =>
        {
            // Report any syntax errors in the parsing of the document.
            // CONSIDER: Should this be handled a separate Gherkin-language analyzer instead?
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
}

internal static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(Diagnostic error) => Result<T>.Failure(error);
}

internal readonly struct Result<T> : IEquatable<Result<T>>
{
    private Result(T? value, Diagnostic? error)
    {
        _value = value;
        Error = error;
    }

    private readonly T? _value;

    public readonly T Value
    {
        get
        {
            if (Error != null)
            {
                throw new InvalidOperationException($"The result represents an error: {Error}");
            }

            return _value!;
        }
    }

    public Diagnostic? Error { get; }

    public readonly bool IsFailure => Error != null;

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Diagnostic error) => new(default, error);

    public override bool Equals(object obj)
    {
        if (obj is Result<T> result)
        {
            return Equals(result);
        }
        else
        {
            return false;
        }
    }

    public bool Equals(Result<T> other)
    {
        return Equals(_value, other._value)
            && Equals(Error, other.Error);
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 27;

            hash = hash * 43 + _value?.GetHashCode() ?? 0;
            hash = hash * 43 + Error?.GetHashCode() ?? 0;

            return hash;
        }
    }
}

public record FeatureInformation(
    GherkinSyntaxTree FeatureSyntax,
    string FeatureHintName,
    string FeatureNamespace,
    CompilationInformation CompilationInformation,
    ITestFrameworkHandler TestFrameworkHandler);

public sealed record CompilationInformation(
    string? AssemblyName, 
    string Language,
    ImmutableArray<AssemblyIdentity> ReferencedAssemblies)
{
    public bool Equals(CompilationInformation other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal)
            && ReferencedAssemblies.SequenceEqual(other.ReferencedAssemblies);
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 27;

            hash = hash * 43 + AssemblyName?.GetHashCode() ?? 0;

            foreach (var assembly in ReferencedAssemblies)
            {
                hash = hash * 43 + assembly?.GetHashCode() ?? 0;
            }

            return hash;
        }
    }
}
