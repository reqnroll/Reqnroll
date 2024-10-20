using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator;

public class TestMethodGenerationContext<TCompilationInformation>(
    ScenarioInformation scenarioInformation,
    TestFixtureGenerationContext<TCompilationInformation> testFixtureGenerationContext) : 
    IEquatable<TestMethodGenerationContext<TCompilationInformation>?>
    where TCompilationInformation : CompilationInformation
{
    public TestFixtureGenerationContext<TCompilationInformation> TestFixtureGenerationContext { get; } = testFixtureGenerationContext;

    public ScenarioInformation ScenarioInformation { get; } = scenarioInformation;

    public ITestFixtureGenerator<TCompilationInformation> TestFixtureGenerator => TestFixtureGenerationContext.TestFixtureGenerator;

    public FeatureInformation FeatureInformation => TestFixtureGenerationContext.FeatureInformation;

    public override bool Equals(object obj) => Equals(obj as TestMethodGenerationContext<TCompilationInformation>);

    public bool Equals(TestMethodGenerationContext<TCompilationInformation>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return TestFixtureGenerationContext.Equals(other.TestFixtureGenerationContext) &&
            ScenarioInformation.Equals(other.ScenarioInformation);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 14353993;

            hash *= 50733161 + TestFixtureGenerationContext.GetHashCode();
            hash *= 50733161 + ScenarioInformation.GetHashCode();

            return hash;
        }
    }
}
