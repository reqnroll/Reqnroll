using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.Bindings;

#nullable enable

/// <summary>
/// A provider of steps which can be invoked as part of a scenario.
/// </summary>
public interface IStepDefinitionProvider
{
    /// <summary>
    /// Gets the step definitions known to the provider.
    /// </summary>
    /// <returns>A collection of step definitions known to the provider.</returns>
    public IReadOnlyCollection<IStepDefinition> GetStepDefinitions();
}

/// <summary>
/// Provides a definition of a step which can be invoked as part of a scenario.
/// </summary>
public interface IStepDefinition
{
    /// <summary>
    /// Gets the text pattern which binds this step definition to scenario step texts.
    /// </summary>
    IStepTextPattern TextPattern { get; }

    /// <summary>
    /// Gets the parameters of the step definition.
    /// </summary>
    IReadOnlyList<IStepParameter> Parameters { get; }

    /// <summary>
    /// Invokes the step.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the step.</param>
    /// <returns>A <see cref="ValueTask"/> representing the execution of the step.</returns>
    ValueTask InvokeAsync(object[] arguments);
}

/// <summary>
/// Defines a parameter that is expressed by a step.
/// </summary>
public interface IStepParameter
{
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    Type Type { get; }
}

/// <summary>
/// A pattern used to match the text of a step with a step definition.
/// </summary>
public interface IStepTextPattern
{
    /// <summary>
    /// Attempts to match this pattern with given text.
    /// </summary>
    /// <param name="text">Step text to match to this pattern.</param>
    /// <param name="values">If the step text matches the pattern, the array contains values extracted
    /// from the step text by the pattern, in the order they are defined in the pattern.</param>
    /// <returns><c>true</c> if <paramref name="text"/> is text which matches the pattern; otherwise <c>false</c>.</returns>
    bool TryMatch(string text, out object?[]? values);
}

/// <summary>
/// Provides the base class for compile-time generated step definition registries.
/// </summary>
public abstract class StepDefinitionRegistry : IStepDefinitionProvider
{
    private readonly List<IStepDefinition> _stepDefinitions = [];

    /// <summary>
    /// Adds a definition to the registry.
    /// </summary>
    /// <param name="step">A step to add to the registry.</param>
    /// <remarks>
    /// <para><see cref="AddStepDefinition(IStepDefinition)"/> is intended to be called from the constructor of a
    /// step registry which inherits from <see cref="StepDefinitionRegistry"/> to build a registry of all step definitions
    /// available in an assembly.</para>
    /// </remarks>
    protected void AddStepDefinition(IStepDefinition step) => _stepDefinitions.Add(step);

    /// <summary>
    /// Gets all step definitions which have been added to the registry.
    /// </summary>
    /// <returns>A collection containing all definitions which have been added to the registry.</returns>
    public IReadOnlyCollection<IStepDefinition> GetStepDefinitions() => _stepDefinitions;
}
