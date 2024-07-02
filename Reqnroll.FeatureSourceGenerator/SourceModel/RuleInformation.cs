using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class RuleInformation(string name, ImmutableArray<string> tags) : IEquatable<RuleInformation?>
{
    public string Name { get; } = string.IsNullOrEmpty(name) ? 
        throw new ArgumentException("Value cannot be null or an empty string", nameof(name)) : 
        name;

    public ImmutableArray<string> Tags { get; } = tags.IsDefault ? ImmutableArray<string>.Empty : tags;

    public bool Equals(RuleInformation? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name.Equals(other.Name) && Tags.SetEqual(other.Tags);
    }

    public static bool Equals(RuleInformation? first, RuleInformation? second)
    {
        if (first is null)
        {
            return false;
        }

        return first.Equals(second);
    }

    public override bool Equals(object obj) => Equals(obj as RuleInformation);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 14353993;

            hash *= 50733161 + Name.GetHashCode();
            hash *= 50733161 + Tags.GetSetHashCode();

            return hash;
        }
    }

    public override string ToString() => $"Name={Name}";
}
