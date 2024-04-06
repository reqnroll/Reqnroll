using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Represents a summary of a Gherkin feature - the fields which provide an overview of the feature, excluding the examples that make
/// up the feature specification.
/// </summary>
/// <param name="Name">The name of the feature.</param>
/// <param name="Description"></param>
/// <param name="Keyword"></param>
/// <param name="Language"></param>
/// <param name="Tags"></param>
public record FeatureSummary (
    string Name, 
    string Description, 
    string Keyword, 
    string Language, 
    ImmutableArray<string> Tags) : IEquatable<FeatureSummary>
{
    private int _hashCode;

    public virtual bool Equals(FeatureSummary other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetHashCode() != other.GetHashCode())
        {
            return false;
        }

        if (!Name.Equals(other.Name)
            || !Description.Equals(other.Description)
            || !Keyword.Equals(other.Keyword)
            || !Language.Equals(other.Language))
        {
            return false;
        }

        return Tags.SequenceEqual(other.Tags);
    }

    public override int GetHashCode()
    {
        return _hashCode == 0 ? _hashCode = CaclulateHashCode() : _hashCode;
    }

    private int CaclulateHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;

            hash = hash * 23 + Name.GetHashCode();
            hash = hash * 23 + Description.GetHashCode();
            hash = hash * 23 + Keyword.GetHashCode();
            hash = hash * 23 + Language.GetHashCode();

            foreach (var tag in Tags)
            {
                hash = hash * 23 + tag.GetHashCode();
            }

            return hash;
        }
    }
}
