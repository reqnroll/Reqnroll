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
                .ComparingByMembers<SyntaxTrivia>()
                .ComparingByMembers<Microsoft.CodeAnalysis.Diagnostic>()
                .ComparingByMembers<TableCellSyntaxList>()
                .WithStrictOrderingFor(obj => 
                    obj.GetType().IsGenericType && 
                    obj.GetType().GetGenericTypeDefinition() == typeof(SyntaxList<>))
                .Using(new SyntaxNodeOrTokenStep())
                .Excluding(member => member.DeclaringType == typeof(SyntaxNode) &&
                    member.Name == nameof(SyntaxNode.Parent))
                .Excluding(member => member.DeclaringType == typeof(SyntaxToken) &&
                    member.Name == nameof(SyntaxToken.Parent))
                .Excluding(member => member.DeclaringType == typeof(SyntaxToken) &&
                    member.Name == nameof(SyntaxToken.SyntaxTree))
                .Excluding(member => member.DeclaringType == typeof(SyntaxTrivia) &&
                    member.Name == nameof(SyntaxTrivia.Token))
                .Excluding(member => member.DeclaringType == typeof(StructuredTriviaSyntax) &&
                    member.Name == nameof(StructuredTriviaSyntax.ParentTrivia))
                .Excluding(member => member.Type == typeof(GherkinSyntaxTree))
                .RespectingRuntimeTypes());
    }
}
