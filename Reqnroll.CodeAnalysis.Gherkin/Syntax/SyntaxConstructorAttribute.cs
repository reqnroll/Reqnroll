namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Defines a group of slots that are valid for composing the syntax node.
/// </summary>
/// <param name="slots">The names of the slots which form a valid group for the syntax node.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SyntaxConstructorAttribute(params string[] slots) : Attribute
{
    /// <summary>
    /// Gets the names of the slots which form a valid group for the syntax node.
    /// </summary>
    public IReadOnlyList<string> Slots { get; } = slots;
}
