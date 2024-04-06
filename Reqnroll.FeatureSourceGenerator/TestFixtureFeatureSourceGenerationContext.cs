namespace Reqnroll.FeatureSourceGenerator;

public record TestFixtureFeatureSourceGenerationContext(
        GherkinSyntaxTree SyntaxTree,
        FeatureSourceGenerationOptions Options,
        ITestFrameworkHandler TestFramework) : 
    FeatureSourceGenerationContext(SyntaxTree, Options);
