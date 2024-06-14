
namespace Reqnroll.FeatureSourceGenerator.XUnit;

internal class XUnitCSharpTestFixtureGenerator : ITestFixtureGenerator
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
