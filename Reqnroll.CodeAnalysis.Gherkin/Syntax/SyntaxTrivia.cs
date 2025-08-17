using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a segment of trivial syntax in a tree, such as whitespace.
/// </summary>
/// <remarks>
/// <para>The trival portions of a document's syntax are represented using <see cref="SyntaxTrivia"/> values. This allows us to
/// account for all values, including spacing that would otherwise be lost if we only considered the values which affect the
/// meaningful content of the document.</para>
/// <para>This class is loosely based around the public interface of Roslyn's syntax tree: 
/// <see cref="Microsoft.CodeAnalysis.SyntaxTrivia"/>.</para>
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public readonly struct SyntaxTrivia : IEquatable<SyntaxTrivia>
{
    internal SyntaxTrivia(in SyntaxToken token, InternalNode? node, int position)
    {
        Token = token;
        InternalNode = node;
        Position = position;
    }

    /// <summary>
    /// Gets the parent token that this trivia token is associated with.
    /// </summary>
    public SyntaxToken Token { get; }

    /// <summary>
    /// Gets the raw node that this trivia wraps.
    /// </summary>
    internal InternalNode? InternalNode { get; }

    /// <summary>
    /// Gets the position of this triva in the source tree.
    /// </summary>
    internal int Position { get; }

    /// <summary>
    /// Gets the syntax tree that owns this trivia.
    /// </summary>
    public GherkinSyntaxTree? SyntaxTree => Token.SyntaxTree;

    /// <summary>
    /// Gets the absolute span of this trivia in characters.
    /// </summary>
    public TextSpan Span
    {
        get
        {
            var width = Width;

            if (width > 0)
            {
                return new TextSpan(Position, width);
            }

            return default;
        }
    }

    /// <summary>
    /// Gets the absolute span of this trivia in characters. If this trivia has structure, then the span
    /// will include any of the leading or trailing trivia of the structure.
    /// </summary>
    public TextSpan FullSpan
    {
        get
        {
            var width = FullWidth;

            if (width > 0)
            {
                return new TextSpan(Position, width);
            }

            return default;
        }
    }

    internal int Width => InternalNode?.Width ?? 0;

    internal int FullWidth => InternalNode?.FullWidth ?? 0;

    /// <summary>
    /// Gets the kind of the trivia.
    /// </summary>
    public SyntaxKind Kind => InternalNode?.Kind ?? SyntaxKind.None;

    /// <summary>
    /// Gets a value indicating whether this trivia has any diagnostics on it. If the trivia has structure and any of the 
    /// structure has a diagnostic on it, the trivia is considered to have a contain a diagnostic.
    /// </summary>
    /// <value><c>true</c> if the trivia has any diagnostics on it or any structured content; otherwise <c>false</c>.</value>
    public bool ContainsDiagnostics => InternalNode?.ContainsDiagnostics ?? false;

    /// <summary>
    /// Gets a value indicating whether the trivia has a child structure.
    /// </summary>
    /// <value><c>true</c> if the trivia has a child structure; otherwise <c>false</c>.</value>
    /// <remarks>
    /// <para>Most trivia is simple tokens like whitespace or comments. Some trivia can be more complex to represent structures
    /// with more information, such as parts of the syntax tree which the parser skipped, or directives which influence the parsing
    /// process.</para>
    /// <para>In these cases, the structure is wrapped into trivia to clearly separate it from the valid and syntactically 
    /// meaningful tree nodes.</para>
    /// </remarks>
    public bool HasStructure => InternalNode?.IsStructuredTrivia ?? false;

    /// <summary>
    /// Gets the structure of the trivia, if it has structure.
    /// </summary>
    /// <value>If the trivia has structure, the <see cref="StructuredTriviaSyntax"/> representing the structure 
    /// of the trivia; otherwise <c>null</c>.</value>
    public StructuredTriviaSyntax? Structure => HasStructure ?
        InternalNode!.CreateStructuredTriviaSyntaxNode(this) :
        null;

    /// <summary>
    /// Gets the text contained by this trivia.
    /// </summary>
    public string? Text => InternalNode?.ToString();

    private string GetDebuggerDisplay() => 
        GetType().Name + " " + Kind.ToString() + " " + ToString();

    /// <summary>
    /// Gets the string representation of this trivia.
    /// </summary>
    /// <returns>The string representation of this trivia.</returns>
    public override string ToString() => InternalNode?.ToString() ?? string.Empty;

    public string ToFullString() => InternalNode?.ToFullString() ?? string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is SyntaxTrivia other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(SyntaxTrivia other)
    {
        return Token == other.Token &&
            ReferenceEquals(InternalNode, other.InternalNode) &&
            Position == other.Position;
    }


    /// <summary>
    /// Determines whether this trivia is equivalent to a specified trivia.
    /// </summary>
    /// <param name="trivia">The token to compare this token to.</param>
    /// <returns><c>true</c> if this trivia and the specified trivia represent the same trivia; 
    /// otherwise <c>false</c>.</returns>
    public bool IsEquivalentTo(SyntaxTrivia trivia)
    {
        if (InternalNode == null)
        {
            return trivia.InternalNode == null;
        }

        return InternalNode.IsEquivalentTo(trivia.InternalNode);
    }

    public static bool operator ==(SyntaxTrivia left, SyntaxTrivia right) => left.Equals(right);

    public static bool operator !=(SyntaxTrivia left, SyntaxTrivia right) => !left.Equals(right);

    public override int GetHashCode() => Hash.Combine(InternalNode, Token, Position);

    public Location GetLocation()
    {
        if (SyntaxTree == null)
        {
            return Location.None;
        }

        return SyntaxTree.GetLocation(Span);
    }

    internal InternalNode RequireRawNode()
    {
        var node = InternalNode;
        Debug.Assert(node is not null, "RawNode is required in this context.");
        return node!;
    }

    /// <summary>
    /// Gets a list of all the diagnostics associated with this trivia.
    /// </summary>
    /// <returns>A list of the diagnostics for this trivia.</returns>
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
}
