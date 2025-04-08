using Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;
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
                if (currentNode.RawNode.IsList && MoveNextSyntaxNodeListSlot(currentNode))
                {
                    return true;
                }                
            }

            // If the current node is not a list, or we have exhausted the list, advance to the next slot.
            for(_slotIndex++; _slotIndex < parent.RawNode.SlotCount; _slotIndex++)
            {
                var slot = parent.RawNode.GetSlot(_slotIndex);

                // If the slot is empty, we advance the enumerator.
                if (slot == null)
                {
                    continue;
                }

                // If the slot is a token, we return the raw token wrapped in a public type.
                if (slot.IsToken)
                {
                    _current = new SyntaxToken(parent, slot, _position);
                    _position += slot.FullWidth;
                    return true;
                }

                var node = parent.GetSlotAsSyntaxNode(_slotIndex);

                // If the slot contains a list, we move to the next item in the list.
                if (slot.IsList)
                {
                    _slotIndex = -1;

                    // If the slot doesn't correspond to a syntax node, but is a list, it must be a syntax token list.
                    // We can return the slot as a syntax token.
                    if (node == null)
                    {
                        if (MoveNextSyntaxTokenListSlot(slot))
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
                _current = node;
                _position += slot.FullWidth;
                return true;
            }

            return false;
        }

        private bool MoveNextSyntaxTokenListSlot(RawNode tokenList)
        {
            for (_listIndex++; _listIndex < tokenList.SlotCount; _listIndex++)
            {
                var listChild = tokenList.GetSlot(_listIndex);

                if (listChild != null)
                {
                    _current = new SyntaxToken(parent, listChild, _position);
                    _position += listChild.FullWidth;
                    return true;
                }
            }

            return false;
        }

        private bool MoveNextSyntaxNodeListSlot(SyntaxNode childList)
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
