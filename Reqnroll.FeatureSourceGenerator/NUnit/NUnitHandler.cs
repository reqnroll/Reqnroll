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

    public SourceText GenerateTestFixture(FeatureInformation feature)
    {
        return feature.CompilationInformation switch
        {
            CSharpCompilationInformation => new NUnitCSharpSyntaxGeneration(feature).GetSourceText(),
            _ => throw new NotSupportedException(),
        };
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return compilationInformation.ReferencedAssemblies.Any(assembly => assembly.Name == "nunit.framework");
    }
}
