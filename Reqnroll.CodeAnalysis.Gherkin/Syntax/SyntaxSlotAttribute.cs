namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Indicates that a property is a slot for syntax in a syntax node.
/// </summary>
/// <param name="syntaxKinds">The kinds of syntax expected in the slot.</param>
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

    /// <summary>
    /// Gets or sets the name of the slot this slot is located immediately after.
    /// </summary>
    /// <remarks>
    /// <para>By default, slots are arranged in the order in which they are declared. Inherited slots are placed
    /// <em>after</em> the slots declared by the type. The <see cref="LocatedAfter"/> property allows the leading 
    /// slot to be specified, indicating this slot should be immediately after.</para>
    /// <para>Setting the property to the name of the slot declared before this slot is the same as the default
    /// behavior.</para>
    /// </remarks>
    public string? LocatedAfter { get; set; }
}
