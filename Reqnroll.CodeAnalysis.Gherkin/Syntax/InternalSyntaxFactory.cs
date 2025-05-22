using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal static partial class InternalSyntaxFactory
{
    public static readonly InternalSyntaxTrivia ElasticZeroSpace = Whitespace(string.Empty, true);
    public static readonly InternalSyntaxTrivia ElasticSpace = Whitespace(" ", true);

    private const string LineFeedText = "\n";
    public static readonly InternalSyntaxTrivia LineFeed = CreateEndOfLineTrivia(LineFeedText);
    public static readonly InternalSyntaxTrivia ElasticLineFeed = CreateEndOfLineTrivia(LineFeedText, true);

    private const string CarriageReturnLineFeedText = "\r\n";
    public static readonly InternalSyntaxTrivia CarriageReturnLineFeed = CreateEndOfLineTrivia(CarriageReturnLineFeedText);
    public static readonly InternalSyntaxTrivia ElasticCarriageReturnLineFeed = CreateEndOfLineTrivia(CarriageReturnLineFeedText, true);

    /// <summary>
    /// A token used to represent a single space character.
    /// </summary>
    public static readonly InternalSyntaxTrivia Space = Whitespace(" ");

    private static bool IsLineFeed(SourceText text, TextSpan span)
    {
        return span.Length == 1 && text[span.Start] == '\n';
    }

    private static bool IsCarriageReturnLineFeed(SourceText text, TextSpan span)
    {
        return span.Length == 2 && text[span.Start] == '\r' && text[span.Start + 1] == '\n';
    }

    public static InternalSyntaxTrivia EndOfLine(string text, bool elastic = false)
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

    public static InternalSyntaxTrivia CreateEndOfLineTrivia(string text, bool elastic = false)
    {
        var trivia = new InternalSyntaxTrivia(SyntaxKind.EndOfLineTrivia, text);

        if (elastic)
        {
            trivia = (InternalSyntaxTrivia)trivia.WithAnnotations(SyntaxAnnotation.ElasticAnnotation);
        }

        return trivia;
    }

    public static InternalSyntaxTrivia EndOfLine(SourceText text, TextSpan span)
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

        return new InternalSyntaxTrivia(SyntaxKind.EndOfLineTrivia, GetSourceTextValue(text, span));
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

    public static InternalSyntaxTrivia Whitespace(string text, bool elastic = false)
    {
        var node = new InternalSyntaxTrivia(SyntaxKind.WhitespaceTrivia, text);

        if (elastic)
        {
            return (InternalSyntaxTrivia)node.WithAnnotations(SyntaxAnnotation.ElasticAnnotation);
        }

        return node;
    }

    public static InternalSyntaxTrivia? Whitespace(SourceText text, TextSpan span)
    {
        if (span.IsEmpty)
        {
            return null;
        }

        var whitespace = text.ToString(span);

        Debug.Assert(whitespace.All(char.IsWhiteSpace), "Non-whitespace characters found in whitespace text.");

        return new(SyntaxKind.WhitespaceTrivia, whitespace);
    }

    public static InternalSyntaxTrivia Comment(string text)
    {
        return new InternalSyntaxTrivia(SyntaxKind.CommentTrivia, text);
    }

    public static InternalSyntaxToken Token(SyntaxKind kind) => Token(null, kind, null);

    public static InternalSyntaxToken Token(SyntaxKind kind, string text) => Token(null, kind, text, null);

    public static InternalSyntaxToken Token(
        InternalNode? leadingTrivia,
        SyntaxKind kind,
        InternalNode? trailingTrivia) => InternalSyntaxToken.Create(kind, leadingTrivia, trailingTrivia);

    public static InternalSyntaxToken Token(
        InternalNode? leadingTrivia,
        SyntaxKind kind,
        string text,
        InternalNode? trailingTrivia) => InternalSyntaxToken.Create(kind, text, leadingTrivia, trailingTrivia);

    public static InternalSyntaxToken Literal(InternalNode? leadingTrivia, string text, InternalNode? trailingTrivia) => 
        InternalSyntaxToken.Create(SyntaxKind.TextLiteralToken, text, leadingTrivia, trailingTrivia);

    public static InternalSyntaxToken MissingToken(SyntaxKind kind) => InternalSyntaxToken.CreateMissing(
        kind,
        ElasticZeroSpace,
        ElasticZeroSpace);

    public static InternalSyntaxToken MissingToken(
        InternalNode? leadingTrivia,
        SyntaxKind kind,
        InternalNode? trailingTrivia) => InternalSyntaxToken.CreateMissing(kind, leadingTrivia, trailingTrivia);

    public static InternalSyntaxToken Identifier(InternalNode? leadingTrivia, string text, InternalNode? trailingTrivia) =>
        InternalSyntaxToken.Create(SyntaxKind.IdentifierToken, text, leadingTrivia, trailingTrivia);

    /// <summary>
    /// Reads characters from source text as whitespace trivia.
    /// </summary>
    /// <param name="text">The text to read from.</param>
    /// <param name="start">The start index at which to begin reading.</param>
    /// <param name="length">The number of characters to read.</param>
    /// <returns>A <see cref="InternalSyntaxTrivia"/> representing the read whitespace, 
    /// or <c>null</c> if <paramref name="length"/> is 0.</returns>
    public static InternalSyntaxTrivia? Whitespace(SourceText text, int start, int length)
    {
        if (length == 0)
        {
            return null;
        }

        var whitespace = text.ToString(new TextSpan(start, length));

        Debug.Assert(whitespace.All(char.IsWhiteSpace), "Non-whitespace characters found in whitespace text.");

        return new(SyntaxKind.WhitespaceTrivia, whitespace);
    }

    public static InternalSkippedTokensTriviaSyntax SkippedTokensTrivia(InternalNode? tokens) => new(tokens);
}
