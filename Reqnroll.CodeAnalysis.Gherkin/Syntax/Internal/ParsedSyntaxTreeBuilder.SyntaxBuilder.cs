namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

using global::Gherkin;
using Microsoft.CodeAnalysis.Text;
using static InternalSyntaxFactory;

internal partial class ParsedSyntaxTreeBuilder
{
    /// <summary>
    /// Defines the syntax builder interface to control the composition of syntax nodes.
    /// </summary>
    interface ISyntaxBuilder
    {
        /// <summary>
        /// Adds a token to the syntax being built.
        /// </summary>
        /// <param name="token">The token to add.</param>
        /// <param name="context">Contextual information about the building process.</param>
        void Append(Token token, Context context);

        /// <summary>
        /// Called when the parser has started to consume a new element matching the specified rule.
        /// </summary>
        /// <param name="ruleType">The type of rule being processed.</param>
        /// <returns>The syntax builder that should be used to handle subsequent tokens.</returns>
        ISyntaxBuilder StartRule(RuleType ruleType);

        /// <summary>
        /// Called when the parser has reached the end of an element.
        /// </summary>
        /// <param name="ruleType">The type of rule the parser has reached the end of.</param>
        /// <returns>The syntax builder that should be used to handle subsequent tokens.</returns>
        ISyntaxBuilder EndRule(RuleType ruleType);
    }

    /// <summary>
    /// Provides the base for creating syntax builders.
    /// </summary>
    /// <typeparam name="TSyntax">The type of syntax produced by the builder.</typeparam>
    abstract class SyntaxBuilder<TSyntax> : ISyntaxBuilder where TSyntax : RawNode
    {
        /// <summary>
        /// Builds a syntax node based on the tokens added to the builder.
        /// </summary>
        /// <returns>An instance of the raw syntax node specified by <typeparamref name="TSyntax"/>, or <c>null</c> if
        /// the builder cannot create a syntax node from the tokens added so far.</returns>
        public abstract TSyntax? Build();

        /// <summary>
        /// Appends a token to the syntax being built.
        /// </summary>
        /// <param name="token">The token from the parser to add.</param>
        /// <param name="context">Contains contextual information about the building of the syntax tree.</param>
        public void Append(Token token, Context context)
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

        public abstract ISyntaxBuilder StartRule(RuleType ruleType);

        public abstract ISyntaxBuilder EndRule(RuleType ruleType);

        protected virtual void AppendEmpty(Token token, TextLine line, Context context)
        {
            // Empty tokens are all either zero-width or all-whitespace and can be added to leading trivia.
            context.AddLeadingWhitespace(line.Span);

            if (line.End != line.EndIncludingLineBreak)
            {
                context.AddLeadingTrivia(
                    EndOfLine(context.SourceText, TextSpan.FromBounds(line.End, line.EndIncludingLineBreak)));
            }
        }

        protected virtual void AppendComment(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendTagLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendFeatureLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendRuleLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendBackgroundLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendScenarioLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendExamplesLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendStepLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendDocStringSeparator(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendTableRow(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendLanguage(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        protected virtual void AppendOther(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException();
        }

        public virtual void AppendEndOfFile(Context context)
        {
            throw new NotSupportedException();
        }
    }
}
