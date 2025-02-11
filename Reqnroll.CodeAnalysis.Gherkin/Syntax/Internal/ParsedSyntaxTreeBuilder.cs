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

    private Context _context;

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

        _rootHandler = new();
        _ruleHandlers.Push(_rootHandler);

        _parseOptions = parseOptions;
        _filePath = filePath;
        _cancellationToken = cancellationToken;
    }

    public void Reset()
    {
        _context = new(_context.SourceText);

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
        _ruleHandlers.Push(CurrentRuleHandler.StartChildRule(ruleType));
    }

    /// <summary>
    /// Called by the parser to indicate a change in context at the end of a parsing rule.
    /// </summary>
    /// <param name="ruleType">A <see cref="RuleType"/> indiciating the type of rule the parser is at the end of.</param>
    public void EndRule(RuleType ruleType)
    {
        _cancellationToken.ThrowIfCancellationRequested();

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
            // TODO: Add diagnostic information.
            return new();
        }

        throw new NotImplementedException(
            "No error-code exists to handle when an unexpected token was encountered whilst expecting " +
            $"tokens {string.Join(", ", expectedTokenSet)}");
    }
}
