using Gherkin;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin;

using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

/// <summary>
/// A container of Gherkin syntax.
/// </summary>
/// <remarks>
/// <para>This Roslyn-like representation enables:</para>
/// <list type="bullet">
///     <item>Simple syntactic equivalence comparison.</item>
///     <item>Full mapping of Gherkin syntax positions to generated code because this model tracks whitespace.</item>
///     <item>Future incremental parsing enhancements which rely on a model that can offset existing elements.</item>
/// </list>
/// </remarks>
public class GherkinSyntaxTree
{
    private SourceText? _text;
    private readonly SyntaxNode _root;

    internal GherkinSyntaxTree(SourceText? text, SyntaxNode root, GherkinParseOptions options, string filePath)
    {
        _text = text;
        _root = SyntaxNode.CloneAsRoot(root, this);
        Options = options;
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the options used by the parser to produce the syntax tree.
    /// </summary>
    public GherkinParseOptions Options { get; }

    /// <summary>
    /// Gets the path of the source document file.
    /// </summary>
    /// <value>The path of the source document file, or an empty string if the syntax tree is not associated with a file.</value>
    public string FilePath { get; }

    /// <summary>
    /// Gets a list of all the diagnostics in the syntax tree.
    /// </summary>
    /// <param name="cancellationToken">A token used to signal when the operation should be canceled.</param>
    /// <returns>A list of the <see cref="Diagnostic"/> instances associated with nodes in the tree.</returns>
    public IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default) => 
        GetDiagnostics(GetRoot(cancellationToken));

    public IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token) => GetDiagnostics(token.InternalNode, token.Position);

    public IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia) => GetDiagnostics(trivia.RawNode, trivia.Position);

    public IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node) => GetDiagnostics(node.InternalNode, node.Position);

    private IEnumerable<Diagnostic> GetDiagnostics(InternalNode? node, int position)
    {
        if (node != null && node.ContainsDiagnostics)
        {
            return EnumerateDiagnostics(node, position);
        }

        return [];
    }

    /// <summary>
    /// Enumerates the syntax tree, starting at the specified node and position, and moving through the tree
    /// returning diagnostics in position order.
    /// </summary>
    /// <param name="node">The node to start enumeration at.</param>
    /// <param name="position">The position of the node.</param>
    /// <returns>An enumerable which iterates through the diagnostics of the sytnax tree.</returns>
    private IEnumerable<Diagnostic> EnumerateDiagnostics(InternalNode node, int position)
    {
        var nodeStack = new Stack<InternalNode>();

        void PushNode(InternalNode node)
        {
            // If the node is a token, unpack any leading and trailing trivia to be processed before and after the token.
            if (node.IsToken)
            {
                var trailing = node.GetTrailingTrivia();
                if (trailing != null)
                {
                    nodeStack.Push(trailing);
                }

                nodeStack.Push(node);

                var leadingTrivia = node.GetLeadingTrivia();
                if (leadingTrivia != null)
                {
                    nodeStack.Push(leadingTrivia);
                }
            }
            else
            {
                nodeStack.Push(node);
            }
        }

        PushNode(node);

        while (nodeStack.Count > 0)
        {
            var current = nodeStack.Pop();

            // If the node doesn't contain diagnostics, we can skip right past it.
            if (!current.ContainsDiagnostics)
            {
                position += current.Width;
                continue;
            }

            // Emit any diagnostics attached directly to the node.
            var diagnostics = current.GetAttachedDiagnostics();

            if (!diagnostics.IsDefaultOrEmpty)
            {
                foreach (var diagnostic in diagnostics)
                {
                    yield return diagnostic.CreateDiagnostic(this, new TextSpan(position, current.Width));
                }
            }

            // If the node has children, push them to be processed in document order.
            if (current.SlotCount > 0)
            {
                for (var i = current.SlotCount - 1; i >= 0; i--)
                {
                    var child = current.GetSlot(i);
                    if (child != null)
                    {
                        PushNode(child);
                    }
                }
            }
            else
            {
                // This is a terminating node.
                // Advance the position based on the width of the node.
                position += current.Width;
            }
        }
    }

    public SyntaxNode GetRoot(CancellationToken cancellationToken = default) => _root;

    public SourceText GetSourceText(CancellationToken cancellationToken)
    {
        _text ??= GetRoot(cancellationToken).GetText();

        return _text;
    }

    public static GherkinSyntaxTree ParseText(
        string text,
        GherkinParseOptions? options = null,
        string path = "",
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        return ParseText(
            SourceText.From(text, encoding, SourceHashAlgorithm.Sha1),
            options,
            path,
            cancellationToken);
    }

    public static GherkinSyntaxTree ParseText(
        SourceText text,
        GherkinParseOptions? options = null,
        string path = "",
        CancellationToken cancellationToken = default)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        options ??= GherkinParseOptions.Default;

        var matcher = new TokenMatcher(new DialectProvider(options.Culture.Name));
        var builder = new ParsedSyntaxTreeBuilder(options, text, path, cancellationToken);
        var parser = new SyntaxParser(builder);
        var tokens = new SourceTokenScanner(text);

        return parser.Parse(tokens, matcher);
    }

    public Location GetLocation(TextSpan span) => 
        Location.Create(FilePath, span, GetSourceText(CancellationToken.None).Lines.GetLinePositionSpan(span));

    public SourceText GetText(CancellationToken cancellationToken)
    {
        _text ??= GetRoot(cancellationToken).GetText();

        return _text;
    }

    public override string ToString() => GetText(CancellationToken.None).ToString();
}
