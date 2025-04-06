using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public readonly struct SyntaxTriviaList : IReadOnlyList<SyntaxTrivia>, IEquatable<SyntaxTriviaList>
{
    public SyntaxTriviaList(SyntaxTrivia trivia)
    {
        RawNode = trivia.RawNode;
    }

    public SyntaxTriviaList(IEnumerable<SyntaxTrivia> trivia)
    {
        var builder = new Builder();
        builder.AddRange(trivia);
        RawNode = builder.CreateRawNode();
    }

    internal SyntaxTriviaList(in SyntaxToken token, InternalNode? node, int position)
    {
        _token = token;
        RawNode = node;
        _position = position;
    }

    public struct Enumerator(SyntaxTriviaList list) : IEnumerator<SyntaxTrivia>
    {
        public readonly SyntaxTrivia Current
        {
            get
            {
                if (_index < 0 || _index >= list.Count)
                {
                    throw new InvalidOperationException();
                }

                return list[_index];
            }
        }

        private int _index = -1;

        readonly object IEnumerator.Current => Current;

        public readonly void Dispose()
        {
        }

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

    internal struct Builder()
    {
        private readonly ImmutableArray<InternalNode>.Builder _nodes = ImmutableArray.CreateBuilder<InternalNode>();

        public void AddRange(IEnumerable<SyntaxTrivia> trivia)
        {
            foreach (var item in trivia)
            {
                var node = item.RawNode;
                if (node != null)
                {
                    _nodes.Add(node);
                }
            }
        }

        public readonly InternalNode? CreateRawNode() => InternalNode.CreateList(_nodes);
    }

    private readonly SyntaxToken _token;
    private readonly int _position;

    internal InternalNode? RawNode { get; }

    public static SyntaxTriviaList Empty { get; } = default;

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

    public SyntaxTrivia this[int index]
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
                    return new SyntaxTrivia(_token, RawNode.GetSlot(index), _position + RawNode.GetSlotOffset(index));
                }
            }
            else
            {
                return new SyntaxTrivia(_token, RawNode, _position);
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public readonly Enumerator GetEnumerator() => new(this);

    readonly IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => RawNode?.ToString() ?? "";

    public string ToFullString() => RawNode?.ToFullString() ?? "";

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not SyntaxTriviaList other)
        {
            return false;
        }

        return Equals(other);
    }

    public bool Equals(SyntaxTriviaList other)
    {
        return ReferenceEquals(RawNode, other.RawNode) &&
            _token.Equals(other._token) &&
            _position == other._position;
    }

    public override int GetHashCode() => Hash.Combine(RawNode, _token, _position);
}
