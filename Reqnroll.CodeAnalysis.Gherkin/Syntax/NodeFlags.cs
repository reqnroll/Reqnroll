namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Flags which identify the specific qualities of a node.
/// </summary>
[Flags]
internal enum NodeFlags
{
    /// <summary>
    /// The node has no flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// The node is not missing.
    /// </summary>
    IsNotMissing = 1,

    /// <summary>
    /// The node contains diagnostics, either directly or one of its descendants.
    /// </summary>
    ContainsDiagnostics = 2,

    /// <summary>
    /// The node contains annotations, either directly or one of its descendants.
    /// </summary>
    ContainsAnnotations = 4,

    /// <summary>
    /// The node contains structured trivia, either directly or one of its descendants.
    /// </summary>
    ContainsStructuredTrivia = 8,

    /// <summary>
    /// The node contains skipped text, either directly or one of its descendants.
    /// </summary>
    ContainsSkippedText = 16,

    /// <summary>
    /// The node contains a placeholder, either directly or one of its descendants.
    /// </summary>
    ContainsPlaceholder = 32,
}
