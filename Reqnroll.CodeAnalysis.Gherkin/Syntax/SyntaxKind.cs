namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Defines every kind of syntax element found in a Gherkin document.
/// </summary>
public enum SyntaxKind : ushort
{
    /// <summary>
    /// The node's kind is not known.
    /// </summary>
    None = 0,

    /// <summary>
    /// The node is a list of nodes.
    /// </summary>
    List,

    /// <summary>
    /// The node is the root structure of a feature file.
    /// </summary>
    FeatureFile,

    /// <summary>
    /// The node is the root structure of a Gherkin document.
    /// </summary>
    GherkinDocument,

    // Textless syntax tokens //

    /// <summary>
    /// The token is the "language" keyword.
    /// </summary>
    LanguageKeyword,

    /// <summary>
    /// The token is a colon <c>:</c>
    /// </summary>
    ColonToken, // This is assumed to be the first textless syntax token.

    /// <summary>
    /// The token is the at sign <c>@</c>
    /// </summary>
    AtToken,

    /// <summary>
    /// The token is the asterisk symbol <c>*</c>
    /// </summary>
    AsterixToken,

    /// <summary>
    /// The token is the less-than symbol <c>&lt;</c>
    /// </summary>
    LessThanToken,

    /// <summary>
    /// The token is the greater-than symbol <c>&gt;</c>
    /// </summary>
    GreaterThanToken,

    /// <summary>
    /// The token is a vertical bar <c>|</c>
    /// </summary>
    VerticalBarToken,

    /// <summary>
    /// The token is a hash symbol <c>#</c>
    /// </summary>
    HashToken,

    /// <summary>
    /// The token marks the end of the file.
    /// </summary>
    EndOfFileToken, // This is assumed to be the last textless syntax token.

    // Keywords //

    /// <summary>
    /// The token is a "feature" keyword.
    /// </summary>
    FeatureKeyword,

    /// <summary>
    /// The token is a "background" keyword.
    /// </summary>
    BackgroundKeyword,

    /// <summary>
    /// The token is a "rule" keyword.
    /// </summary>
    RuleKeyword,

    /// <summary>
    /// The token is a "scenario" keyword.
    /// </summary>
    ScenarioKeyword,

    /// <summary>
    /// The token is an "examples" keyword.
    /// </summary>
    ExamplesKeyword,

    /// <summary>
    /// The token is a "given" keyword.
    /// </summary>
    GivenKeyword,

    /// <summary>
    /// The token is a "when" keyword.
    /// </summary>
    WhenKeyword,

    /// <summary>
    /// The token is a "then" keyword.
    /// </summary>
    ThenKeyword,

    /// <summary>
    /// The token is an "and" keyword.
    /// </summary>
    AndKeyword,

    /// <summary>
    /// The token is a "but" keyword.
    /// </summary>
    ButKeyword,

    // Other text-containing tokens //

    /// <summary>
    /// The token is a delimiter that marks the start or end of a Doc String, typically """ or ```.
    /// </summary>
    DocStringDelimiterToken,

    /// <summary>
    /// The token is the identifier of a Doc String content type, such as "json" or "xml".
    /// </summary>
    DocStringContentTypeIdentifierToken,

    /// <summary>
    /// The token is a literal string within an interpolated text.
    /// </summary>
    InterpolatedLiteralToken,

    /// <summary>
    /// The token is a literal string within an interpolated text in a table.
    /// </summary>
    InterpolatedTableLiteralToken,

    /// <summary>
    /// The token is an identifier.
    /// </summary>
    IdentifierToken,

    /// <summary>
    /// The token is text from inside a table.
    /// </summary>
    TableLiteralToken,

    /// <summary>
    /// The token is literal text.
    /// </summary>
    LiteralToken, // This is assumed to be the last syntax token.

    // Trivia tokens //

    /// <summary>
    /// The trivia is a section of whitespace.
    /// </summary>
    WhitespaceTrivia,

    /// <summary>
    /// The trivia marking the end of a line.
    /// </summary>
    EndOfLineTrivia,

    /// <summary>
    /// The trivia is a comment.
    /// </summary>
    CommentTrivia,

    /// <summary>
    /// The trivia is a comment to select the language.
    /// </summary>
    LanguageCommentTrivia,

    /// <summary>
    /// The trivia is one or more tokens which have been skipped by the parser. This is typically due to an error in syntax.
    /// </summary>
    SkippedTokensTrivia,

    // Declarations //

    /// <summary>
    /// The node is a feature declaration.
    /// </summary>
    Feature,

    /// <summary>
    /// The node is a rule declaration.
    /// </summary>
    Rule,

    /// <summary>
    /// The node is a background declaration.
    /// </summary>
    Background,

    /// <summary>
    /// The node is a scenario declaration.
    /// </summary>
    Scenario,

    /// <summary>
    /// The node is a scenario outline declaration.
    /// </summary>
    ScenarioOutline,

    /// <summary>
    /// The node is a declaration of examples for an outline scenario.
    /// </summary>
    Examples,

    /// <summary>
    /// The node is a tag.
    /// </summary>
    Tag,

    /// <summary>
    /// The node is a step.
    /// </summary>
    Step,

    /// <summary>
    /// The node is a table within a step.
    /// </summary>
    StepTable,

    /// <summary>
    /// The node is a docstring within a step.
    /// </summary>
    StepDocString,

    /// <summary>
    /// The node is text with no additional meaning.
    /// </summary>
    LiteralText,

    /// <summary>
    /// The node is text which includes one or more parameters.
    /// </summary>
    InterpolatedText,

    /// <summary>
    /// The node is a literal text within an interpolated text.
    /// </summary>
    InterpolatedTextLiteral,

    /// <summary>
    /// The node is a parameter interpolation within an interpolated text.
    /// </summary>
    Interpolation,

    /// <summary>
    /// The node is a table.
    /// </summary>
    Table,

    /// <summary>
    /// The node is a data row within a table.
    /// </summary>
    TableRow,

    /// <summary>
    /// The node is a Doc String, a delimited, multi-line block of text.
    /// </summary>
    DocString,

    /// <summary>
    /// The node is the content-type identifier of a Doc String, such as "json" or "xml".
    /// </summary>
    DocStringContentType,
}
