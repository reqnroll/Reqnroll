using FluentAssertions.Equivalency;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Assertions;

public static class SyntaxEquivalencyAssertionOptionExtensions
{
    public static EquivalencyAssertionOptions<T> IgnoringSyntaxPositions<T>(this EquivalencyAssertionOptions<T> options)
    {
        options.Excluding(member => member.DeclaringType == typeof(SyntaxNode) &&
            (member.Name == nameof(SyntaxNode.Span) || member.Name == nameof(SyntaxNode.FullSpan)));

        options.Excluding(member => member.DeclaringType == typeof(SyntaxToken) &&
             (member.Name == nameof(SyntaxToken.Span) || member.Name == nameof(SyntaxToken.FullSpan)));

        options.Excluding(member => member.DeclaringType == typeof(SyntaxTrivia) &&
             (member.Name == nameof(SyntaxTrivia.Span) || member.Name == nameof(SyntaxTrivia.FullSpan)));

        return options;
    }
}
