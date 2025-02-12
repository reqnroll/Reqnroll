using FluentAssertions;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using System.Runtime.CompilerServices;

namespace Reqnroll.CodeAnalysis.Gherkin.Assertions;

internal static class AssertionOptionsInitializer
{
    [ModuleInitializer]
    public static void SetAssertionOptions()
    {
        AssertionOptions.AssertEquivalencyUsing(
            options => options
                .ComparingByMembers<SyntaxNode>()
                .ComparingByMembers<SyntaxToken>()
                .Excluding(member => member.DeclaringType == typeof(SyntaxNode) && member.Name == nameof(SyntaxNode.Parent))
                .Excluding(member => member.DeclaringType == typeof(SyntaxToken) && member.Name == nameof(SyntaxToken.Parent))
                .Excluding(member => member.DeclaringType == typeof(SyntaxToken) && member.Name == nameof(SyntaxToken.SyntaxTree))
                .Using(new SyntaxTriviaEquivalenceComparer()));
    }
}
