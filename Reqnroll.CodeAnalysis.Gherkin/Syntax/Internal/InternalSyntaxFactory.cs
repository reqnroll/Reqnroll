using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal static class InternalSyntaxFactory
{
    public static readonly RawSyntaxTrivia ElasticZeroSpace = Whitespace(string.Empty, true);
    public static readonly RawSyntaxTrivia ElasticSpace = Whitespace(" ", true);

    private const string LineFeedText = "\n";
    public static readonly RawSyntaxTrivia LineFeed = CreateEndOfLineTrivia(LineFeedText);
    public static readonly RawSyntaxTrivia ElasticLineFeed = CreateEndOfLineTrivia(LineFeedText, true);

    private const string CarriageReturnLineFeedText = "\r\n";
    public static readonly RawSyntaxTrivia CarriageReturnLineFeed = CreateEndOfLineTrivia(CarriageReturnLineFeedText);
    public static readonly RawSyntaxTrivia ElasticCarriageReturnLineFeed = CreateEndOfLineTrivia(CarriageReturnLineFeedText, true);

    /// <summary>
    /// A token used to represent a single space character.
    /// </summary>
    public static readonly RawSyntaxTrivia Space = Whitespace(" ");

    private static bool IsLineFeed(SourceText text, TextSpan span)
    {
        return span.Length == 1 && text[span.Start] == '\n';
    }

    private static bool IsCarriageReturnLineFeed(SourceText text, TextSpan span)
    {
        return span.Length == 2 && text[span.Start] == '\r' && text[span.Start + 1] == '\n';
    }

    public static RawSyntaxTrivia EndOfLine(string text, bool elastic = false)
    {
        if (text == CarriageReturnLineFeedText)
        {
            if (elastic)
            {
                return ElasticCarriageReturnLineFeed;
            }

            return CarriageReturnLineFeed;
        }

        if (text == LineFeedText)
        {
            if (elastic)
            {
                return ElasticLineFeed;
            }

            return LineFeed;
        }

        return CreateEndOfLineTrivia(text, elastic);
    }

    public static RawSyntaxTrivia CreateEndOfLineTrivia(string text, bool elastic = false)
    {
        var trivia = new RawSyntaxTrivia(SyntaxKind.EndOfLineTrivia, text);

        if (elastic)
        {
            trivia = (RawSyntaxTrivia)trivia.WithAnnotations(SyntaxAnnotation.ElasticAnnotation);
        }

        return trivia;
    }

    public static RawSyntaxTrivia EndOfLine(SourceText text, TextSpan span)
    {
        // The vast majority of line end tokens will be CRLF or LF.
        // We can intern all matching spans to the two instances we'll need.
        if (IsCarriageReturnLineFeed(text, span))
        {
            return CarriageReturnLineFeed;
        }

        if (IsLineFeed(text, span))
        {
            return LineFeed;
        }

        return new RawSyntaxTrivia(SyntaxKind.EndOfLineTrivia, GetSourceTextValue(text, span));
    }

    private static string GetSourceTextValue(SourceText text, TextSpan span)
    {
        if (span.Length < 1)
        {
            return string.Empty;
        }

        var buffer = new StringBuilder();

        for (var i = span.Start; i < span.End; i++)
        {
            buffer.Append(text[i]);
        }

        return buffer.ToString();
    }

    public static RawSyntaxTrivia Whitespace(string text, bool elastic = false)
    {
        var node = new RawSyntaxTrivia(SyntaxKind.WhitespaceTrivia, text);

        if (elastic)
        {
            return (RawSyntaxTrivia)node.WithAnnotations(SyntaxAnnotation.ElasticAnnotation);
        }

        return node;
    }

    public static RawSyntaxTrivia? Whitespace(SourceText text, TextSpan span)
    {
        if (span.IsEmpty)
        {
            return null;
        }

        var whitespace = text.ToString(span);

        Debug.Assert(whitespace.All(char.IsWhiteSpace), "Non-whitespace characters found in whitespace text.");

        return new(SyntaxKind.WhitespaceTrivia, whitespace);
    }

    public static RawSyntaxTrivia Comment(string text)
    {
        return new RawSyntaxTrivia(SyntaxKind.CommentTrivia, text);
    }

    public static RawSyntaxToken Token(SyntaxKind kind) => Token(null, kind, null);

    public static RawSyntaxToken Token(
        RawNode? leadingTrivia,
        SyntaxKind kind,
        RawNode? trailingTrivia) => RawSyntaxToken.Create(kind, leadingTrivia, trailingTrivia);

    public static RawSyntaxToken Token(
        RawNode? leadingTrivia,
        SyntaxKind kind,
        string text,
        RawNode? trailingTrivia) => RawSyntaxToken.Create(kind, text, leadingTrivia, trailingTrivia);

    public static RawSyntaxToken Literal(RawNode? leadingTrivia, string text, RawNode? trailingTrivia) => 
        RawSyntaxToken.Create(SyntaxKind.TextLiteralToken, text, leadingTrivia, trailingTrivia);

    public static RawSyntaxToken MissingToken(SyntaxKind kind) => RawSyntaxToken.CreateMissing(
        kind,
        ElasticZeroSpace,
        ElasticZeroSpace);

    public static RawSyntaxToken MissingToken(
        RawNode? leadingTrivia,
        SyntaxKind kind,
        RawNode? trailingTrivia) => RawSyntaxToken.CreateMissing(kind, leadingTrivia, trailingTrivia);

    public static FeatureDeclarationSyntax FeatureDeclaration(
        RawNode keyword,
        RawNode colon,
        RawNode name,
        DescriptionSyntax? description)
    {
        return new FeatureDeclarationSyntax(keyword, colon, name, description);
    }

    public static RawSyntaxToken Identifier(RawNode? leadingTrivia, string text, RawNode? trailingTrivia) =>
        RawSyntaxToken.Create(SyntaxKind.IdentifierToken, text, leadingTrivia, trailingTrivia);

    public static FeatureFileSyntax FeatureFile(RawNode? featureDeclaration, RawNode endOfFileToken)
    {
        return new(featureDeclaration, endOfFileToken);
    }

    /// <summary>
    /// Reads characters from source text as whitespace trivia.
    /// </summary>
    /// <param name="text">The text to read from.</param>
    /// <param name="start">The start index at which to begin reading.</param>
    /// <param name="length">The number of characters to read.</param>
    /// <returns>A <see cref="RawSyntaxTrivia"/> representing the read whitespace, 
    /// or <c>null</c> if <paramref name="length"/> is 0.</returns>
    public static RawSyntaxTrivia? Whitespace(SourceText text, int start, int length)
    {
        if (length == 0)
        {
            return null;
        }

        var whitespace = text.ToString(new TextSpan(start, length));

        Debug.Assert(whitespace.All(char.IsWhiteSpace), "Non-whitespace characters found in whitespace text.");

        return new(SyntaxKind.WhitespaceTrivia, whitespace);
    }

    public static DescriptionSyntax Description(RawNode? textNodes)
    {
        return new(textNodes);
    }

    public static SkippedTokensTriviaSyntax SkippedTokensTrivia(RawNode? tokens) => new(tokens);
}
