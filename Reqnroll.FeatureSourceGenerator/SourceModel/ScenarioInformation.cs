using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;
public class ScenarioInformation(
    string name,
    int lineNumber,
    ImmutableArray<string> tags,
    ImmutableArray<ScenarioStep> steps,
    ImmutableArray<ScenarioExampleSet> examples = default,
    RuleInformation? rule = null) : IEquatable<ScenarioInformation?>
{
    public string Name { get; } = string.IsNullOrEmpty(name) ?
        throw new ArgumentException("Value cannot be null or an empty string", nameof(name)) :
        name;

    public int LineNumber { get; } = lineNumber;

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

        return Name.Equals(other.Name) &&
            Tags.SetEqual(other.Tags) &&
            Steps.SequenceEqual(other.Steps) &&
            Examples.SequenceEqual(other.Examples) &&
            RuleInformation.Equals(Rule, other.Rule);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 42261493;

            hash *= 95921717 + Name.GetHashCode();
            hash *= 95921717 + Tags.GetSetHashCode();
            hash *= 95921717 + Steps.GetSequenceHashCode();
            hash *= 95921717 + Examples.GetSequenceHashCode();

            if (Rule != null)
            {
                hash *= 95921717 + Rule.GetHashCode();
            }

            return hash;
        }
    }

    public override string ToString() => $"Name={Name}";
}
