namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Indicates that a property is a slot for syntax in a syntax node.
/// </summary>
/// <param name="syntaxKind">The kind of syntax expected in the slot.</param>
/// <param name="description">A description of the syntax contained by the slot.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SyntaxSlotAttribute(SyntaxKind syntaxKind, string description) : Attribute
{
    /// <summary>
    /// Gets the kind of syntax expected in the slot.
    /// </summary>
    public SyntaxKind SyntaxKind { get; } = syntaxKind;

    /// <summary>
    /// Gets a description of the syntax contained by the slot.
    /// </summary>
    public string Description { get; } = description;
}
