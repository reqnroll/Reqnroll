using Cucumber.TagExpressions;
using System;

namespace Reqnroll.Bindings.Discovery;

public class InvalidTagExpression : ITagExpression
{
    public string Message { get; }
    public string OriginalTagExpression { get; }
    public InvalidTagExpression(string originalTagExpression, string message)
    {
        OriginalTagExpression = originalTagExpression;
        Message = message;
    }
    public bool Evaluate(System.Collections.Generic.IEnumerable<string> tags)
    {
        throw new InvalidOperationException("Cannot evaluate an invalid tag expression: " + Message);
    }
    public override string ToString()
    {
        return "Invalid Tag Expression: " + Message;
    }
}
