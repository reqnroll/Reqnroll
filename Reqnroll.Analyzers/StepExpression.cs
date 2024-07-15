using CucumberExpressions.Parsing;
using System.Collections.Immutable;

namespace Reqnroll.Analyzers;
public class StepExpression(ImmutableArray<CaptureGroup> captureGroups)
{
    public ImmutableArray<CaptureGroup> CaptureGroups { get; } = captureGroups;

    public static ParseResult<StepExpression> Parse(string s)
    {
        var parser = new CucumberExpressionParser();

        var node = parser.Parse(s);

        var groups = node.Nodes
            .Where(node => node.Type == CucumberExpressions.Ast.NodeType.PARAMETER_NODE)
            .Select(node => new CaptureGroup(node.Text))
            .ToImmutableArray();

        return new StepExpression(groups);

        //return new ParsingError("ruh-roh", new Position(1, 2));
    }
}

public class CaptureGroup(string text)
{
    public string Text { get; } = text;
}
