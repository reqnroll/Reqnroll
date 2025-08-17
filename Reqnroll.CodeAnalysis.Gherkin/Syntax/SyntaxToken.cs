using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a token in a syntax tree, such as a keyword or a block of text.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public readonly struct SyntaxToken : IEquatable<SyntaxToken>
{
    internal SyntaxToken(InternalNode? syntaxToken, SyntaxNode? parent, int position)
    {
        InternalNode = syntaxToken;
        Parent = parent;
        Position = position;
    }

    internal SyntaxToken(InternalNode? token)
    {
        InternalNode = token;
    }

    /// <summary>
    /// Gets the syntax tree that owns this token.
    /// </summary>
    public GherkinSyntaxTree? SyntaxTree => Parent?.SyntaxTree;

    /// <summary>
    /// Gets the absolute span of this node in characters, including its leading and trailing trivia.
    /// </summary>
    public TextSpan FullSpan
    {
        get
        {
            if (InternalNode == null)
            {
                return default;
            }

            return new(Position, InternalNode.FullWidth);
        }
    }

    /// <summary>
    /// Determines whether this node has any leading trivia.
    /// </summary>
    public bool HasLeadingTrivia => InternalNode?.HasLeadingTrivia ?? false;

    /// <summary>
    /// Determines whether this node has any leading trivia.
    /// </summary>
    public bool HasTrailingTrivia => InternalNode?.HasTrailingTrivia ?? false;

    /// <summary>
    /// Gets the absolute span of this node in characters, not including its leading and trailing trivia.
    /// </summary>
    public TextSpan Span
    {
        get
        {
            if (InternalNode == null)
            {
                return default;
            }

            return TextSpan.FromBounds(
                Position + InternalNode.GetLeadingTriviaWidth(),
                (Position + InternalNode.FullWidth) - InternalNode.GetTrailingTriviaWidth());

        }
    }

    private int FullWidth => InternalNode?.FullWidth ?? 0;

    /// <summary>
    /// Gets the node that is the parent of this node.
    /// </summary>
    public SyntaxNode? Parent { get; }

    /// <summary>
    /// Gets the text represented by the token.
    /// </summary>
    public string Text => ToString();

    /// <summary>
    /// Gets the value of the token. For example, the value of a tag token is the name of the tag.
    /// </summary>
    public object? Value => InternalNode?.GetValue();

    /// <summary>
    /// Gets what kind of language construct is represented by this token.
    /// </summary>
    public SyntaxKind Kind => InternalNode?.Kind ?? SyntaxKind.None;

    /// <summary>
    /// Gets whether the syntax token represents a token that was actually parsed from source code. Missing
    /// tokens are generate by the parser in error scenarios to represent constructs that should have been
    /// present in the source code but were actually missing.
    /// </summary>
    public bool IsMissing => InternalNode?.IsMissing ?? false;

    internal InternalNode? InternalNode { get; }

    internal int Position { get; }

    public SyntaxTriviaList LeadingTrivia
    {
        get
        {
            if (InternalNode == null)
            {
                return default;
            }

            return new SyntaxTriviaList(this, InternalNode.GetLeadingTrivia(), Position);
        }
    }

    public SyntaxTriviaList TrailingTrivia
    {
        get
        {
            if (InternalNode == null)
            {
                return default;
            }

            var position = Position + FullWidth;

            var trivia = InternalNode.GetTrailingTrivia();

            if (trivia != null)
            {
                position -= trivia.FullWidth;
            }

            return new SyntaxTriviaList(this, trivia, position);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the token has any diagnostics on it or its associated trivia.
    /// </summary>
    /// <value><c>true</c> if the token or its trivia has diagnostics; otherwise <c>false</c>.</value>
    public bool ContainsDiagnostics => InternalNode?.ContainsDiagnostics ?? false;

    private string GetDebuggerDisplay()
    {
        return GetType().Name + " " + Kind.ToString() + " " + ToString();
    }

    /// <summary>
    /// Gets a list of all the diagnostics associated with this token.
    /// </summary>
    /// <returns>A list of the diagnostics for this token.</returns>
    public IEnumerable<Diagnostic> GetDiagnostics()
    {
        if (InternalNode == null)
        {
            return [];
        }

        var tree = SyntaxTree;

        if (tree != null)
        {
            return tree.GetDiagnostics(this);
        }

        if (InternalNode.ContainsDiagnostics)
        {
            return InternalNode.GetAttachedDiagnostics().Select(diag => diag.CreateDiagnostic());
        }

        return [];
    }

    /// <summary>
    /// Determines whether this token is equivalent to a specified token.
    /// </summary>
    /// <param name="token">The token to compare this token to.</param>
    /// <returns><c>true</c> if this token and the specified token represent the same token and trivia; 
    /// otherwise <c>false</c>.</returns>
    public bool IsEquivalentTo(SyntaxToken token)
    {
        if (InternalNode == null)
        {
            return token.InternalNode == null;
        }

        return InternalNode.IsEquivalentTo(token.InternalNode);
    }

    /// <summary>
    /// Gets the location of this syntax token in the source document.
    /// </summary>
    /// <returns>The location of this syntax token.</returns>
    public Location GetLocation()
    {
        if (SyntaxTree == null)
        {
            return Location.None;
        }

        return SyntaxTree.GetLocation(Span);
    }

    public override string ToString() => InternalNode?.ToString() ?? string.Empty;

    public string ToFullString() => InternalNode?.ToFullString() ?? string.Empty;

    public override bool Equals(object? other)
    {
        if (other is SyntaxToken token)
        {
            return Equals(token);
        }

        return false;
    }

    public bool Equals(SyntaxToken other)
    {
        return ReferenceEquals(Parent, other.Parent)
            && ReferenceEquals(InternalNode, other.InternalNode)
            && Position == other.Position;
    }

    public override int GetHashCode() => Hash.Combine(Parent, InternalNode, Position);

    public SyntaxToken WithLeadingTrivia(params SyntaxTrivia[] trivia) => WithLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);

    public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
    {
        if (InternalNode == null)
        {
            return default;
        }

        return new SyntaxToken(
            InternalNode.WithLeadingTrivia(InternalNode.CreateList(trivia.Select(static t => t.RequireRawNode()))),
            null,
            position: 0);
    }

    public SyntaxToken WithLeadingTrivia(SyntaxTriviaList trivia) => WithLeadingTrivia((IEnumerable<SyntaxTrivia>) trivia);

    public SyntaxToken WithTrailingTrivia(params SyntaxTrivia[] trivia) => WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);

    public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
    {
        if (InternalNode == null)
        {
            return default;
        }

        return new SyntaxToken(
            InternalNode.WithTrailingTrivia(InternalNode.CreateList(trivia.Select(static t => t.RequireRawNode()))),
            null,
            position: 0);
    }

    public SyntaxToken WithTrailingTrivia(SyntaxTriviaList trivia) => WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);

    public static bool operator == (SyntaxToken left, SyntaxToken right) => left.Equals(right);

    public static bool operator != (SyntaxToken left, SyntaxToken right) => !left.Equals(right);
}
