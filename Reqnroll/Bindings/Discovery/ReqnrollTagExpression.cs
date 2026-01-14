using Cucumber.TagExpressions;
using System.Collections.Generic;

namespace Reqnroll.Bindings.Discovery;

public class ReqnrollTagExpression : ITagExpression
{
    public ReqnrollTagExpression(ITagExpression inner, string tagExpressionText)
    {
        TagExpressionText = tagExpressionText;
        _inner = inner;
    }
    public string TagExpressionText { get; }

    private ITagExpression _inner;

    public virtual bool Evaluate(IEnumerable<string> inputs)
    {
        return _inner.Evaluate(inputs);
    }
}
