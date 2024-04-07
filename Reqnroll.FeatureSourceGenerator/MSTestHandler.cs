using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.FeatureSourceGenerator;

public class MSTestHandler : ITestFrameworkHandler
{
    public string FrameworkName => "MSTest";

    public bool CanGenerateLanguage(string language) => string.Equals(language, LanguageNames.CSharp, StringComparison.Ordinal);

    public SourceText GenerateTestFixture(FeatureInformation feature)
    {
        return feature.CompilationInformation.Language switch
        {
            LanguageNames.CSharp => new MSTestCSharpTestFixtureSourceTextGenerator(feature).GenerateTestFixtureSourceText(),
            _ => throw new NotSupportedException(),
        };
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return compilationInformation.ReferencedAssemblies
            .Any(assembly => assembly.Name == "Microsoft.VisualStudio.TestPlatform.TestFramework");
    }
}