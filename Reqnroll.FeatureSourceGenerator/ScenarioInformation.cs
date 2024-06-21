using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

public class ScenarioInformation(
    FeatureInformation feature,
    string name,
    ImmutableArray<string> tags,
    ImmutableArray<ScenarioStep> steps,
    ImmutableArray<ScenarioExampleSet> examples = default,
    RuleInformation? rule = null) : IEquatable<ScenarioInformation?>
{
    public FeatureInformation Feature { get; } = feature;

    public string Name { get; } = string.IsNullOrEmpty(name) ?
        throw new ArgumentException("Value cannot be null or an empty string", nameof(name)) :
        name;

    public ImmutableArray<string> Tags { get; } = tags.IsDefault ? ImmutableArray<string>.Empty : tags;

    public ImmutableArray<ScenarioStep> Steps { get; } = steps.IsDefault ? ImmutableArray<ScenarioStep>.Empty : steps;

    public ImmutableArray<ScenarioExampleSet> Examples { get; } = examples.IsDefault ? ImmutableArray<ScenarioExampleSet>.Empty : examples;

    public RuleInformation? Rule { get; } = rule;

    public override bool Equals(object obj) => Equals(obj as ScenarioInformation);

    public bool Equals(ScenarioInformation? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Feature.Equals(other.Feature) &&
            Name.Equals(other.Name) &&
            Tags.SetEqual(other.Tags) &&
            Steps.SequenceEqual(other.Steps) &&
            Examples.SequenceEqual(other.Examples) &&
            RuleInformation.Equals(Rule, other.Rule);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString() => $"Name={Name}";
}

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
