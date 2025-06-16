using Microsoft.CodeAnalysis.Operations;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public static class SeparatedSyntaxList
{
}

public struct SeparatedSyntaxList<TNode> where TNode : SyntaxNode
{
    private readonly SyntaxNode _parent;

    internal SeparatedSyntaxList(InternalNode? node, SyntaxNode parent, int position)
    {
        InternalNode = node;
        _parent = parent;
        Position = position;
    }

    public SeparatedSyntaxList(IEnumerable<SyntaxNodeOrToken> nodes)
    {
        throw new NotImplementedException();
    }

    internal InternalNode? InternalNode { get; }

    public int Position { get; }

    public static bool operator ==(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right) =>
        left.Equals(right);

    public static bool operator !=(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right) =>
        !left.Equals(right);

    public override bool Equals(object? obj)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
