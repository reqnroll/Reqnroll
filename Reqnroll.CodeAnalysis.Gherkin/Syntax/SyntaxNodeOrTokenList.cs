using System.Collections;

#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public static class SyntaxNodeOrTokenList
{
    public static SyntaxNodeOrTokenList<TNode> Create<TNode>(ReadOnlySpan<SyntaxNodeOrToken<TNode>> nodes) 
        where TNode : SyntaxNode
    {
        var builder = new InternalSyntaxList<InternalNode>.Builder(nodes.Length);

        foreach (var node in nodes)
        {
            builder.Add(node.InternalNode!);
        }

        return new SyntaxNodeOrTokenList<TNode>(builder.ToSyntaxList());
    }
}

#if NET8_0_OR_GREATER
[CollectionBuilder(typeof(SyntaxNodeOrTokenList), methodName: nameof(SyntaxNodeOrTokenList.Create))]
#endif
public readonly struct SyntaxNodeOrTokenList<TNode> : 
    IReadOnlyList<SyntaxNodeOrToken<TNode>>, 
    IEquatable<SyntaxNodeOrTokenList<TNode>>
    where TNode : SyntaxNode
{
    public struct Enumerator(SyntaxNodeOrTokenList<TNode> list) : IEnumerator<SyntaxNodeOrToken<TNode>>
    {
        private readonly SyntaxNodeOrTokenList<TNode> _list = list;

        private int _index = -1;

        public readonly SyntaxNodeOrToken<TNode> Current => _list[_index];

        readonly object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            return _index < _list.Count;
        }

        public void Reset() => _index = -1;

        public readonly void Dispose() { }
    }

    private readonly SyntaxNode? _parent;

    internal SyntaxNodeOrTokenList(InternalNode? internalNode)
        : this(internalNode, null, 0)
    {
    }

    internal SyntaxNodeOrTokenList(InternalNode? internalNode, SyntaxNode? parent, int position)
    {
        InternalNode = internalNode;
        _parent = parent;
        Position = position;
    }

    public int Count
    {
        get
        {
            if (InternalNode == null)
            {
                return 0;
            }

            return InternalNode.IsList ? InternalNode.SlotCount : 1;
        }
    }

    public int Position { get; }

    internal InternalNode? InternalNode { get; }

    public SyntaxNodeOrToken<TNode> this[int index]
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
                    var slot = InternalNode.GetSlot(index);

                    if (slot == null)
                    {
                        return null;
                    }

                    if (slot.IsToken)
                    {
                        return new SyntaxToken(slot, _parent, Position + InternalNode.GetSlotOffset(index));
                    }
                    else
                    {
                        return (TNode)slot.CreateSyntaxNode(_parent, Position + InternalNode.GetSlotOffset(index));
                    }
                }
            }
            else if (index == 0)
            {
                return (TNode)InternalNode.CreateSyntaxNode(_parent, Position + InternalNode.GetSlotOffset(index));
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    /// <summary>
    /// Gets a string representation of this list's content.
    /// </summary>
    /// <returns>A string of the syntax nodes and separator characters represented by this list.</returns>
    public override string? ToString() => InternalNode?.ToString();

    /// <summary>
    /// Gets a string representation of this list, including any leading or trailing trivia.
    /// </summary>
    /// <returns>A string containing the leading trivia, content and trailing trivia of this list.</returns>
    public string? ToFullString() => InternalNode?.ToFullString();

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<SyntaxNodeOrToken<TNode>> IEnumerable<SyntaxNodeOrToken<TNode>>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(SyntaxNodeOrTokenList<TNode> other) => 
        Equals(InternalNode, other.InternalNode) &&
            Position == other.Position &&
            _parent == other._parent;

    public override bool Equals(object? obj) => obj is SyntaxNodeOrTokenList<TNode> other && Equals(other);

    public override int GetHashCode() => InternalNode?.GetHashCode() ?? 0;

    public static bool operator ==(SyntaxNodeOrTokenList<TNode> left, SyntaxNodeOrTokenList<TNode> right) => left.Equals(right);

    public static bool operator !=(SyntaxNodeOrTokenList<TNode> left, SyntaxNodeOrTokenList<TNode> right) => !left.Equals(right);
}
