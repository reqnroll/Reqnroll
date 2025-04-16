using Gherkin;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal abstract class ParsingRuleHandler(RuleType ruleType)
{
    public RuleType RuleType => ruleType;

    public virtual ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        throw new NotSupportedException($"{GetType().Name} does not support having child rules of type {ruleType}");
    }

    /// <summary>
    /// Appends a token to the syntax being built.
    /// </summary>
    /// <param name="token">The token from the parser to add.</param>
    /// <param name="context">Contains contextual information about the building of the syntax tree.</param>
    public void AppendToken(Token token, ParsingContext context)
    {
        if (token.MatchedType == TokenType.EOF)
        {
            AppendEndOfFile(context);
            return;
        }

        var line = context.SourceText.Lines[token.Line.LineNumber - 1];

        // Capture any leading whitespace.
        if (token.Line.Indent > 0)
        {
            context.AddLeadingWhitespace(new TextSpan(line.Start, token.Line.Indent));
        }

        switch (token.MatchedType)
        {
            case TokenType.Empty: AppendEmpty(token, line, context); break;
            case TokenType.Comment: AppendComment(token, line, context); break;
            case TokenType.TagLine: AppendTagLine(token, line, context); break;
            case TokenType.FeatureLine: AppendFeatureLine(token, line, context); break;
            case TokenType.RuleLine: AppendRuleLine(token, line, context); break;
            case TokenType.BackgroundLine: AppendBackgroundLine(token, line, context); break;
            case TokenType.ScenarioLine: AppendScenarioLine(token, line, context); break;
            case TokenType.ExamplesLine: AppendExamplesLine(token, line, context); break;
            case TokenType.StepLine: AppendStepLine(token, line, context); break;
            case TokenType.DocStringSeparator: AppendDocStringSeparator(token, line, context); break;
            case TokenType.TableRow: AppendTableRow(token, line, context); break;
            case TokenType.Language: AppendLanguage(token, line, context); break;
            case TokenType.Other: AppendOther(token, line, context); break;
            default: throw new InvalidOperationException();
        }
    }

    protected virtual void AppendEmpty(Token token, TextLine line, ParsingContext context)
    {
        // Empty tokens are all either zero-width or all-whitespace and can be added to leading trivia.
        // Matched indentation will already be included by the generic line-handling behaviour.
        context.AddLeadingWhitespace(TextSpan.FromBounds(line.Start + token.MatchedIndent, line.End));
        context.AddLeadingTrivia(line.GetEndOfLineTrivia());
    }

    protected virtual void AppendComment(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding comments.");
    }

    protected virtual void AppendTagLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding tag lines.");
    }

    protected virtual void AppendFeatureLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding feature lines.");
    }

    protected virtual void AppendRuleLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding rule lines.");
    }

    protected virtual void AppendBackgroundLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding background lines.");
    }

    protected virtual void AppendScenarioLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding scenario lines.");
    }

    protected virtual void AppendExamplesLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding examples lines.");
    }

    protected virtual void AppendStepLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding step lines.");
    }

    protected virtual void AppendDocStringSeparator(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding doc string separator.");
    }

    protected virtual void AppendTableRow(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding table rows.");
    }

    protected virtual void AppendLanguage(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding language comments.");
    }

    protected virtual void AppendOther(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding other text.");
    }

    protected virtual void AppendEndOfFile(ParsingContext context)
    {
        throw new NotSupportedException($"{GetType().Name} does not support adding an end of file marker.");
    }
}
