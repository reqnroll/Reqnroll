namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Defines the common elements of declarations.
/// </summary>
public interface IDeclarationSyntax
{
    /// <summary>
    /// Gets the tags associated with this declaration.
    /// </summary>
    SyntaxTokenList Tags { get; }

    /// <summary>
    /// Gets the keyword of the declaration. This identifies which type of declaration this is, such as Feature or Scenario.
    /// </summary>
    SyntaxToken Keyword { get; }

    /// <summary>
    /// Gets the name of the declared element.
    /// </summary>
    SyntaxToken Name { get; }
}
