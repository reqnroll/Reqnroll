using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal class TagsRuleHandler() : ParsingRuleHandler(RuleType.Tags)
{
    public InternalNode? Tags { get; private set; }

    protected override void AppendTagLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotImplementedException("I need to see the tokens.");
    }
}
