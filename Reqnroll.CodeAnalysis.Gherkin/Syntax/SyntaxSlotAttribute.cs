namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Indicates that a property is a slot for syntax in a syntax node.
/// </summary>
/// <param name="syntaxKind">The kind of syntax expected in the slot.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SyntaxSlotAttribute(SyntaxKind syntaxKind) : Attribute
{
    /// <summary>
    /// Gets the kind of syntax expected in the slot.
    /// </summary>
    public SyntaxKind SyntaxKind { get; } = syntaxKind;
}
