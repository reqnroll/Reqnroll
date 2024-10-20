using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

public class TestFixtureGenerationContext<TCompilationInformation>(
    FeatureInformation featureInformation,
    ImmutableArray<ScenarioInformation> scenarioInformations,
    string featureHintName,
    NamespaceString testFixtureNamespace,
    TCompilationInformation compilationInformation,
    ITestFixtureGenerator<TCompilationInformation> testFixtureGenerator)
    where TCompilationInformation : CompilationInformation
{
    public FeatureInformation FeatureInformation { get; } = featureInformation;

    public ImmutableArray<ScenarioInformation> ScenarioInformations { get; } =
        scenarioInformations.IsDefault ?
        ImmutableArray<ScenarioInformation>.Empty :
        scenarioInformations;

    public string FeatureHintName { get; } = featureHintName;

    public NamespaceString TestFixtureNamespace { get; } = testFixtureNamespace;

    public TCompilationInformation CompilationInformation { get; } = compilationInformation;

    public ITestFrameworkHandler TestFrameworkHandler => TestFixtureGenerator.TestFrameworkHandler;

    public ITestFixtureGenerator<TCompilationInformation> TestFixtureGenerator { get; } = testFixtureGenerator;

    public bool Equals(TestFixtureGenerationContext<TCompilationInformation>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(other, this))
        {
            return true;
        }

        return FeatureInformation.Equals(other.FeatureInformation) &&
            ScenarioInformations.SequenceEqual(other.ScenarioInformations) &&
            FeatureHintName.Equals(other.FeatureHintName, StringComparison.Ordinal) &&
            TestFixtureNamespace.Equals(other.TestFixtureNamespace) &&
            CompilationInformation.Equals(other.CompilationInformation) &&
            TestFixtureGenerator.Equals(other.TestFixtureGenerator);
    }

    public override bool Equals(object obj) => Equals(obj as TestFixtureGenerationContext<TCompilationInformation>);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 37970701;

            hash *= 99584197 + FeatureInformation.GetHashCode();
            hash *= 99584197 + ScenarioInformations.GetSequenceHashCode();
            hash *= 99584197 + FeatureHintName.GetHashCode();
            hash *= 99584197 + TestFixtureNamespace.GetHashCode();
            hash *= 99584197 + CompilationInformation.GetHashCode();
            hash *= 99584197 + TestFixtureGenerator.GetHashCode();

            return hash;
        }
    }
}

