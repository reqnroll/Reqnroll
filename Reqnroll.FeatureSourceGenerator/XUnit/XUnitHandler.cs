using Reqnroll.FeatureSourceGenerator.CSharp;

namespace Reqnroll.FeatureSourceGenerator.XUnit;

/// <summary>
/// The handler for xUnit.
/// </summary>
public class XUnitHandler : ITestFrameworkHandler
{
    public string FrameworkName => "xUnit";

    public bool CanGenerateLanguage(LanguageInformation language) => language is CSharpLanguageInformation;

    public SourceText GenerateTestFixture(FeatureInformation feature)
    {
        return feature.CompilationInformation switch
        {
            CompilationInformation<CSharpLanguageInformation> => new XUnitCSharpSyntaxGeneration(feature).GetSourceText(),
            _ => throw new NotSupportedException(),
        };
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return compilationInformation.ReferencedAssemblies
            .Any(assembly => assembly.Name == "xunit.core");
    }
}
