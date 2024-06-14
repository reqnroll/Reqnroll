namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Defines a component which generates test-fixture classes.
/// </summary>
public interface ITestFixtureGenerator
{
    /// <summary>
    /// Generates a test method for a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to create a test method for.</param>
    /// <param name="cancellationToken">A token used to signal when generation should be canceled.</param>
    /// <returns>A <see cref="TestMethod"/> representing the generated method.</returns>
    TestMethod GenerateTestMethod(
        ScenarioInformation scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a test fixture class for a feature, incorporating a set of generated methods.
    /// </summary>
    /// <param name="feature">The feature to generate the test-fixture class for.</param>
    /// <param name="methods">The collection of methods to incorporate into the fixture.</param>
    /// <param name="cancellationToken">A token used to signal when generation should be canceled.</param>
    /// <returns>A <see cref="TestFixtureClass"/> represented the generated test-fixture class.</returns>
    TestFixtureClass GenerateTestFixture(
        FeatureInformation feature,
        IEnumerable<TestMethod> methods,
        CancellationToken cancellationToken = default);
}
