using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a token in a syntax tree, such as a keyword or a block of text.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public readonly struct SyntaxToken : IEquatable<SyntaxToken>
{
    internal SyntaxToken(SyntaxNode? parent, RawNode? syntaxToken, int position)
    {
        Parent = parent;
        RawNode = syntaxToken;
        Position = position;
    }

    internal SyntaxToken(RawNode? token)
    {
        RawNode = token;
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
            if (RawNode == null)
            {
                return default;
            }

            return new(Position, RawNode.FullWidth);
        }
    }

    /// <summary>
    /// Determines whether this node has any leading trivia.
    /// </summary>
    public bool HasLeadingTrivia => LeadingTrivia.Count > 0;

    /// <summary>
    /// Determines whether this node has any leading trivia.
    /// </summary>
    public bool HasTrailingTrivia => TrailingTrivia.Count > 0;

    /// <summary>
    /// Gets the absolute span of this node in characters, not including its leading and trailing trivia.
    /// </summary>
    public TextSpan Span
    {
        get
        {
            if (RawNode == null)
            {
                return default;
            }

            return TextSpan.FromBounds(
                Position + RawNode.GetLeadingTriviaWidth(),
                (Position + RawNode.FullWidth) - RawNode.GetTrailingTriviaWidth());

        }
    }

    private int FullWidth => RawNode?.FullWidth ?? 0;

    /// <summary>
    /// Gets the node that is the parent of this node.
    /// </summary>
    public SyntaxNode? Parent { get; }

    /// <summary>
    /// Gets the text represented by the token.
    /// </summary>
    public string Text => ToString();

    /// <summary>
    /// Gets what kind of language construct is represented by this token.
    /// </summary>
    public SyntaxKind Kind => RawNode?.Kind ?? SyntaxKind.None;

    /// <summary>
    /// Gets whether the syntax token represents a token that was actually parsed from source code. Missing
    /// tokens are generate by the parser in error scenarios to represent constructs that should have been
    /// present in the source code but were actually missing.
    /// </summary>
    public bool IsMissing => RawNode?.IsMissing ?? false;

    internal RawNode? RawNode { get; }

    internal int Position { get; }

    public SyntaxTriviaList LeadingTrivia
    {
        get
        {
            if (RawNode == null)
            {
                return default;
            }

            return new SyntaxTriviaList(this, RawNode.GetLeadingTrivia(), Position);
        }
    }

    public SyntaxTriviaList TrailingTrivia
    {
        get
        {
            if (RawNode == null)
            {
                return default;
            }

            var position = Position + FullWidth;

            var trivia = RawNode.GetTrailingTrivia();

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
    public bool ContainsDiagnostics => RawNode?.ContainsDiagnostics ?? false;

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
        if (RawNode == null)
        {
            return [];
        }

        var tree = SyntaxTree;

        if (tree != null)
        {
            return tree.GetDiagnostics(this);
        }

        if (RawNode.ContainsDiagnostics)
        {
            return RawNode.GetDiagnostics().Select(diag => diag.CreateDiagnosticWithoutLocation());
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
        if (RawNode == null)
        {
            return token.RawNode == null;
        }

        return RawNode.IsEquivalentTo(token.RawNode);
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

    public override string ToString() => RawNode?.ToString() ?? string.Empty;

    public string ToFullString() => RawNode?.ToFullString() ?? string.Empty;

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
            && ReferenceEquals(RawNode, other.RawNode)
            && Position == other.Position;
    }

    public override int GetHashCode() => Hash.Combine(Parent, RawNode, Position);

    public SyntaxToken WithLeadingTrivia(params SyntaxTrivia[] trivia) => WithLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);

    public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
    {
        if (RawNode == null)
        {
            return default;
        }

        return new SyntaxToken(
            null,
            RawNode.WithLeadingTrivia(RawNode.CreateList(trivia.Select(static t => t.RequireRawNode()))),
            position: 0);
    }

    public SyntaxToken WithLeadingTrivia(SyntaxTriviaList trivia) => WithLeadingTrivia((IEnumerable<SyntaxTrivia>) trivia);

    public SyntaxToken WithTrailingTrivia(params SyntaxTrivia[] trivia) => WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);

    public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
    {
        if (RawNode == null)
        {
            return default;
        }

        return new SyntaxToken(
            null,
            RawNode.WithTrailingTrivia(RawNode.CreateList(trivia.Select(static t => t.RequireRawNode()))),
            position: 0);
    }

    public SyntaxToken WithTrailingTrivia(SyntaxTriviaList trivia) => WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);

    public static bool operator == (SyntaxToken left, SyntaxToken right) => left.Equals(right);

    public static bool operator != (SyntaxToken left, SyntaxToken right) => !left.Equals(right);
}
