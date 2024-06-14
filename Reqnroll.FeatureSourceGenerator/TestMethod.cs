using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Represents a test method that executes a scenario.
/// </summary>
public abstract class TestMethod : IEquatable<TestMethod?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestMethod"/> class.
    /// </summary>
    /// <param name="identifier">The identifier of the method.</param>
    /// <param name="attributes">The attributes applied to the method.</param>
    /// <param name="parameters">The parameters defined by the method.</param>
    /// <exception cref="ArgumentException">
    /// <para><paramref name="identifier"/> is an empty identifier string.</para>
    /// </exception>
    public TestMethod(
        IdentifierString identifier,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<ParameterDescriptor> parameters = default)
    {
        if (identifier.IsEmpty)
        {
            throw new ArgumentException("Value cannot be an empty identifier.", nameof(identifier));
        }

        Identifier = identifier;

        Attributes = attributes.IsDefault ? ImmutableArray<AttributeDescriptor>.Empty : attributes;
        Parameters = parameters.IsDefault ? ImmutableArray<ParameterDescriptor>.Empty : parameters;
    }

    /// <summary>
    /// Gets the identifier of the test method.
    /// </summary>
    public IdentifierString Identifier { get; }

    /// <summary>
    /// Gets the attributes applied to the method.
    /// </summary>
    public ImmutableArray<AttributeDescriptor> Attributes { get; }

    /// <summary>
    /// Gets the parameters which are defined by this method.
    /// </summary>
    public ImmutableArray<ParameterDescriptor> Parameters { get; }

    public override bool Equals(object obj) => Equals(obj as TestMethod);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 86434151;

            hash *= 83155477 + Identifier.GetHashCode();
            hash *= 83155477 + Attributes.GetSetHashCode();
            hash *= 83155477 + Parameters.GetSequenceHashCode();

            return hash;
        }
    }

    public bool Equals(TestMethod? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Identifier.Equals(other.Identifier) &&
            (Attributes.Equals(other.Attributes) || Attributes.SetEquals(other.Attributes)) &&
            (Parameters.Equals(other.Parameters) || Parameters.SequenceEqual(other.Parameters));
    }
}
