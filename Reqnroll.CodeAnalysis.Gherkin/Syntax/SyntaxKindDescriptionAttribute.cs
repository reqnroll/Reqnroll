namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Provides a human-readable description of a syntax kind.
/// </summary>
/// <param name="value">The description of the syntax.</param>
[AttributeUsage(AttributeTargets.Field,  AllowMultiple = false)]
internal class SyntaxKindDescriptionAttribute(string value) : Attribute
{
    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Value { get; } = value;
}
