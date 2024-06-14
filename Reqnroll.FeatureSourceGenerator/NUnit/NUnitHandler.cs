using Reqnroll.FeatureSourceGenerator.CSharp;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

/// <summary>
/// The handler for NUnit.
/// </summary>
public class NUnitHandler : ITestFrameworkHandler
{
    public string FrameworkName => "NUnit";

    public bool CanGenerateForCompilation(CompilationInformation compilationInformation) => 
        compilationInformation is CSharpCompilationInformation;

    public ITestFixtureGenerator GetTestFixtureGenerator(CompilationInformation compilation)
    {
        return compilation switch
        {
            CSharpCompilationInformation => new NUnitCSharpTestFixtureGenerator(),
            _ => throw new NotSupportedException(),
        };
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return compilationInformation.ReferencedAssemblies.Any(assembly => assembly.Name == "nunit.framework");
    }
}
