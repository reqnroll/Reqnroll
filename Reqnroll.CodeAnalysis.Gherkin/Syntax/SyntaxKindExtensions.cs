namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal static class SyntaxKindExtensions
{
    public static bool IsToken(this SyntaxKind kind)
    {
        return kind <= SyntaxKind.DocStringDelimiterToken && kind >= SyntaxKind.ColonToken;
    }

    public static bool IsLiteralToken(this SyntaxKind kind)
    {
        return kind <= SyntaxKind.DocStringContentTypeIdentifierToken && kind >= SyntaxKind.DirectiveIdentifierToken;
    }

    public static bool IsTextlessToken(this SyntaxKind kind)
    {
        return kind <= SyntaxKind.EndOfFileToken && kind >= SyntaxKind.ColonToken;
    }
}
