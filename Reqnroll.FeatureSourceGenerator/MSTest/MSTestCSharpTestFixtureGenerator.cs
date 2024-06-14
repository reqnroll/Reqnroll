
namespace Reqnroll.FeatureSourceGenerator.MSTest;

/// <summary>
/// Performs generation of MSTest test fixtures in the C# language.
/// </summary>
internal class MSTestCSharpTestFixtureGenerator : ITestFixtureGenerator
{
    public TestFixtureClass GenerateTestFixture(
        FeatureInformation feature,
        IEnumerable<TestMethod> methods,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public TestMethod GenerateTestMethod(
        ScenarioInformation scenario,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
