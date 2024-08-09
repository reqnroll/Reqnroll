namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public record Step(StepType StepType, string Keyword, string Text, FileLinePositionSpan Position);
