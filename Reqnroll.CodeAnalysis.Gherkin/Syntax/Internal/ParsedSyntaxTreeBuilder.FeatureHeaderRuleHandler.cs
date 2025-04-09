﻿using Gherkin;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

using static InternalSyntaxFactory;

internal partial class ParsedSyntaxTreeBuilder
{
    class FeatureHeaderRuleHandler() : RuleHandler(RuleType.FeatureHeader)
    {
        private DescriptionRuleHandler? _descriptionRuleHandler;

        public InternalNode? keyword;
        public InternalNode? colon;
        public InternalNode? name;

        protected override void AppendFeatureLine(Token token, TextLine line, Context context)
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

            keyword = Token(context.ConsumeLeadingTrivia(), SyntaxKind.FeatureKeyword, token.MatchedKeyword, null);
            colon = Token(null, SyntaxKind.ColonToken, colonWhitespace);

            // Extract any whitespace between the end of the feature name and the end of the line.
            var featureNameEndPosition = colonPosition + (colonWhitespace?.Width ?? 0) + token.MatchedText.Length;
            InternalNode? nameWhitespace = context.SourceText
                .ConsumeWhitespace(featureNameEndPosition, line.End);

            nameWhitespace += line.GetEndOfLineTrivia();

            name = Identifier(null, token.MatchedText, nameWhitespace);
        }

        public override RuleHandler StartChildRule(RuleType ruleType)
        {
            if (ruleType == RuleType.Description)
            {
                Debug.Assert(_descriptionRuleHandler == null, "Duplicate description from parser.");
                return _descriptionRuleHandler = new();
            }

            return base.StartChildRule(ruleType);
        }

        public DescriptionSyntax.Internal? CreateDescriptionSyntax() => _descriptionRuleHandler?.CreateDescriptionSyntax();
    }
}
