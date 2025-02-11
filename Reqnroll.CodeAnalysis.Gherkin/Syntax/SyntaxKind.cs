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

    // Textless syntax tokens //

    /// <summary>
    /// The token is a colon <c>:</c>
    /// </summary>
    ColonToken, // This is assumed to be the first textless syntax token.

    /// <summary>
    /// The token marks the end of the file.
    /// </summary>
    EndOfFileToken, // This is assumed to be the last textless syntax token.

    // Keywords //

    /// <summary>
    /// The token is a feature keyword.
    /// </summary>
    FeatureKeyword,

    // Other text-containing tokens //

    /// <summary>
    /// The token is an identifier, such as the name of a feature or scenario.
    /// </summary>
    IdentifierToken,

    /// <summary>
    /// The token is a section of text which has no additional syntatic meaning. 
    /// </summary>
    TextLiteralToken, // This is assumed to be the last syntax token.

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

    // Components // 

    /// <summary>
    /// The node is a description.
    /// </summary>
    Description,

    // Declarations //

    /// <summary>
    /// The node is a feature declaration.
    /// </summary>
    FeatureDeclaration,

    /// <summary>
    /// The node is a rule declaration.
    /// </summary>
    RuleDeclaration,

    /// <summary>
    /// The node is a background declaration.
    /// </summary>
    BackgroundDeclaration,

    /// <summary>
    /// The node is a scenario declaration.
    /// </summary>
    ScenarioDeclaration
}
