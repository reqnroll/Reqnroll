using Cucumber.TagExpressions;
using System.Collections.Generic;

namespace Reqnroll.Bindings.Discovery;

public class ReqnrollTagExpression(ITagExpression inner, string tagExpressionText) : ITagExpression
{
    public string TagExpressionText { get; } = tagExpressionText;

    public override string ToString()
    {
        return inner.ToString();
    }

    public virtual bool Evaluate(IEnumerable<string> inputs)
    {
        return inner.Evaluate(inputs);
    }
}
