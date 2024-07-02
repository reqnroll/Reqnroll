using Reqnroll.FeatureSourceGenerator.CSharp;

namespace Reqnroll.FeatureSourceGenerator.XUnit;

/// <summary>
/// The handler for xUnit.
/// </summary>
public class XUnitHandler : ITestFrameworkHandler
{
    public string TestFrameworkName => "xUnit";

    public ITestFixtureGenerator<TCompilationInformation>? GetTestFixtureGenerator<TCompilationInformation>() 
        where TCompilationInformation : CompilationInformation
    {
        if (typeof(TCompilationInformation).IsAssignableFrom(typeof(CSharpCompilationInformation)))
        {
            return (ITestFixtureGenerator<TCompilationInformation>)new XUnitCSharpTestFixtureGenerator(this);
        }

        return null;
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return compilationInformation.ReferencedAssemblies
            .Any(assembly => assembly.Name == "xunit.core");
    }
}
