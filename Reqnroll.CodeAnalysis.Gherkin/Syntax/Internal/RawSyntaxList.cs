using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal static class RawSyntaxList
{
    public static RawSyntaxList<T> Create<T>(IEnumerable<T> nodes) where T : InternalNode
    {
        var builder = new RawSyntaxList<T>.Builder();

        builder.AddRange(nodes);

        return builder.ToRawSyntaxList();
    }

    public static RawSyntaxList<T> Create<T>(ImmutableArray<T> nodes) where T : InternalNode
    {
        Debug.Assert(!nodes.IsDefault, "Do not pass default array to RawSyntaxList.Create");

        return new RawSyntaxList<T>(nodes);
    }
}

internal class RawSyntaxList<TNode> : InternalNode, IReadOnlyList<TNode> where TNode : InternalNode
{
    public readonly struct Builder() : ICollection<TNode>
    {
        private readonly ImmutableArray<TNode>.Builder _nodes = ImmutableArray.CreateBuilder<TNode>();

        public int Count => _nodes.Count;

        readonly bool ICollection<TNode>.IsReadOnly => false;

        public void Clear() => _nodes.Clear();

        public void CopyTo(TNode[] array, int arrayIndex) => _nodes.CopyTo(array, arrayIndex);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RawSyntaxList<TNode> ToRawSyntaxList() => new(_nodes.ToImmutableArray());

        public void Add(TNode item) => _nodes.Add(item);

        public bool Contains(TNode item) => _nodes.Contains(item);

        public IEnumerator<TNode> GetEnumerator() => _nodes.GetEnumerator();

        public bool Remove(TNode item) => _nodes.Remove(item);

        public void AddRange(IEnumerable<TNode> nodes) => _nodes.AddRange(nodes);
    }

    private readonly ImmutableArray<TNode> _items;

    internal RawSyntaxList(ImmutableArray<TNode> items) : base(SyntaxKind.List)
    {
        _items = items;

        foreach (var child in items)
        {
            IncludeChild(child);
        }
    }

    public TNode this[int index] => _items[index];

    int IReadOnlyCollection<TNode>.Count => _items.Length;

    public override int SlotCount => _items.Length;

    public static RawSyntaxList<TNode> Empty { get; } = new Builder().ToRawSyntaxList();

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<TNode> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public override InternalNode? GetSlot(int index) => this[index];

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        throw new NotImplementedException();
    }

    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
    {
        throw new NotImplementedException();
    }
}
