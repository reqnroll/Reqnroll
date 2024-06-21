using Reqnroll.FeatureSourceGenerator.CSharp;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

/// <summary>
/// The handler for MSTest.
/// </summary>
public class MSTestHandler : ITestFrameworkHandler
{
    public string FrameworkName => "MSTest";

    public bool CanGenerateForCompilation(CompilationInformation compilation) => 
        compilation is CSharpCompilationInformation;

    public bool IsTestFrameworkReferenced(CompilationInformation compilation)
    {
        return compilation.ReferencedAssemblies
            .Any(assembly => assembly.Name == "Microsoft.VisualStudio.TestPlatform.TestFramework");
    }

    public ITestFixtureGenerator GetTestFixtureGenerator(CompilationInformation compilation)
    {
        return compilation switch
        {
            CSharpCompilationInformation => new MSTestCSharpTestFixtureGenerator(),
            _ => throw new NotSupportedException(),
        };
    }
}
