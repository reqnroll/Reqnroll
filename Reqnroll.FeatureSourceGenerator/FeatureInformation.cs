using Reqnroll.FeatureSourceGenerator.Gherkin;

namespace Reqnroll.FeatureSourceGenerator;

public record FeatureInformation(
    GherkinSyntaxTree FeatureSyntax,
    string FeatureHintName,
    string FeatureNamespace,
    CompilationInformation CompilationInformation,
    ITestFrameworkHandler TestFrameworkHandler,
    ITestFixtureGenerator TestFixtureGenerator);
