using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

public record ScenarioInformation(
    FeatureInformation Feature,
    string Name,
    ImmutableArray<string> ImmutableArray,
    ImmutableArray<ScenarioStep> ScenarioSteps,
    ImmutableArray<ScenarioExampleSet> ScenarioExampleSets,
    string? RuleName,
    ImmutableArray<string> RuleTags);
