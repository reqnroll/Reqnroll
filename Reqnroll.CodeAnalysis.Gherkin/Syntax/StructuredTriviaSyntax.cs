using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a trivia syntax node which has structure.
/// </summary>
public abstract class StructuredTriviaSyntax : SyntaxNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StructuredTriviaSyntax"/> class which is orphaned.
    /// </summary>
    /// <param name="trivia">A trivia which represents this syntax.</param>
    /// <param name="node">The internal node to encapsulate.</param>
    internal StructuredTriviaSyntax(InternalStructuredTriviaSyntax node) :
        this(node, new SyntaxTrivia(default, node, 0), 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StructuredTriviaSyntax"/> class which is associated with
    /// a <see cref="SyntaxTrivia"/> value in a syntax tree.
    /// </summary>
    /// <param name="trivia">A trivia which represents this syntax.</param>
    /// <param name="node">The internal node to encapsulate.</param>
    /// <param name="parent">The node which is the parent of this node.</param>
    /// <param name="position">The position of this node.</param>
    internal StructuredTriviaSyntax(
        InternalStructuredTriviaSyntax node,
        SyntaxTrivia trivia,
        int position) : 
        base(node, trivia.Token.Parent, position)
    {
        ParentTrivia = trivia;
    }

    /// <summary>
    /// Gets the trivia which represents this syntax.
    /// </summary>
    public SyntaxTrivia ParentTrivia { get; }
}
