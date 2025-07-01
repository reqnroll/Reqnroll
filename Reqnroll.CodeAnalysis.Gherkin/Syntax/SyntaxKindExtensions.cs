namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal static class SyntaxKindExtensions
{
    public static bool IsToken(this SyntaxKind kind)
    {
        return kind <= SyntaxKind.LiteralToken && kind >= SyntaxKind.ColonToken;
    }

    public static bool IsTextlessToken(this SyntaxKind kind)
    {
        return kind <= SyntaxKind.EndOfFileToken && kind >= SyntaxKind.ColonToken;
    }
}
