namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public record ScenarioStep(StepType StepType, string Keyword, string Text, FileLinePositionSpan Position);
