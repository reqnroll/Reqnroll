using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reqnroll.FeatureSourceGenerator.CSharp;
using Reqnroll.FeatureSourceGenerator.CSharp.MSTest;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

/// <summary>
/// The MSTest framework handler.
/// </summary>
public class MSTestHandler : ITestFrameworkHandler
{
    public string TestFrameworkName => "MSTest";

    public bool IsTestFrameworkReferenced(CompilationInformation compilation)
    {
        return compilation.ReferencedAssemblies
            .Any(assembly => assembly.Name == "Microsoft.VisualStudio.TestPlatform.TestFramework");
    }

    public ITestFixtureGenerator<TCompilationInformation>? GetTestFixtureGenerator<TCompilationInformation>() 
        where TCompilationInformation : CompilationInformation
    {
        if (typeof(TCompilationInformation).IsAssignableFrom(typeof(CSharpCompilationInformation)))
        {
            return (ITestFixtureGenerator<TCompilationInformation>)new MSTestCSharpTestFixtureGenerator(this);
        }

        return null;
    }
}
