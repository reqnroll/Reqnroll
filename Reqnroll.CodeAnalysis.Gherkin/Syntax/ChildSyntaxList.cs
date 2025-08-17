using System.Collections;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public readonly struct ChildSyntaxList(SyntaxNode parent) : 
    IEquatable<ChildSyntaxList>, IReadOnlyList<SyntaxNodeOrToken<SyntaxNode>>
{
    private class EnumeratorProxy(Enumerator enumerator) : IEnumerator<SyntaxNodeOrToken<SyntaxNode>>
    {
        public SyntaxNodeOrToken<SyntaxNode> Current => enumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext() => enumerator.MoveNext();

        public void Reset() => enumerator.Reset();
    }

    public struct Enumerator(SyntaxNode parent)
    {
        private SyntaxNodeOrToken<SyntaxNode> _current;
        private int _slotIndex = -1;
        private int _listIndex = -1;
        private int _position = parent.Position;

        public readonly SyntaxNodeOrToken<SyntaxNode> Current
        {
            get
            {
                if (_current.IsNode || _current.IsToken)
                {
                    return _current;
                }

                throw new InvalidOperationException();
            }
        }

        public bool MoveNext()
        {
            if (_current.IsNode)
            {
                var currentNode = _current.AsNode()!;

                // If we're currently iterating through a list, continue.
                if (currentNode.InternalNode.IsList && MoveNextSyntaxNodeListSlot(currentNode))
                {
                    return true;
                }                
            }

            // If the current node is not a list, or we have exhausted the list, advance to the next slot.
            for(_slotIndex++; _slotIndex < parent.InternalNode.SlotCount; _slotIndex++)
            {
                var slotContent = parent.InternalNode.GetSlot(_slotIndex);

                // If the slot is empty, we advance the enumerator.
                if (slotContent == null)
                {
                    continue;
                }

                // If the slot is a token, we return the raw token wrapped in a public type.
                if (slotContent.IsToken)
                {
                    _current = new SyntaxToken(slotContent, parent, _position);
                    _position += slotContent.FullWidth;
                    return true;
                }

                var node = parent.GetSlotAsSyntaxNode(_slotIndex);

                // If the slot contains a list, we move to the next item in the list.
                if (slotContent.IsList)
                {
                    _slotIndex = -1;

                    // If the slot doesn't correspond to a syntax node, but is a list, it must be a syntax token list.
                    // We can return the slot as a syntax token.
                    if (node == null)
                    {
                        if (MoveNextSyntaxTokenListSlot(slotContent))
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (MoveNextSyntaxNodeListSlot(node))
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                // Otherwise, assume a syntax node.
                _current = (SyntaxNode?)node;
                _position += slotContent.FullWidth;
                return true;
            }

            return false;
        }

        private bool MoveNextSyntaxTokenListSlot(InternalNode tokenList)
        {
            for (_listIndex++; _listIndex < tokenList.SlotCount; _listIndex++)
            {
                var listChild = tokenList.GetSlot(_listIndex);

                if (listChild != null)
                {
                    _current = new SyntaxToken(listChild, parent, _position);
                    _position += listChild.FullWidth;
                    return true;
                }
            }

            return false;
        }

        private bool MoveNextSyntaxNodeListSlot(SyntaxNode childList)
        {
            for (_listIndex++; _listIndex < childList.InternalNode.SlotCount; _listIndex++)
            {
                var listChild = childList.InternalNode.GetSlot(_listIndex);

                if (listChild != null)
                {
                    if (listChild.IsToken)
                    {
                        _current = new SyntaxToken(listChild, parent, _position);
                    }
                    else
                    {
                        _current = childList.GetSlotAsSyntaxNode(_slotIndex)!.GetSlotAsSyntaxNode(_listIndex);
                    }

                    _position += listChild.FullWidth;
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _current = default;
            _slotIndex = -1;
            _listIndex = -1;
            _position = parent.Position;
        }
    }

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

        return new EnumeratorProxy(new Enumerator(_parent));
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<SyntaxNodeOrToken<SyntaxNode>>)this).GetEnumerator();
}
