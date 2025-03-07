using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

/// <summary>
/// Represents an immutable node that is independant of any parent structure or syntax tree.
/// </summary>
/// <remarks>
/// <para>Having a separate class to represent a raw node independant of location information or hierarchy allows
/// us to cheaply create clones of nodes or assign to multiple parents without having to copy any of the data associated
/// with a node.</para>
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class RawNode
{
    protected RawNode(SyntaxKind kind)
    {
        Kind = kind;
    }

    protected RawNode(SyntaxKind kind, int fullWidth)
    {
        Kind = kind;
        FullWidth = fullWidth;
    }

    protected RawNode(
        SyntaxKind kind,
        int fullWidth,
        ImmutableArray<RawDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations)
    {
        Kind = kind;
        FullWidth = fullWidth;
        _diagnostics = diagnostics;
        _annotations = annotations;

        if (!diagnostics.IsDefaultOrEmpty)
        {
            SetFlag(NodeFlags.ContainsDiagnostics);
        }

        if (!annotations.IsDefaultOrEmpty)
        {
            SetFlag(NodeFlags.ContainsAnnotations);
        }
    }

    protected RawNode(
        SyntaxKind kind,
        ImmutableArray<RawDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations)
    {
        Kind = kind;
        _diagnostics = diagnostics;
        _annotations = annotations;

        if (!diagnostics.IsDefaultOrEmpty)
        {
            SetFlag(NodeFlags.ContainsDiagnostics);
        }

        if (!annotations.IsDefaultOrEmpty)
        {
            SetFlag(NodeFlags.ContainsAnnotations);
        }
    }

    /// <summary>
    /// The flags which apply to this node.
    /// </summary>
    /// <remarks>
    /// <para>Using flags has two benefits:</para>
    /// <list type="bullet">
    ///   <item>We can easily aggregate all the flags on child nodes to produce the flags of the current node.</item>
    ///   <item>We can store several boolean property values in a smaller memory footprint.</item>
    /// </list>
    /// </remarks>
    private NodeFlags _flags;

    private readonly ImmutableArray<RawDiagnostic> _diagnostics;

    private readonly ImmutableArray<SyntaxAnnotation> _annotations;

    private string GetDebuggerDisplay() => GetType().Name + " " + Kind + " " + ToString();

    /// <summary>
    /// Creates a stand-alone <see cref="SyntaxNode"/> which encapsulates this raw node.
    /// </summary>
    /// <returns>A <see cref="SyntaxNode"/> which encapsulates this raw node.</returns>
    public SyntaxNode CreateSyntaxNode() => CreateSyntaxNode(null, 0);

    /// <summary>
    /// Creates a <see cref="SyntaxNode"/> which encapsulates this raw node as part of a larger syntax tree.
    /// </summary>
    /// <param name="parent">The syntax node which is the parent of the new node.</param>
    /// <param name="position">The position of the new node within its tree.</param>
    /// <returns>A <see cref="SyntaxNode"/> which encapsulates this raw node.</returns>
    public abstract SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position);

    public static RawNode? CreateList(IEnumerable<RawNode>? nodes)
    {
        if (nodes == null)
        {
            return null;
        }

        return CreateList(nodes.ToList());
    }

    public static RawNode? CreateList(IReadOnlyList<RawNode> nodes)
    {
        return nodes.Count switch
        {
            0 => null,
            1 => nodes[0],
            _ => RawSyntaxList.Create(nodes),
        };
    }

    public static RawNode? CreateList(ImmutableArray<RawNode>.Builder nodes)
    {
        return nodes.Count switch
        {
            0 => null,
            1 => nodes[0],
            _ => RawSyntaxList.Create(nodes.ToImmutable()),
        };
    }

    public static RawNode? CreateList<T>(ImmutableArray<T> nodes) where T : RawNode
    {
        return nodes.Length switch
        {
            0 => null,
            1 => nodes[0],
            _ => RawSyntaxList.Create(nodes),
        };
    }

    /// <summary>
    /// Creates a list from two nodes.
    /// </summary>
    /// <param name="node1">The first node in the list.</param>
    /// <param name="node2">The second node in the list.</param>
    /// <returns>A list containing both nodes.</returns>
    public static RawNode CreateList(RawNode node1, RawNode node2)
    {
        var builder = ImmutableArray.CreateBuilder<RawNode>(2);

        builder.Add(node1);
        builder.Add(node2);

        return RawSyntaxList.Create(builder.ToImmutableArray());
    }

    /// <summary>
    /// Creates a new list by appending a node to the end of the list.
    /// </summary>
    /// <param name="list">The list to extend.</param>
    /// <param name="node">The node to append.</param>
    /// <returns>A new list containing the elements of the list specified by <paramref name="list"/> with 
    /// the node specified by <paramref name="node"/> appended as the last element.</returns>
    private static RawNode AppendList(RawNode list, RawNode node)
    {
        Debug.Assert(list.IsList, "The list is not a list.");

        var builder = ImmutableArray.CreateBuilder<RawNode>(list.SlotCount + 1);

        for (var i = 0; i < list.SlotCount; i++)
        {
            builder.Add(list.GetRequiredSlot(i));
        }

        builder.Add(node);

        return RawSyntaxList.Create(builder.ToImmutableArray());
    }

    private static RawNode ConcatLists(RawNode list1, RawNode list2)
    {
        Debug.Assert(list1.IsList, "List 1 is not a list.");
        Debug.Assert(list2.IsList, "List 2 is not a list.");

        var builder = ImmutableArray.CreateBuilder<RawNode>(list1.SlotCount + list2.SlotCount);

        for (var i = 0; i < list1.SlotCount; i++)
        {
            builder.Add(list1.GetRequiredSlot(i));
        }

        for (var i = 0; i < list2.SlotCount; i++)
        {
            builder.Add(list2.GetRequiredSlot(i));
        }

        return RawSyntaxList.Create(builder.ToImmutableArray());
    }

    private static RawNode PrefixList(RawNode node, RawNode list)
    {
        Debug.Assert(list.IsList, "The list is not a list.");

        var builder = ImmutableArray.CreateBuilder<RawNode>(list.SlotCount + 1);

        builder.Add(node);

        for (var i = 0; i < list.SlotCount; i++)
        {
            builder.Add(list.GetRequiredSlot(i));
        }

        return RawSyntaxList.Create(builder.ToImmutableArray());
    }

    public abstract int SlotCount { get; }

    public bool IsTerminal => SlotCount == 0;

    internal void SetFlag(NodeFlags flag)
    {
        _flags |= flag;
    }

    internal void ClearFlag(NodeFlags flag)
    {
        _flags &= ~flag;
    }

    public virtual int GetSlotOffset(int index)
    {
        var offset = 0;

        for (var i = 0; i < index; i++)
        {
            var slot = GetSlot(i);

            if (slot != null)
            {
                offset += slot.FullWidth;
            }
        }

        return offset;
    }

    public abstract RawNode? GetSlot(int index);

    private RawNode GetRequiredSlot(int index)
    {
        var node = GetSlot(index);
        Debug.Assert(node is not null, "");
        return node!;
    }

    public virtual RawNode WithLeadingTrivia(RawNode? trivia)
    {
        return this;
    }

    public virtual RawNode WithTrailingTrivia(RawNode? trivia)
    {
        return this;
    }

    public virtual int Width => FullWidth - GetLeadingTriviaWidth() - GetTrailingTriviaWidth();

    /// <summary>
    /// Gets the full width of this node including all leading and trailing trivia.
    /// </summary>
    /// <remarks>
    /// <para>Ideally this would be a readonly property, but it's syntatically awkward to enforce this within the class
    /// hierarchy without performing additional allocations to create child collections. The expectation is that this
    /// field is only updated from within a constructor.</para>
    /// </remarks>
    public int FullWidth { get; private set; }

    public virtual bool IsToken => false;

    public virtual bool IsTrivia => false;

    public bool IsMissing => (_flags & NodeFlags.IsNotMissing) == NodeFlags.None;

    public SyntaxKind Kind { get; }

    /// <summary>
    /// Determines whether this node has any leading trivia.
    /// </summary>
    public virtual bool HasLeadingTrivia
    {
        get
        {
            var first = GetFirstTerminalDescendant();

            return first?.HasLeadingTrivia ?? false;
        }
    }

    /// <summary>
    /// Determines whether this node has any leading trivia.
    /// </summary>
    public virtual bool HasTrailingTrivia
    {
        get
        {
            var last = GetLastTerminalDescendant();

            return last?.HasTrailingTrivia ?? false;
        }
    }

    public bool ContainsDiagnostics => (_flags & NodeFlags.ContainsDiagnostics) == NodeFlags.ContainsDiagnostics;

    public bool ContainsAnnotations => (_flags & NodeFlags.ContainsAnnotations) == NodeFlags.ContainsAnnotations;

    public bool IsList => Kind == SyntaxKind.List;

    public virtual bool IsStructuredTrivia => false;

    private bool IsSingleItemList => IsList && SlotCount == 1;

    public virtual bool IsEquivalentTo(RawNode? other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Kind != other.Kind)
        {
            // We consider a single-element list and its single node to be equivalent representations of the same structure.
            if (!IsSingleItemList && !other.IsSingleItemList)
            {
                return false; 
            }

            var subject = this;
            if (subject.IsSingleItemList)
            {
                subject = subject.GetRequiredSlot(0);
            }

            if (other.IsSingleItemList)
            {
                other = other.GetRequiredSlot(0);
            }

            return subject.IsEquivalentTo(other);
        }

        if (FullWidth != other.FullWidth)
        {
            return false;
        }

        if (SlotCount != other.SlotCount)
        {
            return false;
        }

        for (var i = 0; i < SlotCount; i++)
        {
            var child = GetSlot(i);
            var otherChild = other.GetSlot(i);

            if (child == null)
            {
                if (otherChild != null)
                {
                    return false;
                }
            }
            else
            {
                if (!child.IsEquivalentTo(otherChild))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public virtual RawNode? GetLeadingTrivia()
    {
        // Zero-width nodes cannot have trivia.
        if (FullWidth == 0)
        {
            return null;
        }

        var first = GetFirstTerminalDescendant();

        Debug.Assert(first != null, $"No terminal descendant found on non-zero width node {GetDebuggerDisplay()}");

        return first!.GetLeadingTrivia();
    }

    public virtual int GetLeadingTriviaWidth()
    {
        // Zero-width nodes cannot have trivia.
        if (FullWidth == 0)
        {
            return 0;
        }

        var first = GetFirstTerminalDescendant();

        Debug.Assert(first != null, $"No terminal descendant found on non-zero width node {GetDebuggerDisplay()}");

        return first!.GetLeadingTriviaWidth();
    }

    public virtual RawNode? GetTrailingTrivia()
    {
        // Zero-width nodes cannot have trivia.
        if (FullWidth == 0)
        {
            return null;
        }

        var last = GetLastTerminalDescendant();

        Debug.Assert(last != null, $"No terminal descendant found on non-zero width node {GetDebuggerDisplay()}");

        return last!.GetTrailingTrivia();
    }

    public virtual int GetTrailingTriviaWidth()
    {
        // Zero-width nodes cannot have trivia.
        if (FullWidth == 0)
        {
            return 0;
        }

        var last = GetLastTerminalDescendant();

        Debug.Assert(last != null, $"No terminal descendant found on non-zero width node {GetDebuggerDisplay()}");

        return last!.GetTrailingTriviaWidth();
    }

    private RawNode? GetFirstTerminalDescendant()
    {
        RawNode? node = this;

        // Walk the node tree until we hit a terminal node (one that cannot have children).
        do
        {
            RawNode? descendant = null;
            var count = node.SlotCount;
            for (var i = 0; i < count; i++)
            {
                var child = node.GetSlot(i);

                if (child != null)
                {
                    descendant = child;
                    break;
                }
            }

            node = descendant;
        }
        while (node?.SlotCount > 0);

        return node;
    }

    private RawNode? GetLastTerminalDescendant()
    {
        if (SlotCount == 0)
        {
            return null;
        }

        RawNode? node = this;

        // Walk the node tree until we hit a terminal node (one that cannot have children).
        do
        {
            var count = node.SlotCount;
            for (var i = count - 1; i > -1; i--)
            {
                var child = node.GetSlot(i);

                if (child != null)
                {
                    node = child;
                    break;
                }
            }
        }
        while (node?.SlotCount > 0);

        return node;
    }

    public RawNode WithAdditionalAnnotations(SyntaxAnnotation syntaxAnnotation)
    {
        if (_annotations.IsDefaultOrEmpty)
        {
            return WithAnnotations(syntaxAnnotation);
        }

        return WithAnnotations(_annotations.Add(syntaxAnnotation));
    }

    public RawNode WithAdditionalAnnotations(IEnumerable<SyntaxAnnotation> syntaxAnnotations)
    {
        if (_annotations.IsDefaultOrEmpty)
        {
            return WithAnnotations(syntaxAnnotations.ToImmutableArray());
        }

        return WithAnnotations(_annotations.AddRange(syntaxAnnotations));
    }

    public RawNode WithAnnotations(SyntaxAnnotation annotation)
    {
        var builder = ImmutableArray.CreateBuilder<SyntaxAnnotation>();
        builder.Add(annotation);
        var annotations = builder.ToImmutableArray();

        return WithAnnotations(annotations);
    }

    public abstract RawNode WithDiagnostics(ImmutableArray<RawDiagnostic> diagnostics);

    public abstract RawNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations);

    /// <summary>
    /// Gets the diagnostics directly associated with this node. This differs from <see cref="SyntaxNode.GetDiagnostics()"/> which
    /// returns all diagnostics associated with the node directly <b>and</b> those associated with its descendant nodes.
    /// </summary>
    /// <returns>The collection of diagnostics attached to this node, or a default array.</returns>
    public ImmutableArray<RawDiagnostic> GetAttachedDiagnostics() => _diagnostics;

    public ImmutableArray<SyntaxAnnotation> GetAnnotations() => _annotations;

    /// <summary>
    /// Includes a node as a child of this node. Syntax is expected to call this method for each syntax token or child syntax
    /// that are children of the current node. As nodes are immutable, is not valid to call this method outside of the constructor.
    /// </summary>
    /// <param name="node">The child node to incorporate into this node.</param>
    protected void IncludeChild(RawNode node)
    {
        _flags |= node._flags & (NodeFlags.ContainsAnnotations | NodeFlags.ContainsDiagnostics | NodeFlags.IsNotMissing);
        FullWidth += node.FullWidth;
    }

    public override string ToString()
    {
        var writer = new StringWriter(CultureInfo.InvariantCulture);

        WriteTo(writer, false, false);

        return writer.ToString();
    }

    public virtual string ToFullString()
    {
        var writer = new StringWriter(CultureInfo.InvariantCulture);

        WriteTo(writer, true, true);

        return writer.ToString();
    }

    public void WriteTo(TextWriter writer) => WriteTo(writer, true, true);

    internal virtual void WriteTo(TextWriter writer, bool leading, bool trailing)
    {
        var nodesToProcess = new Stack<(RawNode node, bool leading, bool trailing)>();

        nodesToProcess.Push((this, leading, trailing));

        while (nodesToProcess.Count > 0)
        {
            var (node, includeLeadingTrivia, includeTrailingTrivia) = nodesToProcess.Pop();

            if (node.IsTrivia || node.IsToken)
            {
                node.WriteTo(writer, includeLeadingTrivia, includeTrailingTrivia);
                continue;
            }

            var firstIndex = node.GetFirstNonNullChildIndex();

            if (firstIndex == -1)
            {
                continue;
            }

            var lastIndex = node.GetLastNonNullChildIndex();

            // Push child nodes onto the stack in reverse order so they get processed in the expected sequence.
            for (var i = lastIndex; i >= firstIndex; i--)
            {
                var child = node.GetSlot(i);

                if (child == null)
                {
                    continue;
                }

                var isFirst = i == firstIndex;
                var isLast = i == lastIndex;

                nodesToProcess.Push((child, includeLeadingTrivia || !isFirst, includeTrailingTrivia || !isLast));
            }
        }
    }

    private int GetFirstNonNullChildIndex()
    {
        for (var index = 0; index < SlotCount; index++)
        {
            var child = GetSlot(index);

            if (child != null)
            {
                return index;
            }
        }

        return -1;
    }

    private int GetLastNonNullChildIndex()
    {
        for (var index = SlotCount - 1; index >= 0; index--)
        {
            var child = GetSlot(index);

            if (child != null)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// Combines two nodes into a single node.
    /// </summary>
    /// <param name="node1">The first node to combine.</param>
    /// <param name="node2">The second node to combine.</param>
    /// <returns>A <see cref="RawNode"/> which is the combined result of the node pair.</returns>
    internal static RawNode? Concat(RawNode? node1, RawNode? node2)
    {
        if (node1 == null)
        {
            return node2;
        }

        if (node2 == null)
        {
            return node1;
        }

        if (node1.IsList)
        {
            if (node2.IsList)
            {
                return ConcatLists(node1, node2);
            }
            else
            {
                return AppendList(node1, node2);
            }
        }

        if (node2.IsList)
        {
            return PrefixList(node1, node2);
        }

        return CreateList(node1, node2);
    }

    /// <summary>
    /// Creates a <see cref="Syntax.StructuredTriviaSyntax"/> which encapsulates this raw node as structured trivia as part of a syntax tree.
    /// </summary>
    /// <param name="parent">The syntax trivia which encapsulates this structured syntax.</param>
    /// <returns>A <see cref="Syntax.StructuredTriviaSyntax"/> which encapsulates this raw node, or <c>null</c> 
    /// if the node is not structure trivia.</returns>
    public virtual Syntax.StructuredTriviaSyntax? CreateStructuredTriviaSyntaxNode(SyntaxTrivia parent)
    {
        return null;
    }

    public static RawNode? operator +(RawNode? node1, RawNode? node2) => Concat(node1, node2);
}
