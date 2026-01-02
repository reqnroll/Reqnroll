using Cucumber.TagExpressions;
using System;
using System.Xml.Linq;

namespace Reqnroll.Bindings.Discovery;

public class ReqnrollTagExpressionParser() : IReqnrollTagExpressionParser
{
    public ITagExpression Parse(string tagExpression)
    {
        var tagExpressionParser = new TagExpressionParser();
        return Rewrite(tagExpressionParser.Parse(tagExpression));
    }

    // iff the expression is a literal node, prefix it with '@' if not already present
    private ITagExpression Rewrite(ITagExpression expression)
    {
        if (expression is LiteralNode)
        {
            return PrefixLiteralNode(expression);
        }
        if (ConfirmExpressionHasAtPrefixes(expression))
            return expression;
        throw new TagExpressionException("In multi-term tag expressions, all tag names must start with '@'.");
    }

    private bool ConfirmExpressionHasAtPrefixes(ITagExpression expression)
    {
        switch (expression)
        {
            case NullExpression nullNode:
                return true;
            case BinaryOpNode binaryNode:
                return ConfirmExpressionHasAtPrefixes(binaryNode.Left) && ConfirmExpressionHasAtPrefixes(binaryNode.Right);
            case NotNode notNode:
                return ConfirmExpressionHasAtPrefixes(notNode.Operand);
            case LiteralNode literalNode:
                return (literalNode.Name.StartsWith("@"));
            default:
                throw new InvalidOperationException($"Unknown tag expression node type: {expression.GetType().FullName}");
        }
    }

    private ITagExpression PrefixLiteralNode(ITagExpression expression)
    {
        var literalNode = (LiteralNode)expression;
        if (literalNode.Name.IsNullOrEmpty() || literalNode.Name.StartsWith("@"))
            return literalNode;

        return new LiteralNode("@" + literalNode.Name);
    }
}
