using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class BackgroundRuleHandler() : BaseRuleHandler(RuleType.Background)
{
    private readonly DeclarationHelper _declarationHelper = new(SyntaxKind.BackgroundKeyword, false);

    private readonly List<StepRuleHandler> _steps = [];
    private DescriptionRuleHandler? _descriptionRuleHandler;

    protected override void AppendBackgroundLine(Token token, TextLine line, ParsingContext context)
    {   
        // Background lines have the following layout:
        //
        // [keyword][colon] [name] [end-of-line]

        _declarationHelper.DeconstructDeclarationToken(token, line, context);
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        switch (ruleType)
        {
            case RuleType.Description:
                CodeAnalysisDebug.Assert(_descriptionRuleHandler == null, "Duplicate background from parser.");
                return _descriptionRuleHandler = new DescriptionRuleHandler();

            case RuleType.Step:
                var handler = new StepRuleHandler();
                _steps.Add(handler);
                return handler;
        }

        return base.StartChildRule(ruleType);
    }

    internal InternalNode? CreateBackgroundSyntax()
    {
        var steps = _steps.Select(handler => handler.CreateStepSyntax()).ToList();

        return Background(
            null,
            _declarationHelper.Keyword ?? MissingToken(SyntaxKind.BackgroundKeyword),
            _declarationHelper.Colon ?? MissingToken(SyntaxKind.ColonToken),
            _declarationHelper.Name,
            _descriptionRuleHandler?.CreateDescriptionSyntax(),
            steps.Count == 0 ? null : InternalSyntaxList.Create(steps));
    }
}
