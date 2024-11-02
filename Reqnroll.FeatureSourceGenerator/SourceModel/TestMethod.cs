﻿using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

/// <summary>
/// Represents a test method that executes a scenario.
/// </summary>
public abstract class TestMethod : IEquatable<TestMethod?>, IHasAttributes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestMethod"/> class.
    /// </summary>
    /// <param name="identifier">The identifier of the method.</param>
    /// <param name="scenario">The scenario associated with the method.</param>
    /// <param name="stepInvocations">The step invocations performed by the method.</param>
    /// <param name="attributes">The attributes applied to the method.</param>
    /// <param name="parameters">The parameters defined by the method.</param>
    /// <param name="scenarioParameters">A map of scenario parameters to the identifiers that will be used to supply
    /// a value for each parameter.</param>
    /// <exception cref="ArgumentException">
    /// <para><paramref name="identifier"/> is an empty identifier string.</para>
    /// </exception>
    protected TestMethod(
        IdentifierString identifier,
        ScenarioInformation scenario,
        ImmutableArray<StepInvocation> stepInvocations,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<ParameterDescriptor> parameters = default,
        ImmutableArray<KeyValuePair<string, IdentifierString>> scenarioParameters = default)
    {
        if (identifier.IsEmpty)
        {
            throw new ArgumentException("Value cannot be an empty identifier.", nameof(identifier));
        }

        Identifier = identifier;
        Scenario = scenario;
        StepInvocations = stepInvocations.IsDefault ? ImmutableArray<StepInvocation>.Empty : stepInvocations;
        Attributes = attributes.IsDefault ? ImmutableArray<AttributeDescriptor>.Empty : attributes;
        Parameters = parameters.IsDefault ? ImmutableArray<ParameterDescriptor>.Empty : parameters;
        ParametersOfScenario = scenarioParameters.IsDefault ? ImmutableArray<KeyValuePair<string, IdentifierString>>.Empty : scenarioParameters;
    }

    /// <summary>
    /// Initializes a new inastance of the <see cref="TestMethod"/> class from a descriptor.
    /// </summary>
    /// <param name="descriptor">A description of the method to create.</param>
    /// <exception cref="ArgumentException">
    /// <para>The <paramref name="descriptor"/> is incomplete.</para>
    /// </exception>
    protected TestMethod(TestMethodDescriptor descriptor)
        : this(
              descriptor.Identifier,
              descriptor.Scenario ?? throw new ArgumentException(
                  $"{nameof(descriptor.Scenario)} property cannot be null.",
                  nameof(descriptor)),
              descriptor.StepInvocations,
              descriptor.Attributes,
              descriptor.Parameters,
              descriptor.ScenarioParameters)
    {
    }

    /// <summary>
    /// Gets the identifier of the test method.
    /// </summary>
    public IdentifierString Identifier { get; }

    /// <summary>
    /// Gets the step invocations the method performs.
    /// </summary>
    public ImmutableArray<StepInvocation> StepInvocations { get; }

    /// <summary>
    /// Gets the attributes applied to the method.
    /// </summary>
    public ImmutableArray<AttributeDescriptor> Attributes { get; }

    /// <summary>
    /// Gets the parameters which are defined by this method.
    /// </summary>
    public ImmutableArray<ParameterDescriptor> Parameters { get; }

    /// <summary>
    /// Gets the arguments of the scenario, as they map to parameters.
    /// </summary>
    public ImmutableArray<KeyValuePair<string, IdentifierString>> ParametersOfScenario { get; }

    /// <summary>
    /// Gets information about the scenario associated with the test method.
    /// </summary>
    public ScenarioInformation Scenario { get; }

    public override bool Equals(object obj) => Equals(obj as TestMethod);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 86434151;

            hash *= 83155477 + GetType().GetHashCode();
            
            hash *= 83155477 + Identifier.GetHashCode();
            hash *= 83155477 + Scenario.GetHashCode();
            hash *= 83155477 + StepInvocations.GetSequenceHashCode();
            hash *= 83155477 + Attributes.GetSetHashCode();
            hash *= 83155477 + Parameters.GetSequenceHashCode();
            hash *= 83155477 + ParametersOfScenario.GetSequenceHashCode();

            return hash;
        }
    }

    public virtual bool Equals(TestMethod? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return
            Identifier.Equals(other.Identifier) &&
            Scenario.Equals(other.Scenario) &&
            (StepInvocations.Equals(other.StepInvocations) || StepInvocations.SequenceEqual(other.StepInvocations)) &&
            (Attributes.Equals(other.Attributes) || Attributes.SetEquals(other.Attributes)) &&
            (Parameters.Equals(other.Parameters) || Parameters.SequenceEqual(other.Parameters)) &&
            (ParametersOfScenario.Equals(other.ParametersOfScenario) || ParametersOfScenario.SequenceEqual(other.ParametersOfScenario));
    }
}
