using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class StepRuleHandler() : ParsingRuleHandler(RuleType.Step)
{
    private DataTableRuleHandler? _dataTableRuleHandler;

    public InternalNode? Keyword { get; set; }

    public InternalNode? Text { get; set; }

    protected override void AppendStepLine(Token token, TextLine line, ParsingContext context)
    {
        // Step lines have the following layout:
        //
        // [keyword] [text] [end-of-line]

        // Consume any leading or trailing trivia.
        var leading = context.ConsumeLeadingTriviaAndWhitespace(line, token);
        var trailing = line.GetEndOfLineTrivia();

        // The keyword may come with trailing trivia that we will shave off.
        var keywordText = token.MatchedKeyword.TrimEnd();
        var keywordPosition = line.Start + token.Line.Indent;
        var keywordWhitespace = context.SourceText.ConsumeWhitespace(
            keywordPosition + keywordText.Length,
            line.End);

        // The parser doesn't tell us what kind of keyword it is, but we know the dialect, so we can do a simple match.
        var keywordKind = GetKeywordKind(token.MatchedGherkinDialect, token.MatchedKeyword);

        // The step text is the rest of the line, but we may have consumed some whitespace.
        var text = token.MatchedText.Trim();
        var textPosition = keywordPosition + keywordText.Length + (keywordWhitespace?.Width ?? 0);
        var textWhitespace = context.SourceText.ConsumeWhitespace(
            textPosition + text.Length,
            line.End);

        // Create the tokens for the step.
        Keyword = Token(leading, keywordKind, keywordText, keywordWhitespace);
        Text = Literal(
            null,
            SyntaxKind.StepTextToken,
            LiteralEscapingStyle.Default.Escape(text),
            text,
            textWhitespace + trailing);
    }

    private static SyntaxKind GetKeywordKind(GherkinDialect dialect, string matchedKeyword)
    {
        if (!dialect.StepKeywordTypes.TryGetValue(matchedKeyword, out var keywordKind))
        {
            CodeAnalysisDebug.Assert(
                false,
                "Parser matched a step keyword but {0} was not found in the dialect.",
                matchedKeyword);
        }

        return keywordKind switch
        { 
            StepKeywordType.Context => SyntaxKind.ContextStepKeyword,
            StepKeywordType.Action => SyntaxKind.ActionStepKeyword,
            StepKeywordType.Outcome => SyntaxKind.OutcomeStepKeyword,
            StepKeywordType.Conjunction => SyntaxKind.ConjunctionStepKeyword,
            StepKeywordType.Unknown => SyntaxKind.WildcardStepKeyword,
            _ => throw new NotSupportedException($"Keyword type {keywordKind} not implemented")
        };
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        switch (ruleType)
        {
            case RuleType.DataTable:
                CodeAnalysisDebug.Assert(_dataTableRuleHandler == null, "Duplicate data table from parser.");
                return _dataTableRuleHandler = new DataTableRuleHandler();
        }

        return base.StartChildRule(ruleType);
    }

    internal StepSyntax.Internal CreateStepSyntax()
    {
        InternalNode? data = null;

        if (_dataTableRuleHandler != null)
        {
            data = StepTable(_dataTableRuleHandler.CreateTableSyntax());
        }

        return Step(
            Keyword ?? MissingToken(SyntaxKind.WildcardStepKeyword),
            Text ?? MissingToken(SyntaxKind.NameToken),
            data);
    }

    public override string ToString() => $"{Keyword} {Text}";
}
