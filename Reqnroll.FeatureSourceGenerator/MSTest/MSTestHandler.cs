using Reqnroll.FeatureSourceGenerator.CSharp;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

/// <summary>
/// The handler for MSTest.
/// </summary>
public class MSTestHandler : ITestFrameworkHandler
{
    public string FrameworkName => "MSTest";

    public bool CanGenerateLanguage(LanguageInformation language) => language is CSharpLanguageInformation;

    public SourceText GenerateTestFixture(FeatureInformation feature)
    {
        return feature.CompilationInformation switch
        {
            CompilationInformation<CSharpLanguageInformation> => new MSTestCSharpTestFixtureGeneration(feature).GetSourceText(),
            _ => throw new NotSupportedException(),
        };
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return compilationInformation.ReferencedAssemblies
            .Any(assembly => assembly.Name == "Microsoft.VisualStudio.TestPlatform.TestFramework");
    }
}
