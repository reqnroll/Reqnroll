using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class ScenarioRuleHandler() : BaseRuleHandler(RuleType.Scenario)
{
    private readonly DeclarationHelper _declarationHelper = new(SyntaxKind.ExampleKeyword);

    private readonly List<StepRuleHandler> _steps = [];

    public InternalNode? Keyword => _declarationHelper.Keyword;
    public InternalNode? Colon => _declarationHelper.Colon;
    public InternalNode? Name => _declarationHelper.Name;

    public ExampleSyntax.Internal CreateSyntax()
    {
        var steps = _steps.Select(handler => handler.CreateStepSyntax()).ToList();

        return Example(
            null,
            Keyword ?? MissingToken(SyntaxKind.ExampleKeyword),
            Colon ?? MissingToken(SyntaxKind.ColonToken),
            Name,
            null,
            InternalSyntaxList.Create(steps),
            null);
    }

    protected override void AppendScenarioLine(Token token, TextLine line, ParsingContext context)
    {
        // Scenario lines have the following layout:
        //
        // [keyword][colon] [name] [end-of-line]
        _declarationHelper.DeconstructDeclarationToken(token, line, context);
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        if (ruleType == RuleType.Step)
        {
            var stepHandler = new StepRuleHandler();
            _steps.Add(stepHandler);
            return stepHandler;
        }

        return base.StartChildRule(ruleType);
    }
}
