using System.Collections;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public readonly struct ChildSyntaxList : IEquatable<ChildSyntaxList>, IReadOnlyList<SyntaxNodeOrToken>
{
    private class EnumeratorProxy(Enumerator enumerator) : IEnumerator<SyntaxNodeOrToken>
    {
        public SyntaxNodeOrToken Current => enumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext() => enumerator.MoveNext();

        public void Reset() => enumerator.Reset();
    }

    public struct Enumerator(SyntaxNode parent)
    {
        private SyntaxNodeOrToken _current;
        private int _slotIndex = -1;
        private int _listIndex = -1;
        private int _position = parent.Position;

        public readonly SyntaxNodeOrToken Current
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
                if (currentNode.RawNode.IsList && MoveNextListSlot(currentNode))
                {
                    return true;
                }                
            }

            // If the current node is not a list, or we have exhausted the list, advance to the next slot.
            for(_slotIndex++; _slotIndex < parent.RawNode.SlotCount; _slotIndex++)
            {
                var slot = parent.RawNode.GetSlot(_slotIndex);

                if (slot == null)
                {
                    continue;
                }

                if (slot.IsToken)
                {
                    _current = new SyntaxToken(parent, slot, _position);
                    _position += slot.FullWidth;
                    return true;
                }

                var node = parent.GetNodeSlot(_slotIndex)!;

                if (slot.IsList)
                {
                    _slotIndex = -1;
                    if (MoveNextListSlot(node))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }

                _current = node;
                _position += slot.FullWidth;
                return true;
            }

            return false;
        }

        private bool MoveNextListSlot(SyntaxNode childList)
        {
            for (_listIndex++; _listIndex < childList.RawNode.SlotCount; _listIndex++)
            {
                var listChild = childList.RawNode.GetSlot(_listIndex);

                if (listChild != null)
                {
                    if (listChild.IsToken)
                    {
                        _current = new SyntaxToken(parent, listChild, _position);
                    }
                    else
                    {
                        _current = childList.GetNodeSlot(_slotIndex)!.GetNodeSlot(_listIndex);
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

    private readonly SyntaxNode _parent;

    public ChildSyntaxList(SyntaxNode parent)
    {
        _parent = parent;
        Count = CountChildren(parent);
    }

    private static int CountChildren(SyntaxNode parent)
    {
        var count = 0;

        for (var index = 0; index < parent.RawNode.SlotCount; index++)
        {
            var node = parent.RawNode.GetSlot(index);
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

    public SyntaxNodeOrToken this[int index]
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

    public SyntaxNodeOrToken First()
    {
        foreach (var item in this)
        {
            return item;
        }

        return default;
    }

    public SyntaxNodeOrToken Last()
    {
        foreach (var item in this.Reverse())
        {
            return item;
        }

        return default;
    }

    public int Count { get; }

    public bool Equals(ChildSyntaxList other) => _parent.Equals(other._parent);

    public Enumerator GetEnumerator() => new(_parent);

    IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator()
    {
        if (_parent.RawNode == null)
        {
            return Enumerable.Empty<SyntaxNodeOrToken>().GetEnumerator();
        }

        return new EnumeratorProxy(new Enumerator(_parent));
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<SyntaxNodeOrToken>)this).GetEnumerator();
}
