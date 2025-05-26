﻿using Microsoft.CodeAnalysis.Text;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Provides methods for creating <see cref="SyntaxList{TNode}"/> instances.
/// </summary>
public static class SyntaxList
{
    /// <summary>
    /// Creates a new <see cref="SyntaxList{TNode}"/> instance from the specified nodes.
    /// </summary>
    /// <typeparam name="TNode">The type of nodes contained by the list.</typeparam>
    /// <param name="nodes">The nodes to create a list from.</param>
    /// <returns>A <see cref="SyntaxList"/> populated with the syntax nodes specified in <paramref name="nodes"/>.</returns>
    public static SyntaxList<TNode> Create<TNode>(ReadOnlySpan<TNode> nodes)
        where TNode : SyntaxNode
    {
        throw new NotImplementedException();
    }
}

#if NET8_0_OR_GREATER
[CollectionBuilder(typeof(SyntaxList), methodName: nameof(SyntaxList.Create))]
#endif
public readonly struct SyntaxList<TNode> : IEquatable<SyntaxList<TNode>>, IReadOnlyList<TNode>
    where TNode : SyntaxNode
{
    private readonly SyntaxNode? _parent;

    internal SyntaxList(InternalNode? node, SyntaxNode parent, int position)
    {
        InternalNode = node;
        _parent = parent;
        Position = position;
    }

    public SyntaxList(TNode node)
    {
        InternalNode = node.InternalNode;
    }

    public SyntaxList(IEnumerable<TNode> nodes)
    {
        InternalNode = InternalNode.CreateList(nodes.Select(node => node.InternalNode).ToImmutableArray());
    }

    public int Position { get; }

    internal InternalNode? InternalNode { get; }

    public TNode this[int index]
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
                    return (TNode)InternalNode.GetSlot(index)!.CreateSyntaxNode(_parent, Position + InternalNode.GetSlotOffset(index));
                }
            }
            else if (index == 0)
            {
                return (TNode)InternalNode.CreateSyntaxNode(_parent, Position + InternalNode.GetSlotOffset(index));
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
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

    /// <summary>
    /// The absolute span of the list elements in characters, including the leading and trailing trivia of the 
    /// first and last elements.
    /// </summary>
    public TextSpan FullSpan
    {
        get
        {
            if (Count == 0)
            {
                return default;
            }

            return TextSpan.FromBounds(this[0].FullSpan.Start, this[Count - 1].FullSpan.End);
        }
    }

    /// <summary>
    /// The absolute span of the list elements in characters, not including the leading and trailing trivia of the 
    /// first and last elements.
    /// </summary>
    public TextSpan Span
    {
        get
        {
            if (Count == 0)
            {
                return default;
            }

            return TextSpan.FromBounds(this[0].Span.Start, this[Count - 1].Span.End);
        }
    }

    /// <summary>
    /// Returns the string representation of the nodes in this list, not including 
    /// the first node's leading trivia and the last node's trailing trivia.
    /// </summary>
    /// <returns>
    /// The string representation of the nodes in this list, not including 
    /// the first node's leading trivia and the last node's trailing trivia.
    /// </returns>
    public override string ToString() => InternalNode?.ToString() ?? string.Empty;

    /// <summary>
    /// Returns the full string representation of the nodes in this list including 
    /// the first node's leading trivia and the last node's trailing trivia.
    /// </summary>
    /// <returns>
    /// The full string representation of the nodes in this list including 
    /// the first node's leading trivia and the last node's trailing trivia.
    /// </returns>
    public string ToFullString() => InternalNode?.ToFullString() ?? string.Empty;

    public bool Equals(SyntaxList<TNode> other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is SyntaxList<TNode> other && Equals(other);

    public override int GetHashCode() => InternalNode?.GetHashCode() ?? 0;

    public IEnumerator<TNode> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(SyntaxList<TNode> left, SyntaxList<TNode> right) => left.Equals(right);

    public static bool operator !=(SyntaxList<TNode> left, SyntaxList<TNode> right) => !left.Equals(right);
}
