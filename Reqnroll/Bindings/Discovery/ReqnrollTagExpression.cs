using Cucumber.TagExpressions;
using System.Collections.Generic;

namespace Reqnroll.Bindings.Discovery;

public class ReqnrollTagExpression : ITagExpression
{
    public string TagExpressionText { get; }
    private ITagExpression _inner;

    public ReqnrollTagExpression(ITagExpression inner, string tagExpressionText)
    {
        TagExpressionText = tagExpressionText;
        _inner = inner;
    }

    public override string ToString()
    {
        return _inner.ToString();
    }

    public virtual bool Evaluate(IEnumerable<string> inputs)
    {
        return _inner.Evaluate(inputs);
    }
}
