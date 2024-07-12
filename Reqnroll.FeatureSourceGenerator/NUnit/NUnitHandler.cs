using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.CSharp.NUnit;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

/// <summary>
/// The handler for NUnit.
/// </summary>
public class NUnitHandler : ITestFrameworkHandler
{
    public string TestFrameworkName => "NUnit";

    public ITestFixtureGenerator<TCompilationInformation>? GetTestFixtureGenerator<TCompilationInformation>() 
        where TCompilationInformation : CompilationInformation
    {
        if (typeof(TCompilationInformation).IsAssignableFrom(typeof(CSharpCompilationInformation)))
        {
            return (ITestFixtureGenerator<TCompilationInformation>)new NUnitCSharpTestFixtureGenerator(this);
        }

        return null;
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return compilationInformation.ReferencedAssemblies.Any(assembly => assembly.Name == "nunit.framework");
    }
}
