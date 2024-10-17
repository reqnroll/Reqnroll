using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class FeatureInformation(
    string name,
    string? description,
    string language,
    ImmutableArray<string> tags = default,
    string? filePath = default) : IEquatable<FeatureInformation?>
{
    public string Name { get; } =
        string.IsNullOrEmpty(name) ?
        throw new ArgumentException("Value cannot be null or an empty string.", nameof(name)) :
        name;

    public string? Description { get; } = description;

    public string Language { get; } =
        string.IsNullOrEmpty(language) ?
        throw new ArgumentException("Value cannot be null or an empty string.", nameof(language)) :
        language;

    public ImmutableArray<string> Tags { get; } = tags.IsDefault ? ImmutableArray<string>.Empty : tags;

    public string? FilePath { get; } = filePath;

    public override bool Equals(object obj) => Equals(obj as FeatureInformation);

    public bool Equals(FeatureInformation? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name.Equals(other.Name) &&
            Language.Equals(other.Language) &&
            Tags.SetEquals(other.Tags) &&
            string.Equals(FilePath, other.FilePath, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 97533941;

            hash *= 37820603 + Name.GetHashCode();
            hash *= 37820603 + Language.GetHashCode();
            hash *= 37820603 + Tags.GetSetHashCode();
            hash *= 37820603 + FilePath == null ? 0 : StringComparer.Ordinal.GetHashCode(FilePath);

            return hash;
        }
    }
}
