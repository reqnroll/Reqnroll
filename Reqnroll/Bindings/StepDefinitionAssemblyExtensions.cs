using System;
using System.Reflection;

namespace Reqnroll.Bindings;
#nullable enable

/// <summary>
/// Provides extensions for working with step definition assemblies.
/// </summary>
public static class StepDefinitionAssemblyExtensions
{
    /// <summary>
    /// Tries to get the step registry for an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to examine for a step registry.</param>
    /// <param name="stepRegistry">After the method returns, contains the step registry for the assembly, 
    /// if one was found; otherwise <c>null</c>.</param>
    /// <param name="error">After the method returns, if the registry could not be found, contains an exception 
    /// indicating the condition that prevented the registry being located; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if a registry was found; otherwise <c>false</c>.</returns>
    public static bool TryGetStepDefinitionRegistry(
        this Assembly assembly,
        out IStepDefinitionDescriptorsProvider? stepRegistry,
        out Exception? error)
    {
        var registryAttribute = assembly.GetCustomAttribute<StepDefinitionRegistryAttribute>();
        stepRegistry = null;

        if (registryAttribute == null)
        {
            error = new InvalidOperationException(
                $"Assembly '{assembly.FullName}' does not have a '{nameof(StepDefinitionRegistryAttribute)}' defined.");
            return false;
        }

        if (registryAttribute.RegistryType == null)
        {
            error = new InvalidOperationException(
                $"Assembly '{assembly.FullName}' has a '{nameof(StepDefinitionRegistryAttribute)}' " +
                "defined but no registry type is specified.");
            return false;
        }

        if (!typeof(IStepDefinitionDescriptorsProvider).IsAssignableFrom(registryAttribute.RegistryType))
        {
            error = new InvalidOperationException(
                $"Assembly '{assembly.FullName}' speciifes its registry type is " +
                $"'{registryAttribute.RegistryType.FullName}' however the type does not implement " +
                $"'{nameof(IStepDefinitionDescriptorsProvider)}'.");
            return false;
        }

        var instanceProperty = registryAttribute.RegistryType.GetProperty(
            "Instance",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty);

        if (instanceProperty == null || instanceProperty.PropertyType != registryAttribute.RegistryType)
        {
            error = new InvalidOperationException(
                $"Assembly '{assembly.FullName}' speciifes its registry type is " +
                $"'{registryAttribute.RegistryType.FullName}' however the type does not expose the required " +
                $"'Instance' property. The property must be declared public and static and must return the single " +
                $"instance of the registry type.");
            return false;
        }

        stepRegistry = (IStepDefinitionDescriptorsProvider?)instanceProperty.GetGetMethod().Invoke(null, null);

        if (stepRegistry == null)
        {
            error = new InvalidOperationException(
                $"Assembly '{assembly.FullName}' speciifes its registry type is " +
                $"'{registryAttribute.RegistryType.FullName}' however the 'Instance' property did not return " +
                "an instance.");
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Gets the step registry for an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to examine for a step registry.</param>
    /// <returns>The step registry for the assembly.</returns>
    /// <exception cref="InvalidOperationException">
    /// <para>The assembly does have a step registry.</para>
    /// </exception>
    public static IStepDefinitionDescriptorsProvider GetStepRegistry(this Assembly assembly)
    {
        if (assembly.TryGetStepDefinitionRegistry(out var registry, out var error))
        {
            return registry!;
        }

        throw error!;
    }
}
