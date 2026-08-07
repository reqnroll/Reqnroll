using System.Collections.Generic;

namespace Reqnroll.Bindings;

/// <summary>
/// Provides the base class for compile-time generated step definition registries.
/// </summary>
public abstract class StepDefinitionRegistry : IStepDefinitionDescriptorsProvider
{
    private readonly List<StepDefinitionDescriptor> _stepDefinitions = [];

    /// <summary>
    /// Adds a definition to the registry.
    /// </summary>
    /// <param name="stepDefinition">A step definition description to add to the registry.</param>
    /// <remarks>
    /// <para><see cref="Register(StepDefinitionDescriptor)"/> is intended to be called from the constructor of a
    /// step registry which inherits from <see cref="StepDefinitionRegistry"/> to compose the registry.</para>
    /// </remarks>
    protected void Register(StepDefinitionDescriptor stepDefinition) => _stepDefinitions.Add(stepDefinition);

    /// <summary>
    /// Gets all step definitions which have been added to the registry.
    /// </summary>
    /// <returns>A collection containing all definitions which have been added to the registry.</returns>
    public IReadOnlyCollection<StepDefinitionDescriptor> GetStepDefinitions() => _stepDefinitions;
}
