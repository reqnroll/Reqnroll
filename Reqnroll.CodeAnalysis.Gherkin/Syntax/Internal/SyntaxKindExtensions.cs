namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal static class SyntaxKindExtensions
{
    public static bool IsToken(this SyntaxKind kind)
    {
        return kind <= SyntaxKind.TextLiteralToken && kind >= SyntaxKind.ColonToken;
    }

    public static bool IsTextlessToken(this SyntaxKind kind)
    {
        return kind <= SyntaxKind.EndOfFileToken && kind >= SyntaxKind.ColonToken;
    }
}
