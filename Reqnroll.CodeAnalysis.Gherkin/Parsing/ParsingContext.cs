using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

/// <summary>
/// Contains contextual information for the bulding process.
/// </summary>
/// <param name="text">The source text being parsed.</param>
internal class ParsingContext(SourceText text)
{
    /// <summary>
    /// The leading trivia to be included with the next significant node.
    /// </summary>
    private InternalNode? _trivia;

    /// <summary>
    /// The tokens to be included as skipped tokens.
    /// </summary>
    private InternalNode? _skippedTokens;

    /// <summary>
    /// The diagnostic associated with the skipped tokens.
    /// </summary>
    private InternalDiagnostic? _skippedTokensDiagnostic;

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
    /// <remarks>
    /// <para>We delay constructing the trivia until no further skipped tokens are added to this particular failure 
    /// to ensure the diagnostic is attached to all tokens as one group. Calling <see cref="AddSkippedToken"/>
    /// with a new diagnostic will cause the buffered tokens to be added to buffered trivia using the buffered diagnostic
    /// before creating a new buffer from the specified skipped tokens.</para>
    /// </remarks>
    public void AddSkippedToken(InternalNode? skippedTokens, InternalDiagnostic diagnostic)
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
    /// Adds any whitespace from the start of a token to be included with the next syntax token.
    /// </summary>
    /// <param name="token">The token to check for leading trivia.</param>
    /// <param name="line">The line to read the trivia from.</param>
    public void AddLeadingWhitespace(Token token, TextLine line)
    {
        if (token.MatchedIndent == 0)
        {
            return;
        }

        AddLeadingWhitespace(new TextSpan(line.Start, token.MatchedIndent));
    }

    /// <summary>
    /// Adds leading trivia to be included with the next syntax token.
    /// </summary>
    /// <param name="trivia">A raw node representing the trivia to add. If the value is <c>null</c>, no node is added.</param>
    public void AddLeadingTrivia(InternalNode? trivia)
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
    /// Consumes all leading trivia that has been added to the context by calling <see cref="AddLeadingTrivia(InternalNode?)"/>, 
    /// emptying the buffer and returning a <see cref="InternalNode"/> that contains all the buffered trivia, or <c>null</c> if 
    /// no leading trivia has been buffered.
    /// </summary>
    /// <returns>A <see cref="InternalNode"/> that is the buffered leading trivia, or <c>null</c> if no leading trivia has been
    /// buffered.</returns>
    public InternalNode? ConsumeLeadingTrivia()
    {
        ConsumeSkippedTokensTrivia();

        var trivia = _trivia;

        _trivia = null;

        return trivia;
    }

    /// <summary>
    /// Consumes all leading trivia that has been added to the context by calling <see cref="AddLeadingTrivia(InternalNode?)"/>, 
    /// emptying the buffer and reading any whitespace on the specified line up to the specified token's indent,
    /// returning a <see cref="InternalNode"/> that contains all the trivia, or <c>null</c> if 
    /// no leading trivia has been buffered or was read from the line.
    /// </summary>
    /// <param name="line">The line to read whitespace from.</param>
    /// <param name="token">The token to read until.</param>
    /// <returns>A <see cref="InternalNode"/> that is the buffered leading trivia and the whitespace preceeding the token, 
    /// or <c>null</c> if no leading trivia has been buffered and no whitespace preceeds the token.</returns>
    public InternalNode? ConsumeLeadingTriviaAndWhitespace(TextLine line, Token token)
    {
        return ConsumeLeadingTrivia() +
            SourceText.ConsumeWhitespace(line.Start, line.Start + token.Line.Indent);
    }
}
