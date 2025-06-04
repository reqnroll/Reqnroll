using System.Collections;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public readonly struct SyntaxTokenList : IEquatable<SyntaxTokenList>, IReadOnlyList<SyntaxToken>
{
    internal SyntaxTokenList(InternalNode? node, SyntaxNode? parent, int position)
    {
        Parent = parent;
        InternalNode = node;
        Position = position;
    }

    public SyntaxTokenList(SyntaxToken token)
    {
        Parent = token.Parent;
        InternalNode = token.InternalNode;
        Position = token.Position;
    }

    public SyntaxTokenList(IEnumerable<SyntaxToken> tokens) : this(CreateNode(tokens), null, 0)
    {
    }

    private static InternalNode? CreateNode(IEnumerable<SyntaxToken> tokens)
    {
        var nodes = ImmutableArray.CreateBuilder<InternalNode>();

        foreach (var token in tokens)
        {
            var node = token.InternalNode;
            if (node != null)
            {
                nodes.Add(node);
            }
        }

        return InternalNode.CreateList(nodes);
    }
    
    /// <summary>
    /// Gets the node that is the parent of this token list.
    /// </summary>
    public SyntaxNode? Parent { get; }

    internal InternalNode? InternalNode { get; }

    public static SyntaxTokenList Empty => default;

    public struct Enumerator(SyntaxTokenList list) : IEnumerator<SyntaxToken>
    {
        private int _index = -1;

        public readonly SyntaxToken Current => list[_index];

        readonly object IEnumerator.Current => Current;

        public readonly void Dispose() { }

        public bool MoveNext()
        {
            _index++;

            return _index < list.Count;
        }

        public void Reset()
        {
            _index = -1;
        }
    }

    public SyntaxToken this[int index]
    {
        get
        {
            if (InternalNode == null)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (InternalNode.IsList)
            {
                if (index < InternalNode.SlotCount)
                {
                    return new(InternalNode.GetSlot(index), Parent!, Position + InternalNode.GetSlotOffset(index));
                }
            }
            else if (index == 0)
            {
                return new(InternalNode, Parent, Position);
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    internal int Position { get; }

    public int Count
    {
        get
        {
            if (InternalNode == null)
            {
                return 0;
            }

            if (InternalNode.IsList)
            {
                return InternalNode.SlotCount;
            }

            return 1;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is SyntaxTokenList list)
        {
            return Equals(list);
        }

        return false;
    }

    public bool Equals(SyntaxTokenList other) =>
        ReferenceEquals(Parent, other.Parent)
            && ReferenceEquals(InternalNode, other.InternalNode)
            && Position == other.Position;

    public override int GetHashCode() => InternalNode?.GetHashCode() ?? 0;

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(SyntaxTokenList left, SyntaxTokenList right) => left.Equals(right);

    public static bool operator !=(SyntaxTokenList left, SyntaxTokenList right) => left.Equals(right);
}
