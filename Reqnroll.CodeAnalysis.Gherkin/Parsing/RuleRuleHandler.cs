using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class RuleRuleHandler() :
    BaseRuleHandler(RuleType.Rule), ISyntaxBuilder
{
    private RuleHeaderRuleHandler? _header;

    private BackgroundRuleHandler? _background;

    private readonly List<ISyntaxBuilder> _members = new();

    public InternalNode? CreateSyntax()
    {
        if (_header == null)
        {
            return null;
        }

        var members = _members
            .Select(handler => handler.CreateSyntax()!)
            .Where(syntax => syntax != null)
            .ToList();

        return Rule(
            _header.Tags,
            _header.Keyword ?? MissingToken(SyntaxKind.RuleKeyword),
            _header.Colon ?? MissingToken(SyntaxKind.ColonToken),
            _header.Name ?? MissingToken(SyntaxKind.NameToken),
            _header.Description,
            _background?.CreateBackgroundSyntax(),
            members.Count == 0 ? null : InternalSyntaxList.Create(members));
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        switch (ruleType)
        {
            case RuleType.RuleHeader:
                CodeAnalysisDebug.Assert(_header == null, "Duplicate header from parser.");
                return _header = new();

            case RuleType.Background:
                CodeAnalysisDebug.Assert(_background == null, "Duplicate background from parser.");
                return _background = new();

            case RuleType.ScenarioDefinition:
                var scenarioHandler = new ScenarioDefinitionRuleHandler();
                _members.Add(scenarioHandler);
                return scenarioHandler;
        }

        return base.StartChildRule(ruleType);
    }
}
