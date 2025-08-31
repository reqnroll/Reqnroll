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
    /// The token is a colon <c>:</c>
    /// </summary>
    ColonToken, // This is assumed to be the first textless syntax token.

    /// <summary>
    /// The token is the at sign <c>@</c>
    /// </summary>
    AtToken,

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
    /// The token is an "example" keyword.
    /// </summary>
    ExampleKeyword,

    /// <summary>
    /// The token is an "examples" keyword.
    /// </summary>
    ExamplesKeyword,

    /// <summary>
    /// The token is a step keyword to establish context (e.g. "Given".)
    /// </summary>
    ContextStepKeyword,

    /// <summary>
    /// The token is a step keyword to perform an action (e.g. "When".)
    /// </summary>
    ActionStepKeyword,

    /// <summary>
    /// The token is a step keyword to test the outcome (e.g "Then".)
    /// </summary>
    OutcomeStepKeyword,

    /// <summary>
    /// The token is a step keyword that represents a continuation of a previous step (e.g. "And", "But".)
    /// </summary>
    ConjunctionStepKeyword,

    /// <summary>
    /// The token is a step keyword that matches any step type (e.g. "*".)
    /// </summary>
    WildcardStepKeyword,

    // Text literal tokens //

    /// <summary>
    /// The token is the identifier of a directive.
    /// </summary>
    DirectiveIdentifierToken,

    /// <summary>
    /// The token is the value of a directive.
    /// </summary>
    DirectiveValueToken,

    /// <summary>
    /// The token is text from inside a table cell.
    /// </summary>
    TableLiteralToken,

    /// <summary>
    /// The token is text from a description block, such as the description of a feature or scenario.
    /// </summary>
    DescriptionTextToken,

    /// <summary>
    /// The token is text from a step.
    /// </summary>
    StepTextToken,

    /// <summary>
    /// The token is the name of a declaration, such as a feature or scenario.
    /// </summary>
    NameToken,

    /// <summary>
    /// The token is the identifier of a Doc String content type, such as "json" or "xml".
    /// </summary>
    DocStringContentTypeIdentifierToken,

    /// <summary>
    /// The token is the content of a Doc String.
    /// </summary>
    DocStringContentToken,

    // Other text-containing tokens //

    /// <summary>
    /// The token is a delimiter that marks the start or end of a Doc String, typically """ or ```.
    /// </summary>
    DocStringDelimiterToken, // This is the last syntax token, used as a marker for checking enum ranges.

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
    /// The trivia is a directive comment, such as a language specification comment.
    /// </summary>
    DirectiveCommentTrivia,

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
    /// The node is a description block.
    /// </summary>
    Description,

    /// <summary>
    /// The node is an example declaration.
    /// </summary>
    Example,

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
    /// The node is a table.
    /// </summary>
    Table,

    /// <summary>
    /// The node is a data row within a table.
    /// </summary>
    TableRow,

    /// <summary>
    /// The node is a cell within a table row.
    /// </summary>
    TableCell,

    /// <summary>
    /// The node is a Doc String, a delimited, multi-line block of text.
    /// </summary>
    DocString,
}
