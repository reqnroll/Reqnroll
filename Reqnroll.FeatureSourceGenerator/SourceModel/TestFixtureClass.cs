﻿using Reqnroll.FeatureSourceGenerator.CSharp;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

/// <summary>
/// Represents a Reqnroll text fixture class.
/// </summary>
public abstract class TestFixtureClass : IHasAttributes
{
    /// <summary>
    /// Initializes a new instance of the test fixture class.
    /// </summary>
    /// <param name="identifier">The namespace and name of the class.</param>
    /// <param name="hintName">The hint name used to identify the test fixture within the compilation. This is usually
    /// a virtual path and virtual filename that makes sense within the context of a project. The value must be unique
    /// within the compilation.</param>
    /// <param name="featureInformation">The feature information that will be included in the test fixture.</param>
    /// <param name="attributes">The attributes which are applied to the class.</param>
    protected TestFixtureClass(
        QualifiedTypeIdentifier identifier,
        string hintName,
        FeatureInformation featureInformation,
        ImmutableArray<AttributeDescriptor> attributes = default)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

        if (string.IsNullOrEmpty(hintName))
        {
            throw new ArgumentException("Value cannot be null or an empty string.", nameof(hintName));
        }

        HintName = hintName;
        FeatureInformation = featureInformation;

        Attributes = attributes.IsDefault ? ImmutableArray<AttributeDescriptor>.Empty : attributes;
    }

    protected TestFixtureClass(TestFixtureDescriptor descriptor) : 
        this(
            descriptor.Identifier ?? throw new ArgumentException($"{nameof(descriptor.Identifier)} cannot be null.", nameof(descriptor)),
            descriptor.HintName ?? throw new ArgumentException($"{nameof(descriptor.HintName)} cannot be null.", nameof(descriptor)),
            descriptor.Feature ?? throw new ArgumentException($"{nameof(descriptor.Feature)} cannot be null.", nameof(descriptor)),
            descriptor.Attributes)
    {
    }

    /// <summary>
    /// Gets the identifier of the class.
    /// </summary>
    public QualifiedTypeIdentifier Identifier { get; }

    /// <summary>
    /// Gets the interfaces which are implemented by the class.
    /// </summary>
    public virtual ImmutableArray<TypeIdentifier> Interfaces => ImmutableArray<TypeIdentifier>.Empty;

    /// <summary>
    /// Gets the attributes which are applied to the class.
    /// </summary>
    public ImmutableArray<AttributeDescriptor> Attributes { get; }

    /// <summary>
    /// Gets the hint name associated with the test class.
    /// </summary>
    public string HintName { get; }

    /// <summary>
    /// Gets the feature information that will be included in the test fixture.
    /// </summary>
    public FeatureInformation FeatureInformation { get; }

    /// <summary>
    /// Gets the test methods which make up the fixture.
    /// </summary>
    public abstract IEnumerable<TestMethod> GetMethods();

    public override bool Equals(object obj) => Equals(obj as TestFixtureClass);

    protected bool Equals(TestFixtureClass? other)
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

        return Identifier.Equals(other.Identifier) &&
            (Attributes.Equals(other.Attributes) || Attributes.SetEquals(other.Attributes)) &&
            HintName.Equals(other.HintName, StringComparison.Ordinal) &&
            FeatureInformation.Equals(other.FeatureInformation);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 83155477;

            hash *= 87057149 + Identifier.GetHashCode();
            hash *= 87057149 + Attributes.GetSetHashCode();
            hash *= 87057149 + StringComparer.Ordinal.GetHashCode(HintName);
            hash *= 87057149 + FeatureInformation.GetHashCode();

            return hash;
        }
    }

    /// <summary>
    /// Renders the configured test fixture to source text.
    /// </summary>
    /// <param name="cancellationToken">A token used to signal when rendering should be canceled.</param>
    /// <returns>A <see cref="SourceText"/> representing the rendered test fixture.</returns>
    public abstract SourceText Render(CancellationToken cancellationToken = default);
}
