using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Reqnroll.Bindings;

#nullable enable

/// <summary>
/// An attribute which identifies which type in an assembly should be considered its step registry.
/// </summary>
/// <param name="registryType">The type which is the step registry for the assembly.</param>
[AttributeUsage(AttributeTargets.Assembly)]
public class ReqnrollStepRegistryAttribute(Type registryType) : Attribute
{
    /// <summary>
    /// Gets the registry type.
    /// </summary>
    public Type RegistryType { get; } = registryType;
}

/// <summary>
/// Provides extensions for working with step definition assemblies.
/// </summary>
public static class StepAssemblyExtensions
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
    public static bool TryGetStepRegistry(
        this Assembly assembly,
        out IStepDefinitionDescriptorsProvider? stepRegistry,
        out Exception? error)
    {
        var registryAttribute = assembly.GetCustomAttribute<StepRegistryAttribute>();
        stepRegistry = null;

        if (registryAttribute == null)
        {
            error = new InvalidOperationException(
                $"Assembly '{assembly.FullName}' does not have a '{nameof(StepRegistryAttribute)}' defined.");
            return false;
        }

        if (registryAttribute.RegistryType == null)
        {
            error = new InvalidOperationException(
                $"Assembly '{assembly.FullName}' has a '{nameof(StepRegistryAttribute)}' " +
                "defined but no registry type is specified.");
            return false;
        }

        if (typeof(IStepDefinitionDescriptorsProvider).IsAssignableFrom(registryAttribute.RegistryType))
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
        if (assembly.TryGetStepRegistry(out var registry, out var error))
        {
            return registry!;
        }

        throw error!;
    }
}

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

/// <summary>
/// Provides a definition of a step which can be invoked as part of a scenario.
/// </summary>
public class StepDefinitionDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StepDefinitionDescriptor"/> class.
    /// </summary>
    /// <param name="displayName">A friendly name for this step.</param>
    /// <param name="type">The type of the step definition.</param>
    /// <param name="textPattern">The text pattern that must be matched by step text.</param>
    /// <param name="parameters">The parameters of the step definition.</param>
    /// <exception cref="ArgumentException">
    /// <para><paramref name="textPattern"/> is an empty text pattern.</para>
    /// </exception>
    public StepDefinitionDescriptor(
        string displayName,
        StepDefinitionType type,
        StepTextPattern textPattern,
        ImmutableArray<StepParameterDescriptor> parameters = default)
    {
        if (textPattern.IsEmpty)
        {
            throw new ArgumentException("Value cannot be an empty text pattern.", nameof(textPattern));
        }

        DisplayName = displayName;
        Type = type;
        TextPattern = textPattern;
        Parameters = parameters.IsDefault ? [] : parameters;
    }

    /// <summary>
    /// Gets the friendly name for this step.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the type of the step defintion, indicating the class of keyword it will bind to.
    /// </summary>
    public StepDefinitionType Type { get; }

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

/// <summary>
/// Describes a parameter to a step.
/// </summary>
public class StepParameterDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StepParameterDescriptor"/> class.
    /// </summary>
    /// <param name="name">The name of the paramater.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    public StepParameterDescriptor(string name, Type parameterType)
    {
        Name = name;
        ParameterType = parameterType;
    }

    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    public Type ParameterType { get; }
}

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

/// <summary>
/// Specifies the language of the syntax that is used by a step text pattern to define its parameters.
/// </summary>
public enum StepTextPatternSyntaxLanguage
{
    /// <summary>
    /// The step text pattern does not employ a syntax.
    /// </summary>
    None,

    /// <summary>
    /// The pattern is a Cucumber expression.
    /// </summary>
    CucumberExpression,

    /// <summary>
    /// The pattern is a regular expression.
    /// </summary>
    RegularExpression
};

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
