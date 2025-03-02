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
        var stepBindings = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Reqnroll.WhenAttribute",
                static (_, _) => true,
                static (context, _) => GetStepBindingInfo(context, StepKeyword.When))
            .SelectMany(static (info, _) => info); 

        context.RegisterSourceOutput(stepBindings, (context, stepBinding) =>
        {
            foreach (var diagnostic in stepBinding.Diagnostics)
            {
                context.ReportDiagnostic(diagnostic.CreateDiagnostic());
            }
        });
    }

    private static IEnumerable<StepBindingInfo> GetStepBindingInfo(
        GeneratorAttributeSyntaxContext context,
        StepKeyword keyword)
    {
        var method = (MethodDeclarationSyntax)context.TargetNode;

        foreach (var attribute in context.Attributes)
        {
            // If the attribute candidate is an error symbol, skip it.
            if (attribute.AttributeClass == null || attribute.AttributeClass.Kind == SymbolKind.ErrorType)
            {
                continue;
            }

            // The first argument is the step text.
            var textConstant = attribute.ConstructorArguments[0];
            var text = (string?)textConstant.Value;

            if (string.IsNullOrEmpty(text))
            {
                var location = DiagnosticLocation.None;

                var attributeSyntax = attribute.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;

                if (attributeSyntax?.ArgumentList != null)
                {
                    foreach (var argument in attributeSyntax.ArgumentList.Arguments)
                    {
                        if (argument.NameEquals != null)
                        {
                            continue;
                        }

                        if (argument.NameColon == null)
                        {
                            location = DiagnosticLocation.CreateFrom(argument.Expression);
                            break;
                        }

                        if (argument.NameColon.Name.Identifier.Text == "regex")
                        {
                            location = DiagnosticLocation.CreateFrom(argument.Expression);
                            break;
                        }
                    }
                }

                var diagnostic = new DiagnosticInfo(
                    DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                    location);

                yield return new StepBindingInfo(keyword, text, ImmutableCollection.Create(diagnostic));
            }
        }
    }
}

internal enum StepKeyword
{
    Given,
    When,
    Then,
    Any
}

internal static class ImmutableCollection
{
    public static ImmutableCollection<T> Create<T>(T item)
    {
        return new ImmutableCollection<T>(ImmutableArray.Create(item));
    }

    public static ImmutableCollection<T> Create<T>(ReadOnlySpan<T> values)
    {
        var builder = ImmutableArray.CreateBuilder<T>(values.Length);

        foreach (var item in values)
        {
            builder.Add(item);
        }

        return new ImmutableCollection<T>(builder.ToImmutable());
    }
}

internal readonly struct ImmutableCollection<T>(ImmutableArray<T> values) : IEquatable<ImmutableCollection<T>>, IEnumerable<T>
{
    private readonly ImmutableArray<T> _values = values;

    public int Length => _values.Length;

    public override bool Equals(object obj)
    {
        if (obj is ImmutableCollection<T> other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(ImmutableCollection<T> other)
    {
        if (_values.IsDefaultOrEmpty)
        {
            return other._values.IsDefaultOrEmpty;
        }

        if (other._values.IsDefaultOrEmpty)
        {
            return false;
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
}

internal record StepBindingInfo(
    StepKeyword Keyword,
    string? Text,
    ImmutableCollection<DiagnosticInfo> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public bool HasDiagnostics => Diagnostics.Length > 0;
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

    public static DiagnosticLocation CreateFrom(SyntaxNode node) => CreateFrom(node.GetLocation());

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

    public static implicit operator Location?(DiagnosticLocation location) => location.ToLocation();
}
