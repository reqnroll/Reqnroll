using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections;
using System.Collections.Immutable;

namespace Reqnroll.StepBindingSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CSharpStepBindingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Obtain step definitions by looking for the various attributes
        var givenStepDefinitions = context
            .GetStepDefinitionSyntaxInfoAndConfig("Reqnroll.GivenAttribute", StepKeywordMatch.Given);
        var whenStepDefinitions = context
            .GetStepDefinitionSyntaxInfoAndConfig("Reqnroll.WhenAttribute", StepKeywordMatch.When);
        var thenStepDefinitions = context
            .GetStepDefinitionSyntaxInfoAndConfig("Reqnroll.ThenAttribute", StepKeywordMatch.Then);
        var otherStepDefinitions = context
            .GetStepDefinitionSyntaxInfoAndConfig("Reqnroll.StepDefinitionAttribute", StepKeywordMatch.Any);

        // Combine all step definitions into a single stream and create definitions for emittings
        var allStepDefinitions = givenStepDefinitions.Collect()
            .Combine(whenStepDefinitions.Collect())
            .Combine(thenStepDefinitions.Collect())
            .Combine(otherStepDefinitions.Collect())
            .SelectMany(static (definitions, _) =>
            {
                var (((givens, whens), thens), others) = definitions;

                return givens.Concat(whens).Concat(thens).Concat(others);
            })
            .Select(static (definitionSyntax, cancellationToken) =>
            {
                var displayName = definitionSyntax.Method.Name;
                BindingMethod bindingMethod; 
                if (definitionSyntax.TextPattern == null)
                {
                    bindingMethod = BindingMethod.MethodName;
                }
                else if (CucumberExpressionDetector.IsCucumberExpression(definitionSyntax.TextPattern))
                {
                    bindingMethod = BindingMethod.CucumberExpression;

                    var prefix = definitionSyntax.MatchedKeywords == StepKeywordMatch.Any ? 
                        "*" : 
                        definitionSyntax.MatchedKeywords.ToString();
                    
                    displayName = $"{prefix} {definitionSyntax.TextPattern}";
                }
                else
                {
                    bindingMethod = BindingMethod.RegularExpression;
                }

                var invocationStyle = definitionSyntax.Method.IsAsync ?
                    MethodInvocationStyle.Asynchronous :
                    MethodInvocationStyle.Synchronous;

                return new StepDefinitionInfo(
                    definitionSyntax.Method.Name,
                    displayName,
                    definitionSyntax.Method,
                    definitionSyntax.MatchedKeywords,
                    bindingMethod,
                    definitionSyntax.TextPattern,
                    invocationStyle);
            })
            .Collect();

        // Group steps definitions into their respective catalogs based on the class which declares them
        var catalogs = allStepDefinitions
            .SelectMany(static (definitions, _) => definitions
                .GroupBy(definition => definition.Method.DeclaringClassName)
                .Select(group => new StepDefinitionCatalogInfo(
                    group.Key.Name + "Catalog",
                    group.Key with { Name = group.Key.Name + "Catalog" },
                    ComparableArray.CreateRange(group))));

        // Generate unique hint names for each catalog
        var catalogHintNames = catalogs
            .Select(static (catalog, _) => (catalog.ClassName))
            .Collect()
            .Select(static (catalogIds, _) =>
            {
                var names = new HashSet<string>();
                var mappedNames = ImmutableDictionary.CreateBuilder<QualifiedTypeName, string>();

                foreach (var catalogId in catalogIds)
                {
                    var (classNamespace, className) = catalogId;
                    var hintName = className;
                    var i = 1;

                    while (names.Contains(hintName))
                    {
                        hintName = className + i++;
                    }

                    names.Add(hintName);
                    mappedNames.Add(catalogId, hintName);
                }

                return mappedNames.ToImmutable();
            })
            .WithComparer(
                ImmutableDictionaryEqualityComparer<QualifiedTypeName, string>.Instance);

        // Combine the catalogs with their hint names
        var hintedCatalogs = catalogs
            .Combine(catalogHintNames)
            .Select(static (catalogAndHintNames, _) =>
            {
                var (catalog, hintNames) = catalogAndHintNames;

                if (hintNames.TryGetValue(catalog.ClassName, out var hintName))
                {
                    return catalog with { HintName = hintName };
                }

                return catalog;
            });

        // Output the step registry constructor which roots all defined step methods
        context.RegisterSourceOutput(allStepDefinitions, (context, stepMethods) =>
        {
            if (!stepMethods.Any())
            {
                return;
            }

            var emitter = new RegistryClassEmitter("Sample.Tests");

            context.AddSource("ReqnrollStepRegistry.g.cs", emitter.EmitRegistryClassConstructor(stepMethods));
        });

        // Output the step catalog classes
        context.RegisterSourceOutput(hintedCatalogs, (context, stepDefinitionClass) =>
        {
            var emitter = new StepDefinitionEmitter();

            context.AddSource(
                $"{stepDefinitionClass.HintName}.g.cs",
                emitter.EmitStepDefinitionCatalogClass(stepDefinitionClass));
        });
    }
}

/// <summary>
/// Represents the pipline-relevant information about the declaration of a step definition.
/// </summary>
/// <param name="MethodDeclarationSyntax">The method declaration syntax which has been identified to be a step 
/// definition.</param>
internal record StepDefinitionSyntaxInfo(
    MethodDeclarationSyntax MethodDeclarationSyntax,
    MethodInfo Method,
    StepKeywordMatch MatchedKeywords,
    string? TextPattern);

internal record MethodInfo(
    string Name,
    QualifiedTypeName DeclaringClassName,
    DiagnosticLocation IdentifierLocation,
    bool ReturnsVoid,
    bool ReturnsTask,
    bool IsAsync,
    ComparableArray<ParameterInfo> Parameters);

internal record struct QualifiedTypeName(string Namespace, string Name);

internal record ParameterInfo(string Name, string ParameterType);

internal enum StepKeywordMatch
{
    Given,
    When,
    Then,
    Any
}

internal enum MethodInvocationStyle
{
    Synchronous,
    Asynchronous
}

internal record StepDefinitionInfo(
    string Name,
    string DisplayName,
    MethodInfo Method,
    StepKeywordMatch MatchesKeywords,
    BindingMethod BindingMethod,
    string? TextPattern,
    MethodInvocationStyle InvocationStyle);

internal record StepDefinitionCatalogInfo(
    string HintName,
    QualifiedTypeName ClassName,
    ComparableArray<StepDefinitionInfo> StepDefinitions);

internal static class ComparableArray
{
    public static ComparableArray<T> CreateRange<T>(IEnumerable<T> items) => new(ImmutableArray.CreateRange(items));

    public static ComparableArray<T> Create<T>(T item) => new(ImmutableArray.Create(item));

    public static ComparableArray<T> CreateRange<T>(ReadOnlySpan<T> items)
    {
        var builder = ImmutableArray.CreateBuilder<T>(items.Length);

        foreach (var item in items)
        {
            builder.Add(item);
        }

        return new ComparableArray<T>(builder.ToImmutable());
    }
}

internal readonly struct ComparableArray<T>(ImmutableArray<T> values) : IEquatable<ComparableArray<T>>, IEnumerable<T>
{
    private readonly ImmutableArray<T> _values = values;

    public static ComparableArray<T> Empty { get; } = new ComparableArray<T>(ImmutableArray<T>.Empty);

    public int Length => _values.Length;

    public bool IsDefaultOrEmpty => _values.IsDefaultOrEmpty;

    public override bool Equals(object obj)
    {
        if (obj is ComparableArray<T> other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(ComparableArray<T> other)
    {
        if (_values.IsDefaultOrEmpty)
        {
            return other._values.IsDefaultOrEmpty;
        }

        if (other._values.IsDefaultOrEmpty)
        {
            return false;
        }

        // This comparison checks if the two immutable arrays point to the same underlying array object.
        if (_values == other._values)
        {
            return true;
        }

        if (_values.Length != other._values.Length)
        {
            return false;
        }

        for (var i = 0; i < _values.Length; i++)
        {
            if (!Equals(_values[i], other._values[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        if (_values.IsDefaultOrEmpty)
        {
            return 0;
        }

        return _values.GetSequenceHashCode();
    }

    public ImmutableArray<T>.Enumerator GetEnumerator() => 
        (_values.IsDefault ? ImmutableArray<T>.Empty : _values).GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
        ((IEnumerable<T>)(_values.IsDefault ? ImmutableArray<T>.Empty : _values)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => 
        ((IEnumerable<T>)(_values.IsDefault ? ImmutableArray<T>.Empty : _values)).GetEnumerator();

    public static implicit operator ComparableArray<T>(ImmutableArray<T> array) => new(array);
}

internal enum BindingMethod
{
    RegularExpression,
    CucumberExpression,
    MethodName
}

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    DiagnosticLocation Location,
    params object?[]? MessageArguments)
{
    public DiagnosticSeverity Severity => Descriptor.DefaultSeverity;

    public Diagnostic CreateDiagnostic() => Diagnostic.Create(Descriptor, Location, MessageArguments);

    public bool Equals(DiagnosticInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!(Descriptor.Equals(other.Descriptor) &&
            Location.Equals(other.Location)))
        {
            return false;
        }
        
        if (MessageArguments == null)
        {
            return other.MessageArguments == null;
        }

        if (other.MessageArguments == null)
        {
            return false;
        }

        return MessageArguments.SequenceEqual(other.MessageArguments);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 4037797;

            hash *= 1864987 + Descriptor.GetHashCode();
            hash *= 1864987 + Location.GetHashCode();
            hash *= 1864987 + MessageArguments?.GetSequenceHashCode() ?? 0;

            return hash;
        }
    }
}

internal record struct DiagnosticLocation(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public static DiagnosticLocation None { get; } = default;

    public readonly Location? ToLocation() => FilePath == null ? null : Location.Create(FilePath, TextSpan, LineSpan);

    public static DiagnosticLocation CreateFrom(SyntaxNode? node) => node == null ? None : CreateFrom(node.GetLocation());

    public static DiagnosticLocation CreateFrom(Location location)
    {
        if (location.SourceTree == null)
        {
            throw new ArgumentException("Diagnostic location must be from a source location.", nameof(location));
        }

        return new DiagnosticLocation(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    public static DiagnosticLocation CreateFrom(SyntaxReference? syntaxReference)
    { 
        if (syntaxReference == null)
        {
            return None;
        }

        return CreateFrom(syntaxReference.SyntaxTree.GetLocation(syntaxReference.Span));
    }

    public static DiagnosticLocation CreateFrom(SyntaxToken token) => CreateFrom(token.GetLocation());

    public static implicit operator Location?(DiagnosticLocation location) => location.ToLocation();
}
