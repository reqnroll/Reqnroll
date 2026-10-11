using System.Collections.Generic;

namespace Reqnroll.Bindings;

/// <summary>
/// A provider of step definition descriptors that can be used to obtain the step definitions
/// available to be called.
/// </summary>
public interface IStepDefinitionDescriptorsProvider
{
    /// <summary>
    /// Gets the step definitions known to the provider.
    /// </summary>
    /// <returns>A collection of step definitions known to the provider.</returns>
    public IReadOnlyCollection<StepDefinitionDescriptor> GetStepDefinitions();
}
