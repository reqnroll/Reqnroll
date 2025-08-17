using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal static class InternalSyntaxList
{
    public static InternalSyntaxList<T> Create<T>(IEnumerable<T> nodes) where T : InternalNode?
    {
        var builder = new InternalSyntaxList<T>.Builder();

        builder.AddRange(nodes);

        return builder.ToSyntaxList();
    }

    public static InternalSyntaxList<T> Create<T>(ImmutableArray<T> nodes) where T : InternalNode?
    {
        Debug.Assert(!nodes.IsDefault, "Do not pass default array to RawSyntaxList.Create");

        return new InternalSyntaxList<T>(nodes);
    }
}

internal class InternalSyntaxList<TNode> : InternalNode, IReadOnlyList<TNode> where TNode : InternalNode?
{
    [DebuggerDisplay("Count={Count}")]
    public readonly struct Builder(int initialCapacity) : IList<TNode>
    {
        private readonly ImmutableArray<TNode>.Builder _nodes = ImmutableArray.CreateBuilder<TNode>(initialCapacity);

        public Builder() : this(8)
        {
        }

        public int Count => _nodes.Count;

        readonly bool ICollection<TNode>.IsReadOnly => false;

        public TNode this[int index]
        {
            get => _nodes[index];
            set => _nodes[index] = value;
        }

        public void Clear() => _nodes.Clear();

        public void CopyTo(TNode[] array, int arrayIndex) => _nodes.CopyTo(array, arrayIndex);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public InternalSyntaxList<TNode> ToSyntaxList() => new(_nodes.ToImmutableArray());

        public void Add(TNode item) => _nodes.Add(item);

        public bool Contains(TNode item) => _nodes.Contains(item);

        public IEnumerator<TNode> GetEnumerator() => _nodes.GetEnumerator();

        public bool Remove(TNode item) => _nodes.Remove(item);

        public void AddRange(IEnumerable<TNode> nodes) => _nodes.AddRange(nodes);

        public void RemoveAt(int index) => _nodes.RemoveAt(index);

        public int IndexOf(TNode item) => _nodes.IndexOf(item);

        public void Insert(int index, TNode item) => _nodes.Insert(index, item);

        public void InsertRange(int index, IEnumerable<TNode> items) => _nodes.InsertRange(index, items);
    }

    public struct Enumerator(InternalSyntaxList<TNode> list) : IEnumerator<TNode>
    {
        private int _index = -1;

        public readonly TNode Current => list[_index];

        readonly object? IEnumerator.Current => Current;

        public readonly void Dispose() { }

        public bool MoveNext()
        {
            _index++;
            return _index < list.SlotCount;
        }

        public void Reset() => _index = -1;
    }

    private readonly ImmutableArray<TNode> _items;

    internal InternalSyntaxList(ImmutableArray<TNode> items) : base(SyntaxKind.List)
    {
        _items = items;

        foreach (var child in items)
        {
            if (child != null)
            {
                IncludeChild(child);
            }
        }
    }

    internal InternalSyntaxList(
        ImmutableArray<TNode> items,
        ImmutableArray<SyntaxAnnotation> annotations,
        ImmutableArray<InternalDiagnostic> diagnostics) : base(SyntaxKind.List, diagnostics, annotations)
    {
        _items = items;

        foreach (var child in items)
        {
            if (child != null)
            {
                IncludeChild(child);
            }
        }
    }

    public TNode this[int index] => _items[index];

    int IReadOnlyCollection<TNode>.Count => _items.Length;

    public override int SlotCount => _items.Length;

    public static InternalSyntaxList<TNode> Empty { get; } = new Builder().ToSyntaxList();

    internal override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        throw new NotImplementedException();
    }

    public Enumerator GetEnumerator() => new(this);

    public override InternalNode? GetSlot(int index) => this[index];

    IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new InternalSyntaxList<TNode>(_items, annotations, GetAttachedDiagnostics());
    }

    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
    {
        return new InternalSyntaxList<TNode>(_items, GetAnnotations(), diagnostics);
    }
}
