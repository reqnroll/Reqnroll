using Microsoft.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal static class SymbolExtensions
{
    public static bool IsSyntaxNode(this ITypeSymbol symbol)
    {
        if (symbol.BaseType == null)
        {
            return false;
        }

        if (symbol.BaseType.ToDisplayString() == "Reqnroll.CodeAnalysis.Gherkin.Syntax.SyntaxNode")
        {
            return true;
        }

        return symbol.BaseType.IsSyntaxNode();
    }

    public static bool IsSyntaxList(this ITypeSymbol symbol)
    {
        if (symbol.OriginalDefinition == null)
        {
            return false;
        }

        return symbol.OriginalDefinition.ToDisplayString() == "Reqnroll.CodeAnalysis.Gherkin.Syntax.SyntaxList<TNode>";
    }
}
