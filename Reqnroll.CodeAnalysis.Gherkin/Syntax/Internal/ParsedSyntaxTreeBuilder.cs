using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.SourceGenerator.Gherkin.Syntax.Internal;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

using static InternalSyntaxFactory;

/// <summary>
/// A builder which receives events from the Gherkin parser and composes a <see cref="GherkinSyntaxTree"/> instance.
/// </summary>
/// <remarks>
/// <para>The <see cref="ParsedSyntaxTreeBuilder"/> provides a shim mechansim to interface with the standard Gherkin
/// parser in order to build the lossless <see cref="GherkinSyntaxTree"/>.</para>
/// <para>The builder is implemented using a stack of builders where each builder is responsible for constructing one
/// syntax node in the tree. As tokens are recieved from the parser, they are passed to the current syntax builder.
/// The builder inspects the token and converts it into <see cref="RawSyntaxToken"/> and <see cref="RawSyntaxTrivia"/>
/// values, building up the components of the syntax node.</para>
/// <para>When the parser signals the start of a new syntax node (identified by the <see cref="RuleType"/> enum), the
/// syntax builder is given the opportunity to continue accepting tokens, or delegate tokens to a child syntax builder
/// which is then treated as the current builder. Similarly, when the parser signals the end of a syntax node, the
/// syntax builder is given the opportunity to continue accepting tokens, or finish composition and delegate control back
/// to the parent builder.</para>
/// </remarks>
internal partial class ParsedSyntaxTreeBuilder : IAstBuilder<GherkinSyntaxTree>
{
    /// <summary>
    /// Contains contextual information for the bulding process.
    /// </summary>
    /// <param name="text">The source text being parsed.</param>
    class Context(SourceText text)
    {
        /// <summary>
        /// The leading trivia to be included with the next significant node.
        /// </summary>
        private RawNode? _trivia;

        /// <summary>
        /// The tokens to be included as skipped tokens.
        /// </summary>
        private RawNode? _skippedTokens;

        /// <summary>
        /// The diagnostic associated with the skipped tokens.
        /// </summary>
        private RawDiagnostic? _skippedTokensDiagnostic;

        /// <summary>
        /// Gets the source text being parsed.
        /// </summary>
        public SourceText SourceText { get; } = text;

        /// <summary>
        /// Adds tokens to the context which have been skipped by the parser. These tokens will be included
        /// as trivia the next time the trivia is consumed.
        /// </summary>
        /// <param name="skippedTokens">The skipped tokens to include.</param>
        /// <param name="diagnostic">The diagnostic (usually an error) which describes the reason why the tokens 
        /// were skipped.</param>
        public void AddSkippedToken(RawNode? skippedTokens, RawDiagnostic diagnostic)
        {
            if (_skippedTokensDiagnostic == null)
            {
                _skippedTokensDiagnostic = diagnostic;
            }
            else if (_skippedTokensDiagnostic != diagnostic)
            {
                ConsumeSkippedTokensTrivia();
                _skippedTokensDiagnostic = diagnostic;
            }

            _skippedTokens += skippedTokens;
        }

        /// <summary>
        /// Adds a span as leading whitespace trivia to be included with the next syntax token.
        /// </summary>
        /// <param name="span">The span to include as whitespace trivia.</param>
        public void AddLeadingWhitespace(TextSpan span)
        {
            if (span.IsEmpty)
            {
                return;
            }

            AddLeadingTrivia(Whitespace(SourceText, span)!);
        }

        /// <summary>
        /// Adds leading trivia to be included with the next syntax token.
        /// </summary>
        /// <param name="trivia">A raw node representing the trivia to add. If the value is <c>null</c>, no node is added.</param>
        public void AddLeadingTrivia(RawNode? trivia)
        {
            if (trivia != null)
            {
                ConsumeSkippedTokensTrivia();
            }

            _trivia += trivia;
        }

        private void ConsumeSkippedTokensTrivia()
        {
            if (_skippedTokens == null)
            {
                return;
            }

            _trivia += SkippedTokensTrivia(_skippedTokens);

            _skippedTokens = null;
            _skippedTokensDiagnostic = null;
        }

        /// <summary>
        /// Consumes all leading trivia that has been added to the context by calling <see cref="AddLeadingTrivia(RawNode?)"/>, 
        /// emptying the buffer and returning a <see cref="RawNode"/> that contains all the buffered trivia, or <c>null</c> if 
        /// no leading trivia has been buffered.
        /// buffered.
        /// </summary>
        /// <returns>A <see cref="RawNode"/> that is the buffered leading trivia, or <c>null</c> if no leading trivia has been
        /// buffered.</returns>
        public RawNode? ConsumeLeadingTrivia()
        {
            ConsumeSkippedTokensTrivia();

            var trivia = _trivia;

            _trivia = null;

            return trivia;
        }
    }

    abstract class RuleHandler(RuleType ruleType)
    {
        public RuleType RuleType => ruleType;

        public virtual RuleHandler StartChildRule(RuleType ruleType)
        {
            throw new NotSupportedException($"{GetType().Name} does not support having child rules of type {ruleType}");
        }

        /// <summary>
        /// Appends a token to the syntax being built.
        /// </summary>
        /// <param name="token">The token from the parser to add.</param>
        /// <param name="context">Contains contextual information about the building of the syntax tree.</param>
        public void AppendToken(Token token, Context context)
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

        protected virtual void AppendEmpty(Token token, TextLine line, Context context)
        {
            // Empty tokens are all either zero-width or all-whitespace and can be added to leading trivia.
            // Matched indentation will already be included by the generic line-handling behaviour.
            context.AddLeadingWhitespace(TextSpan.FromBounds(line.Start + token.MatchedIndent, line.End));
            context.AddLeadingTrivia(line.GetEndOfLineTrivia());
        }

        protected virtual void AppendComment(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding comments.");
        }

        protected virtual void AppendTagLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding tag lines.");
        }

        protected virtual void AppendFeatureLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding feature lines.");
        }

        protected virtual void AppendRuleLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding rule lines.");
        }

        protected virtual void AppendBackgroundLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding background lines.");
        }

        protected virtual void AppendScenarioLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding scenario lines.");
        }

        protected virtual void AppendExamplesLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding examples lines.");
        }

        protected virtual void AppendStepLine(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding step lines.");
        }

        protected virtual void AppendDocStringSeparator(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding doc string separator.");
        }

        protected virtual void AppendTableRow(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding table rows.");
        }

        protected virtual void AppendLanguage(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding language comments.");
        }

        protected virtual void AppendOther(Token token, TextLine line, Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding other text.");
        }

        protected virtual void AppendEndOfFile(Context context)
        {
            throw new NotSupportedException($"{GetType().Name} does not support adding an end of file marker.");
        }
    }

    class RootHandler() : RuleHandler(RuleType.None)
    {
        private GherkinDocumentRuleHandler? _gherkinDocumentHandler;

        public override RuleHandler StartChildRule(RuleType ruleType)
        {
            if (ruleType == RuleType.GherkinDocument)
            {
                Debug.Assert(_gherkinDocumentHandler == null, "Duplicate document rule from parser.");
                return _gherkinDocumentHandler = new GherkinDocumentRuleHandler();
            }

            throw new NotSupportedException($"{nameof(RootHandler)} does not support having child rules of type {ruleType}");
        }

        public SyntaxNode BuildFeatureFileSyntax()
        {
            Debug.Assert(_gherkinDocumentHandler != null, "No nodes recieved from parser.");
            return _gherkinDocumentHandler!.BuildFeatureFileSyntax().CreateSyntaxNode();
        }
    }

    class GherkinDocumentRuleHandler() : RuleHandler(RuleType.GherkinDocument)
    {
        private FeatureRuleHandler? _featureRuleHandler;

        private RawNode? _endOfFile;

        public FeatureFileSyntax BuildFeatureFileSyntax()
        { 
            return new FeatureFileSyntax(
                _featureRuleHandler?.CreateFeatureDeclarationSyntax(),
                _endOfFile ?? MissingToken(SyntaxKind.EndOfFileToken));
        }

        public override RuleHandler StartChildRule(RuleType ruleType)
        {
            switch (ruleType)
            {
                case RuleType.Feature:
                    Debug.Assert(_featureRuleHandler == null, "Duplicate feature rule from parser.");
                    return _featureRuleHandler = new FeatureRuleHandler();
            }

            return base.StartChildRule(ruleType);
        }

        protected override void AppendEndOfFile(Context context)
        {
            // End of file tokens are zero-width and can have no trailing trivia.
            // Any whitespace on the line or blank lines preceeding the end of file are associated with the token.
            _endOfFile = Token(context.ConsumeLeadingTrivia(), SyntaxKind.EndOfFileToken, null);
        }
    }

    class FeatureRuleHandler() : RuleHandler(RuleType.Feature)
    {
        private FeatureHeaderRuleHandler? _featureHeaderRuleHandler;

        public FeatureDeclarationSyntax? CreateFeatureDeclarationSyntax()
        {
            if (_featureHeaderRuleHandler == null)
            {
                return null;
            }

            return FeatureDeclaration(
                _featureHeaderRuleHandler.keyword ?? MissingToken(SyntaxKind.FeatureKeyword),
                _featureHeaderRuleHandler.colon ?? MissingToken(SyntaxKind.ColonToken),
                _featureHeaderRuleHandler.name ?? MissingToken(SyntaxKind.IdentifierToken),
                _featureHeaderRuleHandler.CreateDescriptionSyntax());
        }

        public override RuleHandler StartChildRule(RuleType ruleType)
        {
            if (ruleType == RuleType.FeatureHeader)
            {
                Debug.Assert(_featureHeaderRuleHandler == null, "Duplicate feature header from parser.");
                return _featureHeaderRuleHandler = new FeatureHeaderRuleHandler();
            }

            return base.StartChildRule(ruleType);
        }
    }

    class FeatureHeaderRuleHandler() : RuleHandler(RuleType.FeatureHeader)
    {
        private DescriptionRuleHandler? _descriptionRuleHandler;

        public RawNode? keyword;
        public RawNode? colon;
        public RawNode? name;

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
            RawNode? nameWhitespace = context.SourceText
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

        public DescriptionSyntax? CreateDescriptionSyntax() => _descriptionRuleHandler?.CreateDescriptionSyntax();
    }

    class DescriptionRuleHandler() : RuleHandler(RuleType.Description)
    {
        private readonly ImmutableArray<RawNode>.Builder _description = ImmutableArray.CreateBuilder<RawNode>();

        protected override void AppendOther(Token token, TextLine line, Context context)
        {
            // In the context of parsing a feature, "other" tokens are the plain-text lines that form the descriptive text
            // that follows the feature declaration and preceeds any rules or scenarios.
            // There can be multiple of these and they can be separated by all kinds of trivia, including comments.

            // Description lines have the following layout:
            //
            // [text-literal] [end-of-line]

            RawNode? trailing;

            // Blank lines will be matched as "other" in this context. These lines should be included as trivia, rather than tokens.
            if (token.MatchedText.Length == 0)
            {
                // The line is completely empty.
                trailing = line.GetEndOfLineTrivia();
                context.AddLeadingTrivia(trailing);
                return;
            }
            else if (token.MatchedText.All(char.IsWhiteSpace))
            {
                // The whole line is whitespace characters.
                throw new NotImplementedException();
            }

            // For "other" tokens, the parser's `MatchedText` includes all the whitespace on the source line.
            // We need to trim the start and end of the string to yield the correct output.
            var text = token.MatchedText.AsSpan(token.Line.Indent).TrimEnd().ToString();

            trailing = context.SourceText.ConsumeWhitespace(line.Start + token.Line.Indent + text.Length, line.End) +
                line.GetEndOfLineTrivia();

            _description.Add(Literal(context.ConsumeLeadingTrivia(), text, trailing));
        }

        public DescriptionSyntax? CreateDescriptionSyntax()
        {
            var text = RawNode.CreateList(_description);

            if (text == null)
            {
                return null;
            }

            return Description(text);
        }
    }

    private Context _context;

    //private FeatureFileSyntaxBuilder _rootBuilder;

    //private ISyntaxBuilder _currentSyntaxBuilder;

    private RootHandler _rootHandler;

    private readonly Stack<RuleHandler> _ruleHandlers = new();

    private RuleHandler CurrentRuleHandler => _ruleHandlers.Peek();

    private readonly GherkinParseOptions _parseOptions;

    private readonly string _filePath;

    private readonly CancellationToken _cancellationToken;

    private static readonly string[] TriviaTokenTypes = [
        "#Language",
        "#Comment",
        "#Empty",
        "#EOF"];

    private static readonly HashSet<string> FeatureTagTokenTypes = ["#FeatureLine", "#TagLine"];

    public ParsedSyntaxTreeBuilder(
        GherkinParseOptions parseOptions,
        SourceText text,
        string filePath,
        CancellationToken cancellationToken)
    {
        _context = new(text);
        //_rootBuilder = new();
        //_currentSyntaxBuilder = _rootBuilder;

        _rootHandler = new();
        _ruleHandlers.Push(_rootHandler);

        _parseOptions = parseOptions;
        _filePath = filePath;
        _cancellationToken = cancellationToken;
    }

    public void Reset()
    {
        _context = new(_context.SourceText);
        //_rootBuilder = new();
        //_currentSyntaxBuilder = _rootBuilder;

        _rootHandler = new();
        _ruleHandlers.Clear();
        _ruleHandlers.Push(_rootHandler);
    }

    /// <summary>
    /// Called by the parser to indicate a change in context at the start of a parsing rule.
    /// </summary>
    /// <param name="ruleType">A <see cref="RuleType"/> indiciating the type of rule the parser is at the start of.</param>
    public void StartRule(RuleType ruleType)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        //_currentSyntaxBuilder = _currentSyntaxBuilder.StartRule(ruleType);
        _ruleHandlers.Push(CurrentRuleHandler.StartChildRule(ruleType));
    }

    /// <summary>
    /// Called by the parser to indicate a change in context at the end of a parsing rule.
    /// </summary>
    /// <param name="ruleType">A <see cref="RuleType"/> indiciating the type of rule the parser is at the end of.</param>
    public void EndRule(RuleType ruleType)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        //_currentSyntaxBuilder = _currentSyntaxBuilder.EndRule(ruleType);

        Debug.Assert(
            CurrentRuleHandler.RuleType == ruleType,
            "Parser requsted to end rule that is not current.",
            "The current rule being processed is {0} but the parser instructed to end a rule of type {1}",
            CurrentRuleHandler.RuleType,
            ruleType);

        _ruleHandlers.Pop();
    }

    /// <summary>
    /// Called by the parser to add a read token to the syntax model. Each token represents one line of text which
    /// has been determined as valid by the parser.
    /// </summary>
    /// <param name="token">The token to add.</param>
    public void Build(Token token)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        //_currentSyntaxBuilder.Append(token, _context);

        CurrentRuleHandler.AppendToken(token, _context);
    }

    public GherkinSyntaxTree GetResult()
    {
        //var root = _rootBuilder.Build()!.CreateSyntaxNode();
        var root = _rootHandler.BuildFeatureFileSyntax();

        return new GherkinSyntaxTree(_context.SourceText, root, _parseOptions, _filePath);
    }

    internal void AddError(ParserException error)
    {
        // If the parser encounters an error, we still want to include the source text in the syntax tree.
        //
        // Where possible, we'll take a partially-valid line and include the syntax in the tree with missing tokens added
        // or invalid tokens skipped.
        //
        // If the line is completely invalid, we add it to the tree as "skipped tokens", in the trivia of tokens
        // at the point at which the parser was able to recover.

        var message = error.Message.Substring(error.Message.IndexOf(')') + 1);

        switch (error)
        {
            case UnexpectedTokenException ute:
                {
                    AddUnexpectedToken(ute, message);
                    break;
                }
        }
    }

    private void AddUnexpectedToken(UnexpectedTokenException exception, string message)
    {
        // Unexpected tokens are the parser's way of indicating it doesn't know how to interpret the current line.
        // A misspelled keyword or incorrect formatting of a line can cause this.
        // Although there are some scenarios the parser could examine in more depth (like missing a colon after a keyword)
        // for now we'll treat all these errors generically and treat them as skipped tokens.
        var token = exception.ReceivedToken;
        var line = _context.SourceText.Lines[token.Line.LineNumber - 1];
        var contentSpan = TextSpan.FromBounds(line.Start + token.MatchedIndent, line.End);

        RawNode? leadingTrivia = Whitespace(_context.SourceText, line.Start, token.MatchedIndent);
        var leadingWhitespace = _context.SourceText.ConsumeWhitespace(contentSpan);

        leadingTrivia += leadingWhitespace;

        var trailingWhitespace = _context.SourceText.ReverseConsumeWhitespace(contentSpan);
        RawNode? trailingTrivia = trailingWhitespace + line.GetEndOfLineTrivia();

        // All text remaining between any whitespace we'll treat as a literal.
        var literalSpan = TextSpan.FromBounds(
            contentSpan.Start + (leadingWhitespace?.Width ?? 0),
            contentSpan.End - (trailingWhitespace?.Width ?? 0));

        var literal = Literal(leadingTrivia, _context.SourceText.ToString(literalSpan) , trailingTrivia);

        // Add the skipped tokens to be included as leading trivia in the next token.
        // Inkeeping with the CS compiler error codes, we'll create a unique number for each combination of expected tokens.
        var diagnostic = GetUnexpectedTokenDiagnostic(exception.ExpectedTokenTypes);

        // We delay constructing the trivia until no further skipped tokens are added to this particular failure
        // to ensure the diagnostic is attached to the whole span.
        _context.AddSkippedToken(literal, diagnostic);
    }

    private static RawDiagnostic GetUnexpectedTokenDiagnostic(string[] expectedTokenTypes)
    {
        var expectedTokenSet = expectedTokenTypes.Except(TriviaTokenTypes).ToArray();

        if (FeatureTagTokenTypes.SetEquals(expectedTokenSet))
        {
            return new();
        }

        throw new NotImplementedException(
            "No error-code exists to handle when an unexpected token was encountered whilst expecting " +
            $"tokens {string.Join(", ", expectedTokenSet)}");
    }
}
