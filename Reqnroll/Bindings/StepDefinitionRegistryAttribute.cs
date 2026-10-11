using System;

namespace Reqnroll.Bindings;

/// <summary>
/// Specifies the type that serves as the registry for step definitions in an assembly.
/// </summary>
/// <remarks>
/// <para>This attribute is applied at the assembly level to indicate the type that contains the step
/// definitions for the assembly. The type is expected to implement <see cref="IStepDefinitionDescriptorsProvider"/>
/// to be used by Reqnroll step location infrastructure.</para>
/// </remarks>
/// <param name="registryType">The type of the step definition registry.</param>
[AttributeUsage(AttributeTargets.Assembly)]
public class StepDefinitionRegistryAttribute(Type registryType) : Attribute
{
    /// <summary>
    /// Gets the type which is the step definition registry for the assembly.
    /// </summary>
    public Type RegistryType { get; } = registryType;
}
