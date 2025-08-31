using System.Collections;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public readonly partial struct ChildSyntaxList(SyntaxNode parent) : 
    IEquatable<ChildSyntaxList>, IReadOnlyList<SyntaxNodeOrToken<SyntaxNode>>
{
    private readonly SyntaxNode _parent = parent;

    private static int CountChildren(SyntaxNode parent)
    {
        var count = 0;

        for (var index = 0; index < parent.InternalNode.SlotCount; index++)
        {
            var node = parent.InternalNode.GetSlot(index);
            if (node == null)
            {
                continue;
            }

            if (node.IsList)
            {
                count += node.SlotCount;
            }
            else
            {
                count++;
            }
        }

        return count;
    }

    public SyntaxNodeOrToken<SyntaxNode> this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            var count = 0;
            foreach (var item in this)
            {
                if (count == index)
                {
                    return item;
                }

                index++;
            }

            throw new IndexOutOfRangeException();
        }
    }

    public SyntaxNodeOrToken<SyntaxNode> First()
    {
        foreach (var item in this)
        {
            return item;
        }

        return default;
    }

    public SyntaxNodeOrToken<SyntaxNode> Last()
    {
        foreach (var item in this.Reverse())
        {
            return item;
        }

        return default;
    }

    public int Count { get; } = CountChildren(parent);

    public override bool Equals(object? obj)
    {
        if (obj is ChildSyntaxList other)
        {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode() => _parent.GetHashCode();

    public bool Equals(ChildSyntaxList other) => _parent.Equals(other._parent);

    public static bool operator ==(ChildSyntaxList left, ChildSyntaxList right) => left.Equals(right);

    public static bool operator !=(ChildSyntaxList left, ChildSyntaxList right) => !left.Equals(right);

    public Enumerator GetEnumerator() => new(_parent);

    IEnumerator<SyntaxNodeOrToken<SyntaxNode>> IEnumerable<SyntaxNodeOrToken<SyntaxNode>>.GetEnumerator()
    {
        if (_parent.InternalNode == null)
        {
            return Enumerable.Empty<SyntaxNodeOrToken<SyntaxNode>>().GetEnumerator();
        }

        return new Enumerator(_parent);
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<SyntaxNodeOrToken<SyntaxNode>>)this).GetEnumerator();
}
