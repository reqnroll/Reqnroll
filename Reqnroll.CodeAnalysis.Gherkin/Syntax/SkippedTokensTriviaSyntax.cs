namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// A structured trivia syntax node which represents one or more tokens that were skipped by the parser.
/// </summary>
[SyntaxNode(SyntaxKind.SkippedTokensTrivia)]
public class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkippedTokensTriviaSyntax"/> class which is orphaned.
    /// </summary>
    /// <param name="trivia">A trivia which represents this syntax.</param>
    /// <param name="node">The internal node to encapsulate.</param>
    internal SkippedTokensTriviaSyntax(InternalSkippedTokensTriviaSyntax node) : 
        this(node, default, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SkippedTokensTriviaSyntax"/> class which is a child of 
    /// a specified parent node.
    /// </summary>
    /// <param name="trivia">A trivia which represents this syntax.</param>
    /// <param name="node">The internal node to encapsulate.</param>
    /// <param name="position">The position of this node.</param>
    internal SkippedTokensTriviaSyntax(
        InternalSkippedTokensTriviaSyntax node,
        SyntaxTrivia trivia,
        int position) :
        base(node, trivia, position)
    {
    }

    private new InternalSkippedTokensTriviaSyntax InternalNode => (InternalSkippedTokensTriviaSyntax)base.InternalNode;

    /// <summary>
    /// Gets the tokens which were skipped.
    /// </summary>
    public SyntaxTokenList Tokens => new(InternalNode.tokens, this, InternalNode.GetSlotOffset(0));

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index) => null;
}
