using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class ScenarioRuleHandler() : BaseRuleHandler(RuleType.Scenario)
{
    private readonly DeclarationHelper _declarationHelper = new(SyntaxKind.ExampleKeyword, true);

    private readonly List<StepRuleHandler> _steps = [];

    private DescriptionRuleHandler? _description;

    private ExamplesDefinitionRuleHandler? _examplesDefinition;

    public InternalNode? Keyword => _declarationHelper.Keyword;

    public InternalNode? Colon => _declarationHelper.Colon;

    public InternalNode? Name => _declarationHelper.Name;

    public ExampleSyntax.Internal CreateSyntax()
    {
        InternalSyntaxList<StepSyntax.Internal>? steps = null;

        if (_steps.Count > 0)
        {
            steps = InternalSyntaxList.Create(_steps.Select(handler => handler.CreateStepSyntax()));
        }

        return Example(
            null,
            Keyword ?? MissingToken(SyntaxKind.ExampleKeyword),
            Colon ?? MissingToken(SyntaxKind.ColonToken),
            Name ?? MissingToken(SyntaxKind.NameToken),
            _description?.CreateDescriptionSyntax(),
            steps,
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
        switch (ruleType)
        {
            case RuleType.Step:
                var stepHandler = new StepRuleHandler();
                _steps.Add(stepHandler);
                return stepHandler;

            case RuleType.Description:
                CodeAnalysisDebug.Assert(_description == null, "Duplicate description from parser.");
                return _description = new();

            case RuleType.ExamplesDefinition:
                CodeAnalysisDebug.Assert(_examplesDefinition == null, "Duplicate example definitions from parser.");
                return _examplesDefinition = new();
        }

        return base.StartChildRule(ruleType);
    }
}
