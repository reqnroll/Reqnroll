using System;
using System.Collections.Immutable;

namespace Reqnroll.Bindings;

/// <summary>
/// Provides a definition of a step which can be invoked as part of a scenario.
/// </summary>
public class StepDefinitionDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StepDefinitionDescriptor"/> class.
    /// </summary>
    /// <param name="displayName">A friendly name for this step.</param>
    /// <param name="matchedTypes">The step types matched by this definition.</param>
    /// <param name="textPattern">The text pattern that must be matched by step text.</param>
    /// <param name="parameters">The parameters of the step definition.</param>
    /// <exception cref="ArgumentException">
    /// <para><paramref name="matchedTypes"/> is a default value or empty.</para>
    /// </exception>
    public StepDefinitionDescriptor(
        string displayName,
        ImmutableArray<StepDefinitionType> matchedTypes,
        StepTextPattern textPattern,
        ImmutableArray<StepParameterDescriptor> parameters = default)
    {
        if (matchedTypes.IsDefaultOrEmpty)
        {
            throw new ArgumentException("Array cannot be default or empty.", nameof(matchedTypes));
        }

        DisplayName = displayName;
        MatchedTypes = matchedTypes;
        TextPattern = textPattern;
        Parameters = parameters.IsDefault ? [] : parameters;
    }

    /// <summary>
    /// Gets the friendly name for this step.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets step types matched by this definition, indicating the class of keyword it will bind to.
    /// </summary>
    public ImmutableArray<StepDefinitionType> MatchedTypes { get; }

    /// <summary>
    /// Gets the text pattern that must be matched by step text.
    /// </summary>
    public StepTextPattern TextPattern { get; }

    /// <summary>
    /// Gets the parameters of the step definition.
    /// </summary>
    public ImmutableArray<StepParameterDescriptor> Parameters { get; }

    /// <summary>
    /// Gets a string representation of the step definition.
    /// </summary>
    /// <returns>A string representing the step definition.</returns>
    public override string ToString() => $"Step: {DisplayName}";
}
