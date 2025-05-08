using Microsoft.CodeAnalysis.Operations;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public readonly struct SyntaxTriviaList : IReadOnlyList<SyntaxTrivia>, IEquatable<SyntaxTriviaList>
{
    public SyntaxTriviaList(SyntaxTrivia trivia)
    {
        InternalNode = trivia.InternalNode;
    }

    public SyntaxTriviaList(IEnumerable<SyntaxTrivia> trivia)
    {
        var builder = new Builder();
        builder.AddRange(trivia);
        InternalNode = builder.CreateRawNode();
    }

    internal SyntaxTriviaList(in SyntaxToken token, InternalNode? node, int position)
    {
        _token = token;
        InternalNode = node;
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
                var node = item.InternalNode;
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

    internal InternalNode? InternalNode { get; }

    public static SyntaxTriviaList Empty { get; } = default;

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

    public SyntaxTrivia this[int index]
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
                    return new SyntaxTrivia(_token, InternalNode.GetSlot(index), _position + InternalNode.GetSlotOffset(index));
                }
            }
            else
            {
                return new SyntaxTrivia(_token, InternalNode, _position);
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public readonly Enumerator GetEnumerator() => new(this);

    readonly IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => InternalNode?.ToString() ?? "";

    public string ToFullString() => InternalNode?.ToFullString() ?? "";

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
        return ReferenceEquals(InternalNode, other.InternalNode) &&
            _token.Equals(other._token) &&
            _position == other._position;
    }

    public override int GetHashCode() => Hash.Combine(InternalNode, _token, _position);

    public static bool operator==(SyntaxTriviaList left, SyntaxTriviaList right) => left.Equals(right);

    public static bool operator !=(SyntaxTriviaList left, SyntaxTriviaList right) => !left.Equals(right);
}
