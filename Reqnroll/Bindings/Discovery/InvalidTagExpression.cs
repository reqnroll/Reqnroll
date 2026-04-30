using Cucumber.TagExpressions;
using System;

namespace Reqnroll.Bindings.Discovery;
public class InvalidTagExpression(ITagExpression expression, string originalTagExpression, string message) : ReqnrollTagExpression(expression, originalTagExpression)
{
    public string Message { get; } = message;

    public override bool Evaluate(System.Collections.Generic.IEnumerable<string> tags)
    {
        throw new InvalidOperationException("Cannot evaluate an invalid tag expression: " + Message);
    }
    public override string ToString()
    {
        return "Invalid tag expression: " + Message;
    }
}
