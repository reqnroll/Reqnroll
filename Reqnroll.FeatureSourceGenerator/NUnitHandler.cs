using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.FeatureSourceGenerator;

public class NUnitHandler : ITestFrameworkHandler
{
    public string FrameworkName => "NUnit";

    public bool CanGenerateLanguage(string language) => string.Equals(language, LanguageNames.CSharp, StringComparison.Ordinal);

    public SourceText GenerateTestFixture(FeatureInformation feature)
    {
        return feature.CompilationInformation.Language switch
        {
            LanguageNames.CSharp => new NUnitCSharpTestFixtureSyntaxGenerator(feature).GenerateTestFixtureSourceText(),
            _ => throw new NotSupportedException(),
        };
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        throw new NotImplementedException();
    }
}