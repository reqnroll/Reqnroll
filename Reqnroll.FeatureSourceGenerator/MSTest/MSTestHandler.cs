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

    public ITestFixtureGenerator<TCompilationInformation>? GetTestFixtureGenerator<TCompilationInformation>(
        TCompilationInformation compilation) 
        where TCompilationInformation : CompilationInformation
    {
        if (typeof(TCompilationInformation).IsAssignableFrom(typeof(CSharpCompilationInformation)))
        {
            var version = compilation.ReferencedAssemblies
                .Where(assembly => assembly.Name == "Microsoft.VisualStudio.TestPlatform.TestFramework")
                .Max(assembly => assembly.Version);

            if (version >= new Version(3, 0))
            {
                return (ITestFixtureGenerator<TCompilationInformation>)new MSTest3CSharpTestFixtureGenerator(this);
            }

            return (ITestFixtureGenerator<TCompilationInformation>)new MSTest2CSharpTestFixtureGenerator(this);
        }

        return null;
    }
}
