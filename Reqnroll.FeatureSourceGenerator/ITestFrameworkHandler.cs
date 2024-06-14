namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Defines a component that handles the aspects of generating code for a target test framework.
/// </summary>
public interface ITestFrameworkHandler
{
    /// <summary>
    /// Gets the name of the test framework this handler supports.
    /// </summary>
    string FrameworkName { get; }

    /// <summary>
    /// Gets a value indicating whether the handler can generate code for a compilation.
    /// </summary>
    /// <param name="compilation">The compilation to check for compatibilty.</param>
    /// <returns><c>true</c> if the handler can generate for the specified compilation; oterwise <c>false</c>.</returns>
    bool CanGenerateForCompilation(CompilationInformation compilation);

    /// <summary>
    /// Gets a value indicating whether the test framework associated with the handler has been referenced by a compilation.
    /// </summary>
    /// <param name="compilation">The compilation to examine.</param>
    /// <returns><c>true</c> if the associated test framework has been referenced by the compilation; 
    /// otherwise <c>false</c>.</returns>
    bool IsTestFrameworkReferenced(CompilationInformation compilation);

    /// <summary>
    /// Gets a test-fixture generator for a compilation.
    /// </summary>
    /// <param name="compilation">The compilation to get a generator for.</param>
    /// <returns>A test-fixture generator for the specified compilation.</returns>
    ITestFixtureGenerator GetTestFixtureGenerator(CompilationInformation compilation);
}
