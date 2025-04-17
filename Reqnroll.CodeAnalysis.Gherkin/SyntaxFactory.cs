using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin;

using InternalSyntaxFactory = InternalSyntaxFactory;

public static partial class SyntaxFactory
{
    /// <summary>
    /// A token used to represent an opportunity to insert any amount of additional whitespace.
    /// </summary>
    public static SyntaxTrivia ElasticMarker { get; } = InternalSyntaxFactory.ElasticZeroSpace;

    /// <summary>
    /// A token used to represent a flexible space character which can be expanded to contain additional whitespace.
    /// </summary>
    public static SyntaxTrivia ElasticSpace { get; } = InternalSyntaxFactory.ElasticSpace;

    /// <summary>
    /// A trivia with kind <see cref="SyntaxKind.EndOfLineTrivia"/> containing a single line feed character.
    /// </summary>
    public static SyntaxTrivia LineFeed { get; } = InternalSyntaxFactory.LineFeed;

    /// <summary>
    /// An elastic trivia with kind <see cref="SyntaxKind.EndOfLineTrivia"/> containing both the carriage return and line feed characters.
    /// Elastic trivia are used to denote trivia that was not produced by parsing source text, and are usually not
    /// preserved during formatting.
    /// </summary>
    public static SyntaxTrivia ElasticCarriageReturnLineFeed { get; } = InternalSyntaxFactory.ElasticCarriageReturnLineFeed;

    /// <summary>
    /// A trivia with kind <see cref="SyntaxKind.EndOfLineTrivia"/> containing both the carriage return and line feed characters.
    /// </summary>
    public static SyntaxTrivia CarriageReturnLineFeed { get; } = InternalSyntaxFactory.ElasticCarriageReturnLineFeed;

    public static SyntaxTrivia Space { get; } = InternalSyntaxFactory.Space;

    public static GherkinDocumentSyntax GherkinDocument() => GherkinDocument(null);

    public static DescriptionSyntax Description(SyntaxToken text) => new(InternalSyntaxFactory.Description(text.InternalNode));

    /// <summary>
    /// Creates a trivia from a <see cref="StructuredTriviaSyntax"/> node.
    /// </summary>
    /// <param name="node">The structured trivia to create the syntax from.</param>
    /// <returns></returns>
    public static SyntaxTrivia Trivia(StructuredTriviaSyntax node) => 
        new(default, (InternalStructuredTriviaSyntax)node.InternalNode, position: 0);

    public static SyntaxToken MissingToken(SyntaxKind kind) => 
        InternalSyntaxFactory.MissingToken(ElasticMarker.InternalNode, kind, ElasticMarker.InternalNode);

    public static SyntaxToken MissingToken(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing) => 
        InternalSyntaxFactory.MissingToken(leading.InternalNode, kind, trailing.InternalNode);

    public static SyntaxToken Token(SyntaxKind kind) => 
        InternalSyntaxFactory.Token(ElasticMarker.InternalNode, kind, ElasticMarker.InternalNode);

    public static SyntaxToken Token(SyntaxKind kind, string text) =>
        InternalSyntaxFactory.Token(ElasticMarker.InternalNode, kind, text, ElasticMarker.InternalNode);

    public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing) => 
        InternalSyntaxFactory.Token(leading.InternalNode, kind, trailing.InternalNode);

    public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, string text, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.Token(leading.InternalNode, kind, text, trailing.InternalNode);

    public static SyntaxToken Identifier(string text) =>
        InternalSyntaxFactory.Identifier(null, text, null);

    public static SyntaxToken Identifier(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.Identifier(leading.InternalNode, text, trailing.InternalNode);

    public static SyntaxToken Literal(string text) =>
        InternalSyntaxFactory.Literal(null, text, null);

    public static SyntaxToken Literal(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.Literal(leading.InternalNode, text, trailing.InternalNode);

    public static SyntaxTriviaList TriviaList() => default;

    public static SyntaxTriviaList TriviaList(SyntaxTrivia trivia) => new(trivia);

    public static SyntaxTriviaList TriviaList(IEnumerable<SyntaxTrivia> trivias) => new(trivias);

    public static SyntaxTokenList TokenList() => new();

    public static SyntaxTokenList TokenList(SyntaxToken token) => new(token);

    public static SyntaxTokenList TokenList(IEnumerable<SyntaxToken> tokens) => new(tokens);

    public static SyntaxTrivia Whitespace(string text) => InternalSyntaxFactory.Whitespace(text);

    public static SyntaxTrivia Comment(string text) => InternalSyntaxFactory.Comment(text);

    public static SkippedTokensTriviaSyntax SkippedTokensTrivia(SyntaxTokenList tokens) =>
        (SkippedTokensTriviaSyntax)InternalSyntaxFactory.SkippedTokensTrivia(tokens.InternalNode).CreateSyntaxNode();
}
