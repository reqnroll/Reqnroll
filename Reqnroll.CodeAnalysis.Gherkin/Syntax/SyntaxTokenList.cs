using System.Collections;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public readonly struct SyntaxTokenList : IEquatable<SyntaxTokenList>, IReadOnlyList<SyntaxToken>
{
    private readonly SyntaxNode? _parent;

    internal SyntaxTokenList(SyntaxNode? parent, InternalNode? node, int position)
    {
        _parent = parent;
        RawNode = node;
        Position = position;
    }

    public SyntaxTokenList(SyntaxToken token)
    {
        _parent = token.Parent;
        RawNode = token.InternalNode;
        Position = token.Position;
    }

    public SyntaxTokenList(IEnumerable<SyntaxToken> tokens) : this(null, CreateNode(tokens), 0)
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

    internal InternalNode? RawNode { get; }

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
            if (RawNode == null)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (RawNode.IsList)
            {
                if (index < RawNode.SlotCount)
                {
                    return new(_parent!, RawNode.GetSlot(index), Position + RawNode.GetSlotOffset(index));
                }
            }
            else if (index == 0)
            {
                return new(_parent, RawNode, Position);
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    internal int Position { get; }

    public int Count
    {
        get
        {
            if (RawNode == null)
            {
                return 0;
            }

            if (RawNode.IsList)
            {
                return RawNode.SlotCount;
            }

            return 1;
        }
    }

    public bool Equals(SyntaxTokenList other)
    {
        throw new NotImplementedException();
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
