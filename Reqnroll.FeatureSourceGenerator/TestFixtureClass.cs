using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Represents a Reqnroll text fixture class.
/// </summary>
public abstract class TestFixtureClass : IEquatable<TestFixtureClass?>
{
    /// <summary>
    /// Initializes a new instance of the test fixture class.
    /// </summary>
    /// <param name="identifier">The namespace and name of the class.</param>
    /// <param name="hintName">The hint name used to identify the test fixture within the compilation. This is usually
    /// a virtual path and virtual filename that makes sense within the context of a project. The value must be unique
    /// within the compilation.</param>
    /// <param name="attributes">The attributes which are applied to the feature.</param>
    public TestFixtureClass(
        NamedTypeIdentifier identifier,
        string hintName,
        ImmutableArray<AttributeDescriptor> attributes = default)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

        if (string.IsNullOrEmpty(hintName))
        {
            throw new ArgumentException("Value cannot be null or an empty string.", nameof(hintName));
        }

        HintName = hintName;

        Attributes = attributes.IsDefault ? ImmutableArray<AttributeDescriptor>.Empty : attributes;
    }

    /// <summary>
    /// Gets the identifier of the class.
    /// </summary>
    public NamedTypeIdentifier Identifier { get; }

    /// <summary>
    /// Gets the attributes which are applied to the fixture.
    /// </summary>
    public ImmutableArray<AttributeDescriptor> Attributes { get; }

    /// <summary>
    /// Gets the hint name associated with the test fixture.
    /// </summary>
    public string HintName { get; }

    /// <summary>
    /// Gets the test methods which make up the fixture.
    /// </summary>
    public abstract IEnumerable<TestMethod> GetMethods();

    public override bool Equals(object obj) => Equals(obj as TestFixtureClass);

    public bool Equals(TestFixtureClass? other)
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
            HintName.Equals(other.HintName, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 83155477;

            hash *= 87057149 + Identifier.GetHashCode();
            hash *= 87057149 + Attributes.GetSetHashCode();
            hash *= 87057149 + StringComparer.Ordinal.GetHashCode(HintName);

            return hash;
        }
    }

    /// <summary>
    /// Renders the configured test fixture to source text.
    /// </summary>
    /// <returns>A <see cref="SourceText"/> representing the rendered test fixture.</returns>
    public abstract SourceText Render();
}
