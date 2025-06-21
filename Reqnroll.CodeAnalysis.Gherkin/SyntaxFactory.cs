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

    /// <summary>
    /// A trivia with kind <see cref="SyntaxKind.EndOfLineTrivia"/> characters which match the current environment's newline.
    /// </summary>
    public static SyntaxTrivia EnvironmentNewLine => Environment.NewLine == "\r\n"
        ? CarriageReturnLineFeed
        : LineFeed;

    public static SyntaxTrivia Space { get; } = InternalSyntaxFactory.Space;

    public static GherkinDocumentSyntax GherkinDocument() => GherkinDocument(null);

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

    public static SyntaxToken DirectiveIdentifier(string text) =>
        InternalSyntaxFactory.DirectiveIdentifier(null, text, null);

    public static SyntaxToken DirectiveIdentifier(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.DirectiveIdentifier(leading.InternalNode, text, trailing.InternalNode);

    public static SyntaxToken Literal(string value) =>
        InternalSyntaxFactory.Literal(null, LiteralEscapingStyle.Default.Escape(value), value, null);

    public static SyntaxToken Literal(SyntaxTriviaList leading, string value, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.Literal(
            leading.InternalNode,
            LiteralEscapingStyle.Default.Escape(value),
            value,
            trailing.InternalNode);

    public static SyntaxToken Literal(string text, string value) =>
        InternalSyntaxFactory.Literal(null, text, value, null);

    public static SyntaxToken Literal(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.Literal(leading.InternalNode, text, value, trailing.InternalNode);

    public static LiteralTextSyntax LiteralText(string text) => LiteralText(TokenList([Literal(text)]));

    public static LiteralTextSyntax LiteralText(SyntaxToken text) => LiteralText(TokenList([text]));

    /// <summary>
    /// Creates a new <see cref="FeatureSyntax"/> instance.
    /// </summary>
    /// <param name="featureKeyword">The "Feature" keyword.</param>
    /// <param name="name">The name of the feature.</param>
    /// <returns>A new <see cref="FeatureSyntax"/> instance.</returns>
    public static FeatureSyntax Feature(string featureKeyword, string name) => Feature(
        Token(SyntaxKind.FeatureKeyword, featureKeyword),
        LiteralText(name));

    /// <summary>
    /// Creates a new <see cref="ScenarioSyntax"/> instance.
    /// </summary>
    /// <param name="scenarioKeyword">The token that represents the "Scenario" keyword.</param>
    /// <param name="name">The name of the scenario.</param>
    /// <param name="steps">The steps of the scenario.</param>
    /// <returns>A new <see cref="ScenarioSyntax"/> instance.</returns>
    public static ScenarioSyntax Scenario(
        string scenarioKeyword,
        string name,
        SyntaxList<StepSyntax> steps = default) => Scenario(
            Token(SyntaxKind.ScenarioKeyword, scenarioKeyword),
            LiteralText(name),
            steps: steps);

    public static SyntaxToken StepTextLiteral(string text) =>
        InternalSyntaxFactory.StepTextLiteral(null, text, null);

    public static SyntaxToken StepTextLiteral(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.StepTextLiteral(leading.InternalNode, text, trailing.InternalNode);

    public static SyntaxToken TableLiteral(string value) =>
        InternalSyntaxFactory.TableLiteral(null, LiteralEscapingStyle.Table.Escape(value), value, null);

    public static SyntaxToken TableLiteral(SyntaxTriviaList leading, string value, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.TableLiteral(
            leading.InternalNode,
            LiteralEscapingStyle.Table.Escape(value),
            value,
            trailing.InternalNode);

    public static SyntaxToken TableLiteral(string text, string value) =>
        InternalSyntaxFactory.TableLiteral(null, text, value, null);

    public static SyntaxToken TableLiteral(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing) =>
        InternalSyntaxFactory.TableLiteral(leading.InternalNode, text, value, trailing.InternalNode);

    public static SyntaxTriviaList TriviaList() => default;

    public static SyntaxTriviaList TriviaList(SyntaxTrivia trivia) => new(trivia);

    public static SyntaxTriviaList TriviaList(IEnumerable<SyntaxTrivia> trivias) => new(trivias);

    public static SyntaxTokenList TokenList() => new();

    public static SyntaxTokenList TokenList(SyntaxToken token) => new(token);

    public static SyntaxTokenList TokenList(IEnumerable<SyntaxToken> tokens) => new(tokens);

    public static SyntaxList<TNode> List<TNode>() where TNode : SyntaxNode => default;

    public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode => new(nodes);

    public static SeparatedSyntaxList<TNode> SeparatedList<TNode>()
        where TNode : SyntaxNode => new();

    public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<SyntaxNodeOrToken> nodes) 
        where TNode : SyntaxNode => new(nodes);

    public static SyntaxTrivia Whitespace(string text) => InternalSyntaxFactory.Whitespace(text);

    /// <summary>
    /// Creates a trivia with kind <see cref="SyntaxKind.CommentTrivia"/  containing the specified text.
    /// </summary>
    /// <param name="text">The entire text of the comment including the leading '#' token.</param>
    /// <returns>A <see cref="SyntaxTrivia"/> containing the specified text.</returns>
    public static SyntaxTrivia Comment(string text) => InternalSyntaxFactory.Comment(text);

    public static SkippedTokensTriviaSyntax SkippedTokensTrivia(SyntaxTokenList tokens) =>
        (SkippedTokensTriviaSyntax)InternalSyntaxFactory.SkippedTokensTrivia(tokens.InternalNode).CreateSyntaxNode();
}
