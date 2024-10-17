using System.Collections;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class ScenarioExampleSet : IEnumerable<IEnumerable<(string Name, string Value)>>, IEquatable<ScenarioExampleSet?>
{
    public ScenarioExampleSet(
        ImmutableArray<string> headings,
        ImmutableArray<ImmutableArray<string>> values,
        ImmutableArray<string> tags)
    {
        foreach (var set in values)
        {
            if (set.Length != headings.Length)
            {
                throw new ArgumentException(
                    "Values must contain sets with the same number of values as the headings.",
                    nameof(values));
            }
        }

        Headings = headings;
        Values = values;
        Tags = tags;
    }

    public ImmutableArray<string> Headings { get; }

    public ImmutableArray<ImmutableArray<string>> Values { get; }

    public ImmutableArray<string> Tags { get; }

    public override bool Equals(object? obj) => Equals(obj as ScenarioExampleSet);

    public bool Equals(ScenarioExampleSet? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return (Headings.Equals(other.Headings) || Headings.SequenceEqual(other.Headings)) &&
            (Values.Equals(other.Values) || Values.SequenceEqual(other.Values, ImmutableArrayEqualityComparer<string>.Default)) &&
            (Tags.Equals(other.Tags) || Tags.SequenceEqual(other.Tags));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 43961407;

            hash *= 32360441 + Headings.GetSequenceHashCode();
            hash *= 32360441 + Values.GetSequenceHashCode(ImmutableArrayEqualityComparer<string>.Default);
            hash *= 32360441 + Tags.GetSequenceHashCode();

            return hash;
        }
    }

    public IEnumerator<IEnumerable<(string Name, string Value)>> GetEnumerator()
    {
        foreach (var set in Values)
        {
            yield return GetAsRow(set);
        }
    }

    private IEnumerable<(string Name, string Value)> GetAsRow(ImmutableArray<string> set)
    {
        for (var i = 0; i < Headings.Length; i++)
        {
            yield return (Name: Headings[i], Value: set[i]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
