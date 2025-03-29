namespace Reqnroll.Bindings;

/// <summary>
/// A text pattern that is used to bind a step definition to a scenario step.
/// </summary>
public readonly struct StepTextPattern
{
    private StepTextPattern(StepTextPatternSyntaxLanguage patternType, string text)
    {
        SyntaxLanguage = patternType;
        Text = text;
    }

    /// <summary>
    /// Creates a new step text pattern using the Cucumber expression syntax.
    /// </summary>
    /// <param name="text">The pattern text.</param>
    /// <returns>A <see cref="StepTextPattern"/> of type <see cref="StepTextPatternSyntaxLanguage.CucumberExpression"/>
    /// with the specified text pattern.</returns>
    public static StepTextPattern CucumberExpression(string text) => new(StepTextPatternSyntaxLanguage.CucumberExpression, text);

    /// <summary>
    /// Creates a new step text pattern using regular expression syntax.
    /// </summary>
    /// <param name="text">The pattern text.</param>
    /// <returns>A <see cref="StepTextPattern"/> of type <see cref="StepTextPatternSyntaxLanguage.RegularExpression"/>
    /// with the specified text pattern.</returns>
    public static StepTextPattern RegularExpression(string text) => new(StepTextPatternSyntaxLanguage.RegularExpression, text);

    /// <summary>
    /// Gets a step text pattern representing no text pattern.
    /// </summary>
    public static StepTextPattern None { get; } = default;

    /// <summary>
    /// Gets the language of the syntax used by the step text pattern to define matchning semantics and parameters.
    /// </summary>
    public StepTextPatternSyntaxLanguage SyntaxLanguage { get; }

    /// <summary>
    /// Gets the text of the pattern.
    /// </summary>
    public string? Text { get; }

    /// <summary>
    /// Gets a value indicating whether this instance represents an empty pattern.
    /// </summary>
    public bool IsEmpty => Text == null;
}
