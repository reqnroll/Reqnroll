using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Defines a component which generates test-fixture classes.
/// </summary>
/// <typeparam name="TCompilationInformation">The type of compilation information handled by the generator.</typeparam>
public interface ITestFixtureGenerator<TCompilationInformation> 
    where TCompilationInformation : CompilationInformation
{
    /// <summary>
    /// Gets the test framework handler this generator is associated with.
    /// </summary>
    ITestFrameworkHandler TestFrameworkHandler { get; }

    /// <summary>
    /// Generates a test method for a scenario.
    /// </summary>
    /// <param name="context">The method-generation context.</param>
    /// <param name="cancellationToken">A token used to signal when generation should be canceled.</param>
    /// <returns>A <typeparamref name="TTestMethod"/> representing the generated method.</returns>
    TestMethod GenerateTestMethod(
        TestMethodGenerationContext<TCompilationInformation> context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a test fixture class for a feature, incorporating a set of generated methods.
    /// </summary>
    /// <param name="context">The feature-generation context.</param>
    /// <param name="methods">The collection of methods to incorporate into the fixture.</param>
    /// <param name="cancellationToken">A token used to signal when generation should be canceled.</param>
    /// <returns>A <see cref="TestFixtureClass"/> represented the generated test-fixture class.</returns>
    TestFixtureClass GenerateTestFixtureClass(
        TestFixtureGenerationContext<TCompilationInformation> context,
        IEnumerable<TestMethod> methods,
        CancellationToken cancellationToken = default);
}
