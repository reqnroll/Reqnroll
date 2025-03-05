using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Reqnroll.StepBindingSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CSharpStepBindingGenerator : IIncrementalGenerator
{
    private static readonly Regex LeadingWhitespacePattern = new(@"^\s+", RegexOptions.Compiled);
    private static readonly Regex TrailingWhitespacePattern = new(@"\s+$", RegexOptions.Compiled);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var stepBindings = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Reqnroll.WhenAttribute",
                static (_, _) => true,
                static (context, cancellationToken) => GetStepBindingInfo(context, StepKeyword.When, cancellationToken))
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
        StepKeyword keyword,
        CancellationToken cancellationToken)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax);

        // If the semantic model does not contain the method, we can't bind to it.
        // This usually indicates a problem with the general syntax of this method.
        if (methodSymbol == null)
        {
            yield break;
        }

        foreach (var attribute in context.Attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If the attribute candidate is an error symbol, skip it.
            if (attribute.AttributeClass == null || attribute.AttributeClass.Kind == SymbolKind.ErrorType)
            {
                continue;
            }

            var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();

            StepBindingInfo CreateBindingInfo(BindingStyle style, string? text)
            {
                return new StepBindingInfo(
                    style,
                    keyword,
                    text,
                    methodSymbol.Name,
                    methodSymbol.ContainingType.Name,
                    methodSymbol.ContainingType.ContainingNamespace.Name,
                    new ImmutableCollection<DiagnosticInfo>(diagnostics.ToImmutable()));
            }

            // If the attribute candidate does not have any arguments, we're matching based on the method name.
            if (attribute.ConstructorArguments.Length == 0)
            {
                yield return CreateBindingInfo(BindingStyle.MethodName, null);
                continue;
            }

            // If the attribute has any arguments, the first is the step text.
            var textConstant = attribute.ConstructorArguments[0];
            var text = (string?)textConstant.Value;

            // Any further diagnostics will want to refer to the step text location with precision.
            // Unfortunately Roslyn generator doesn't give us the syntax nodes for the attribute arguments,
            // so we have to examine the attribute syntax to find the matching literal node.
            var textExpressionSyntax = GetStepTextExpressionSyntax(attribute);
            var textLocation = DiagnosticLocation.CreateFrom(
                textExpressionSyntax ?? attribute.ApplicationSyntaxReference?.GetSyntax());

            // Empty step text is not allowed (null, empty string or whitespace).
            if (string.IsNullOrWhiteSpace(text))
            {
                diagnostics.Add(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                        textLocation));

                // There's not really any text to bind to, but we still need to return a binding style
                // and it's definitely not "Method Name", so we'll use the Reqnroll default for text.
                yield return CreateBindingInfo(BindingStyle.CucumberExpression, text);
                continue;
            }

            // Determine whether to bind the non-empty text as a Cucumber expression or a regular expression.
            // The default for Reqnroll is Cucumber expression.
            var style = CucumberExpressionDetector.IsCucumberExpression(text!) ?
                BindingStyle.CucumberExpression :
                BindingStyle.RegularExpression;

            CheckStepTextForLeadingWhitespace(text!, textExpressionSyntax, textLocation, diagnostics);
            CheckStepTextForTrailingWhitespace(text!, textExpressionSyntax, textLocation, diagnostics);

            yield return CreateBindingInfo(style, text);
        }
    }

    private static void CheckStepTextForTrailingWhitespace(
        string text,
        ExpressionSyntax? textExpressionSyntax,
        DiagnosticLocation textLocation,
        ImmutableArray<DiagnosticInfo>.Builder diagnostics)
    {
        var trailingWhitespaceMatch = TrailingWhitespacePattern.Match(text);
        if (trailingWhitespaceMatch.Success)
        {
            var location = textLocation;

            if (textExpressionSyntax != null)
            {
                var expressionLocation = textExpressionSyntax.GetLocation();

                var path = expressionLocation.SourceTree!.FilePath;
                var span = expressionLocation.SourceSpan;
                var lineSpan = expressionLocation.GetLineSpan().Span;

                location = new DiagnosticLocation(
                    path,
                    TextSpan.FromBounds((span.End - 1) - trailingWhitespaceMatch.Length, span.End - 1),
                    new LinePositionSpan(
                        new LinePosition(lineSpan.End.Line, (lineSpan.End.Character - 1) - trailingWhitespaceMatch.Length),
                        new LinePosition(lineSpan.End.Line, lineSpan.End.Character - 1)));
            }

            diagnostics.Add(
                new DiagnosticInfo(
                    DiagnosticDescriptors.WarningStepTextHasTrailingWhitespace,
                    location));
        }
    }

    private static void CheckStepTextForLeadingWhitespace(
        string text,
        ExpressionSyntax? textExpressionSyntax,
        DiagnosticLocation textLocation,
        ImmutableArray<DiagnosticInfo>.Builder diagnostics)
    {
        var leadingWhitespaceMatch = LeadingWhitespacePattern.Match(text);
        if (leadingWhitespaceMatch.Success)
        {
            var location = textLocation;

            if (textExpressionSyntax != null)
            {
                var expressionLocation = textExpressionSyntax.GetLocation();

                var path = expressionLocation.SourceTree!.FilePath;
                var span = expressionLocation.SourceSpan;
                var lineSpan = expressionLocation.GetLineSpan().Span;

                location = new DiagnosticLocation(
                    path,
                    new TextSpan(span.Start + 1, leadingWhitespaceMatch.Length),
                    new LinePositionSpan(
                        new LinePosition(lineSpan.Start.Line, lineSpan.Start.Character + 1),
                        new LinePosition(lineSpan.Start.Line, lineSpan.Start.Character + 1 + leadingWhitespaceMatch.Length)));
            }

            diagnostics.Add(
                new DiagnosticInfo(
                    DiagnosticDescriptors.WarningStepTextHasLeadingWhitespace,
                    location));
        }
    }

    private static ExpressionSyntax? GetStepTextExpressionSyntax(AttributeData attribute)
    {
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
                    return argument.Expression;
                }

                if (argument.NameColon.Name.Identifier.Text == "regex")
                {
                    return argument.Expression;
                }
            }
        }

        return null;
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

    public static ImmutableCollection<T> Empty { get; } = new ImmutableCollection<T>(ImmutableArray<T>.Empty);

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
    BindingStyle BindingStyle,
    StepKeyword Keyword,
    string? Text,
    string MethodName,
    string DeclaringClassName,
    string DeclaringClassNamespace,
    ImmutableCollection<DiagnosticInfo> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public bool HasDiagnostics => Diagnostics.Length > 0;
}

internal enum BindingStyle
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

    public static implicit operator Location?(DiagnosticLocation location) => location.ToLocation();
}
