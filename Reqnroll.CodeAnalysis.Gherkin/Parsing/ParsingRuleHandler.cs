using Gherkin;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal abstract class ParsingRuleHandler(RuleType ruleType)
{
    public RuleType RuleType => ruleType;

    public virtual ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportChildRuleType,
            GetType().Name,
            ruleType));
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
        context.AddLeadingWhitespace(token, line);
        context.AddLeadingTrivia(line.GetEndOfLineTrivia());
    }

    protected virtual void AppendComment(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingComments, GetType().Name));
    }

    protected virtual void AppendTagLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingTagLines, GetType().Name));
    }

    protected virtual void AppendFeatureLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingFeatureLines, GetType().Name));
    }

    protected virtual void AppendRuleLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingRuleLines, GetType().Name));
    }

    protected virtual void AppendBackgroundLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingBackgroundLines, GetType().Name));
    }

    protected virtual void AppendScenarioLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingScenarioLines, GetType().Name));
    }

    protected virtual void AppendExamplesLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingExamplesLines, GetType().Name));
    }

    protected virtual void AppendStepLine(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingStepLines, GetType().Name));
    }

    protected virtual void AppendDocStringSeparator(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingDocStringSeparator, GetType().Name));
    }

    protected virtual void AppendTableRow(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingTableRows, GetType().Name));
    }

    protected virtual void AppendLanguage(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingLanguageComments, GetType().Name));
    }

    protected virtual void AppendOther(Token token, TextLine line, ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingOtherText, GetType().Name));
    }

    protected virtual void AppendEndOfFile(ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingEndOfFileMarker, GetType().Name));
    }

    public virtual void AppendUnexpectedToken(
        Token token,
        TextLine line,
        UnexpectedTokenException exception,
        ParsingContext context)
    {
        throw new NotSupportedException(
            string.Format(ParsingExceptionMessages.RuleHandlerDoesNotSupportAddingUnexpectedToken, GetType().Name));
    }
}
