namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Indicates that the class is a syntax node.
/// </summary>
/// <param name="syntaxKind">The kind of syntax node associated with this class.</param>
[AttributeUsage(AttributeTargets.Class)]
public class SyntaxNodeAttribute(SyntaxKind syntaxKind) : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxNodeAttribute"/> class with no specific syntax kind.
    /// </summary>
    public SyntaxNodeAttribute() : this(SyntaxKind.None)
    {
    }

    /// <summary>
    /// Gets the kind of syntax associated with the class.
    /// </summary>
    public SyntaxKind SyntaxKind { get; } = syntaxKind;
}
