namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Defines a component that handles the support for a test framework.
/// </summary>
public interface ITestFrameworkHandler
{
    /// <summary>
    /// Gets the name of the test framework associated with the handler.
    /// </summary>
    string TestFrameworkName { get; }

    /// <summary>
    /// Gets a value indicating whether the test framework associated with the handler is referenced by a compilation.
    /// </summary>
    /// <param name="compilation">The compilation to examine.</param>
    /// <returns><c>true</c> if the test framework is referenced by the compilation; 
    /// otherwise <c>false</c>.</returns>
    bool IsTestFrameworkReferenced(CompilationInformation compilation);

    /// <summary>
    /// Gets a test-fixture generator for the test framework.
    /// </summary>
    /// <typeparam name="TCompilationInformation">The type of compilation to obtain a generator for.</typeparam>
    /// <returns>A test-fixture generator if one can be produced for the compilation type; otherwise <c>null</c>.</returns>
    ITestFixtureGenerator<TCompilationInformation>? GetTestFixtureGenerator<TCompilationInformation>()
        where TCompilationInformation : CompilationInformation;
}
