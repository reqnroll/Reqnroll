namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Indicates that a property is a slot for syntax in a syntax node.
/// </summary>
/// <param name="syntaxKind">The kinds of syntax expected in the slot.</param>
/// <param name="description">A description of the syntax contained by the slot.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SyntaxSlotAttribute(SyntaxKind[] syntaxKinds, string description) : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxSlotAttribute"/> class.
    /// </summary>
    /// <param name="syntaxKind">The kind of syntax expected in the slot.</param>
    /// <param name="description">A description of the syntax contained by the slot.</param>
    public SyntaxSlotAttribute(SyntaxKind syntaxKind, string description) : this(new[] { syntaxKind }, description)
    {
    }

    /// <summary>
    /// Gets the kinds of syntax expected in the slot.
    /// </summary>
    public SyntaxKind[] SyntaxKinds { get; } = syntaxKinds;

    /// <summary>
    /// Gets a description of the syntax contained by the slot.
    /// </summary>
    public string Description { get; } = description;
}
