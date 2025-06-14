using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class FeatureHeaderRuleHandler() : ParsingRuleHandler(RuleType.FeatureHeader)
{
    private DescriptionRuleHandler? _descriptionRuleHandler;

    public InternalNode? Keyword { get; private set; }

    public InternalNode? Colon { get; private set; }

    public InternalNode? Name { get; private set; }

    protected override void AppendFeatureLine(Token token, TextLine line, ParsingContext context)
    {
        // Convert the line into tokens such that all characters are consumed.
        // Feature lines have the following layout:
        //
        // [keyword][colon] [name] [end-of-line]
        //
        // Leading whitespace characters are tracked by the Gherkin parser.
        // The parser also provides the keyword text (without the trailing colon) and position, and the name text.

        // Extract the whitespace between the colon and feature name.
        // Should just be a space, but we can read to be sure.
        var colonPosition = line.Start + token.Line.Indent + token.MatchedKeyword.Length;
        var colonWhitespace = context.SourceText.ConsumeWhitespace(colonPosition + 1, line.End);

        Keyword = Token(context.ConsumeLeadingTrivia(), SyntaxKind.FeatureKeyword, token.MatchedKeyword, null);
        Colon = Token(null, SyntaxKind.ColonToken, colonWhitespace);

        // Extract any whitespace between the end of the feature name and the end of the line.
        var featureNameEndPosition = colonPosition + (colonWhitespace?.Width ?? 0) + token.MatchedText.Length;
        InternalNode? nameWhitespace = context.SourceText
            .ConsumeWhitespace(featureNameEndPosition, line.End);

        nameWhitespace += line.GetEndOfLineTrivia();

        Name = LiteralText(
            Literal(null, LiteralEncoding.EncodeLiteralForDisplay(token.MatchedText), token.MatchedText, nameWhitespace));
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        if (ruleType == RuleType.Description)
        {
            Debug.Assert(_descriptionRuleHandler == null, "Duplicate description from parser.");
            return _descriptionRuleHandler = new();
        }

        return base.StartChildRule(ruleType);
    }

    //public DescriptionSyntax.Internal? Description => _descriptionRuleHandler?.CreateDescriptionSyntax();
}
