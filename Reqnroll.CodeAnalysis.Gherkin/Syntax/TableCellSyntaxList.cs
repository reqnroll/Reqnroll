using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Immutable;

#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static InternalSyntaxFactory;

/// <summary>
/// Represents a specialized list of syntax nodes that are separated by vertical bar tokens.
/// </summary>
/// <typeparam name="TNode">The type of syntax node contained in this collection.</typeparam> 
#if NET8_0_OR_GREATER
[CollectionBuilder(typeof(TableCellSyntaxList), methodName: nameof(Create))]
#endif
public readonly struct TableCellSyntaxList : IEquatable<TableCellSyntaxList>, 
    IReadOnlyList<SyntaxNodeOrToken<TableCellSyntax>>
{
    public struct Enumerator(TableCellSyntaxList list) : IEnumerator<SyntaxNodeOrToken<TableCellSyntax>>
    {
        private int _index = -1;

        public readonly SyntaxNodeOrToken<TableCellSyntax> Current => list[_index];

        readonly object? IEnumerator.Current => Current;

        public readonly void Dispose() { }

        public bool MoveNext() => ++_index < list.Count;

        public void Reset() => _index = -1;
    }

    internal TableCellSyntaxList(InternalNode? internalNode, SyntaxNode? parent, int position)
    {
        _list = new SyntaxNodeOrTokenList<TableCellSyntax>(internalNode, parent, position);
    }

    internal TableCellSyntaxList(InternalNode? node)
    {
        _list = new SyntaxNodeOrTokenList<TableCellSyntax>(node);
    }

    internal TableCellSyntaxList(SyntaxNodeOrTokenList<TableCellSyntax> list)
    {
        _list = list;
    }

    internal TableCellSyntaxList(IEnumerable<TableCellSyntax> nodes)
    {
        // Interpolate the list of nodes with separator tokens.
        var list = new InternalSyntaxList<InternalNode?>.Builder();

        using var enumerator = nodes.GetEnumerator();

        if (enumerator.MoveNext())
        {
            // If this is the first node, we don't need a separator before it.
            list.Add(enumerator.Current?.InternalNode);
        }

        while (enumerator.MoveNext())
        {
            list.Add(Token(SyntaxKind.VerticalBarToken));
            list.Add(enumerator.Current?.InternalNode);
        }

        _list = new SyntaxNodeOrTokenList<TableCellSyntax>(list.ToSyntaxList());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableCellSyntaxList"/> structure from the specified sequence
    /// of nodes and tokens.
    /// </summary>
    /// <param name="nodes">A sequence of nodes and tokens, starting and ending with a syntax node.</param>
    public TableCellSyntaxList(IEnumerable<SyntaxNodeOrToken<TableCellSyntax>> nodes)
    {
        var list = new InternalSyntaxList<InternalNode?>.Builder();

        var index = 0;
        foreach (var nodeOrToken in nodes)
        {
            switch (index % 2)
            {
                case 0:
                    // Even nodes should be syntax nodes, but can also be null.
                    if (nodeOrToken.Kind == SyntaxKind.VerticalBarToken)
                    {
                        throw new ArgumentException(
                            SyntaxExceptionMessages.SequenceMustBeSyntaxNodesSeparatedBySyntaxTokens,
                            nameof(nodes));
                    }
                    break;

                case 1:
                    // Odd nodes should be separator tokens.
                    if (nodeOrToken.Kind != SyntaxKind.VerticalBarToken)
                    {
                        throw new ArgumentException(
                            string.Format(
                                SyntaxExceptionMessages.SequenceMustBeSeparatedByTokensOfKind,
                                SyntaxKind.VerticalBarToken,
                                nodeOrToken.Kind),
                            nameof(nodes));
                    }
                    break;
            }

            list.Add(nodeOrToken.InternalNode);

            index++;
        }

        _list = new SyntaxNodeOrTokenList<TableCellSyntax>(list.ToSyntaxList());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableCellSyntaxList"/> structure with a single node.
    /// </summary>
    /// <param name="node">The node to create the list from.</param>
    public TableCellSyntaxList(TableCellSyntax node): this(node.InternalNode)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableCellSyntaxList"/> instance from the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to create a list from.</param>
    /// <returns>A <see cref="TableCellSyntaxList}"/> populated with the syntax nodes and tokens specified 
    /// in <paramref name="nodes"/>.</returns>
    public static TableCellSyntaxList Create(ReadOnlySpan<SyntaxNodeOrToken<PlainTextSyntax>> nodes)
    {
        if (nodes.Length == 0)
        {
            return default;
        }

        if (nodes.Length == 1)
        {
            var node = nodes[0].AsNode();

            return new TableCellSyntaxList(node?.InternalNode);
        }

        var internalNodes = ImmutableArray.CreateBuilder<InternalNode?>(nodes.Length);

        foreach (var nodeOrToken in nodes)
        {
            internalNodes.Add(nodeOrToken.InternalNode);
        }

        var list = InternalSyntaxList.Create(internalNodes.ToImmutable());
        var delimiterKind = SyntaxKind.VerticalBarToken;

        // A valid list is syntax nodes separated by tokens.
        for (var i = 0; i < list.SlotCount; i++)
        {
            var node = list.GetSlot(i);

            if (i % 2 != 0 && (node == null || node.Kind != delimiterKind))
            {
                throw new ArgumentException(
                    string.Format(
                        SyntaxExceptionMessages.SequenceMustBeSeparatedByTokensOfKind,
                        delimiterKind,
                        node?.Kind ?? null),
                    nameof(nodes));
            }
        }

        // If the last node is a token, the list is invalid.
        if (list.SlotCount % 2 != 1)
        {
            throw new ArgumentException(
                SyntaxExceptionMessages.SequenceMustBeSyntaxNodesSeparatedBySyntaxTokens,
                nameof(nodes));
        }

        return new TableCellSyntaxList(list);
    }

    private readonly SyntaxNodeOrTokenList<TableCellSyntax> _list;

    internal InternalNode? InternalNode => _list.InternalNode;

    public int Position => _list.Position;

    public int Count => _list.Count;

    public SyntaxNodeOrToken<TableCellSyntax> this[int index] => _list[index];

    public static bool operator ==(TableCellSyntaxList left, TableCellSyntaxList right) =>
        left.Equals(right);

    public static bool operator !=(TableCellSyntaxList left, TableCellSyntaxList right) =>
        !left.Equals(right);

    public override bool Equals(object? obj)
    {
        if (obj is TableCellSyntaxList other)
        {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode() => _list.GetHashCode();

    public bool Equals(TableCellSyntaxList other) => _list.Equals(other._list);

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<SyntaxNodeOrToken<TableCellSyntax>> IEnumerable<SyntaxNodeOrToken<TableCellSyntax>>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the list of nodes which are the values of this list, excluding the separator tokens.
    /// </summary>
    public CellList Cells => new(_list);

    /// <summary>
    /// Gets the contents of this list, including the separator tokens present between the text nodes,
    /// as a <see cref="SyntaxNodeOrTokenList{T}"/> instance.
    /// </summary>
    /// <returns>The contents of this list, including the separator tokens.</returns>
    public SyntaxNodeOrTokenList<TableCellSyntax> ToSyntaxNodeOrTokenList() => _list;

    /// <summary>
    /// Gets a string representation of this list's content.
    /// </summary>
    /// <returns>A string of the syntax nodes and separator characters represented by this list.</returns>
    public override string? ToString() => _list.ToString();

    /// <summary>
    /// Gets a string representation of this list, including any leading or trailing trivia.
    /// </summary>
    /// <returns>A string containing the leading trivia, content and trailing trivia of this list.</returns>
    public string? ToFullString() => _list.ToFullString();

    public readonly struct CellList(SyntaxNodeOrTokenList<TableCellSyntax> list) : IReadOnlyList<TableCellSyntax>
    {
        public struct Enumerator(CellList list) : IEnumerator<TableCellSyntax>
        {
            private int _index = -1;

            public readonly TableCellSyntax Current => list[_index];

            readonly object? IEnumerator.Current => Current;

            public readonly void Dispose() { }

            public bool MoveNext() => ++_index < list.Count;

            public void Reset() => _index = -1;
        }

        public int Count => (list.Count + 1) / 2;

        public TableCellSyntax this[int index]
        {
            get
            {
                if (list.InternalNode == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                // We want to map indexes to only return the syntax nodes, not the separators.
                // 0, 1, 2, 3 maps to
                // 0, 2, 4, 6 in the internal node list.
                var targetIndex = index * 2;

                return list[targetIndex].AsNode()!;
            }
        }

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<TableCellSyntax> IEnumerable<TableCellSyntax>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TableCellSyntaxList Add(TableCellSyntax node) => Insert(Count, node);

        public TableCellSyntaxList Insert(int index, TableCellSyntax node) => InsertRange(index, new[] { node });

        public TableCellSyntaxList InsertRange(int index, IEnumerable<TableCellSyntax> nodes)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (Count == 0)
            {
                // If the list is empty, we can directly create a new list with the nodes.
                return new TableCellSyntaxList(nodes);
            }

            var newList = new InternalSyntaxList<InternalNode?>.Builder();

            // If the list is not empty, copy all the current items and separators to a new list.
            foreach (var item in list)
            {
                newList.Add(item.InternalNode);
            }

            // Determine the position where the new nodes should be inserted:
            // If the position is within the current list, insert before the existing node at that index;
            // otherwise append to the end of the list.
            var insertionPoint = index < Count ? index : Count;

            // If the insertion point is not first or last position, we need to include a separator.
            if (insertionPoint > 0 && insertionPoint < newList.Count)
            {
                var previousNode = newList[insertionPoint - 1];
                if (previousNode != null && previousNode.IsToken)
                {
                    // The previous node is a separator.
                    // Move the insertion point before the separator.
                    insertionPoint--;
                }
            }

            // Create a list of internal nodes to insert by interpolating the nodes with separators.
            var inserts = new List<InternalNode?>();

            using var enumerator = nodes.GetEnumerator();

            if (enumerator.MoveNext())
            {
                // If this is the first node, we don't need a separator before it.
                if (insertionPoint > 0)
                {
                    inserts.Add(Token(SyntaxKind.VerticalBarToken));
                }

                inserts.Add(enumerator.Current?.InternalNode);
            }

            while (enumerator.MoveNext())
            {
                inserts.Add(Token(SyntaxKind.VerticalBarToken));
                inserts.Add(enumerator.Current?.InternalNode);
            }

            // If we're inserting at the start of a list, we add a separator after the last node.
            inserts.Add(Token(SyntaxKind.VerticalBarToken));

            // Insert the nodes and separators at the insertion point.
            newList.InsertRange(insertionPoint, inserts);

            return new TableCellSyntaxList(
                new SyntaxNodeOrTokenList<TableCellSyntax>(newList.ToSyntaxList()));
        }
    }
}

