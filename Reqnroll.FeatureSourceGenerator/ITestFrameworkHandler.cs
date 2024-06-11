namespace Reqnroll.FeatureSourceGenerator;

public interface ITestFrameworkHandler
{
    string FrameworkName { get; }

    bool CanGenerateForCompilation(CompilationInformation compilationInformation);

    TestFixtureMethod GenerateTestFixtureMethod(ScenarioInformation scenarioInformation, CancellationToken cancellationToken);

    TestFixture GenerateTestFixture(
        FeatureInformation featureInformation,
        IEnumerable<TestFixtureMethod> methods,
        CancellationToken cancellationToken);

    bool IsTestFrameworkReferenced(CompilationInformation compilationInformation);
}
