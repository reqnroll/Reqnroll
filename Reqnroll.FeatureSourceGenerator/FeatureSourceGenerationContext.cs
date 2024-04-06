namespace Reqnroll.FeatureSourceGenerator;

public record FeatureSourceGenerationContext(
    GherkinSyntaxTree SyntaxTree,
    FeatureSourceGenerationOptions Options);
