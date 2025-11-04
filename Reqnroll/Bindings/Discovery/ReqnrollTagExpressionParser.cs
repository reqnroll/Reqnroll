using Cucumber.TagExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Bindings.Discovery
{
    public class ReqnrollTagExpressionParser(ITagExpressionParser tagExpressionParser) : ITagExpressionParser
    {
        public ITagExpression Parse(string tagExpression)
        {
            return Rewrite(tagExpressionParser.Parse(tagExpression));
        }

        private ITagExpression Rewrite(ITagExpression expression)
        {
            return expression switch
            {
                NullExpression nullExpression => nullExpression,
                NotNode notNode => new NotNode(Rewrite(notNode.Operand)),
                BinaryOpNode binaryOpNode => new BinaryOpNode(binaryOpNode.Op, Rewrite(binaryOpNode.Left), Rewrite(binaryOpNode.Right)),
                LiteralNode literalNode => new LiteralNode(PrefixLiteral(literalNode.Name)),
                _ => throw new NotSupportedException($"Unsupported tag expression type: {expression.GetType().FullName}"),
            };
        }

        private string PrefixLiteral(string name)
        {
            if (name.IsNullOrEmpty() )
                return name;
            if (name.StartsWith("@"))
                return name;
            return "@" + name;
        }
    }
}
