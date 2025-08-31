using System.Collections;
using System.Reflection;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public readonly partial struct ChildSyntaxList
{
    public struct Enumerator(SyntaxNode parent) : IEnumerator<SyntaxNodeOrToken<SyntaxNode>>
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

        readonly object IEnumerator.Current => Current;

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
            for (_slotIndex++; _slotIndex < parent.InternalNode.SlotCount; _slotIndex++)
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

                // If the slot doesn't correspond to a syntax token, or a list, and doesn't convert to a node directly,
                // it must be a single syntax node in a slot that can also contain a list.
                if (node == null)
                {
                    _current = slotContent.CreateSyntaxNode(parent, _position);
                }
                else
                {
                    // Otherwise, assume a regular syntax node.
                    _current = node;
                }

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

        readonly void IDisposable.Dispose()
        {
        }
    }
}
