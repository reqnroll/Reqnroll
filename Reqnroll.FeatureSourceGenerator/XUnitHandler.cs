using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.FeatureSourceGenerator;

public class XUnitHandler : ITestFrameworkHandler
{
    public string FrameworkName => "xUnit";

    public bool CanGenerateLanguage(string language) => string.Equals(language, LanguageNames.CSharp, StringComparison.Ordinal);

    public SourceText GenerateTestFixture(FeatureInformation feature)
    {
        throw new NotImplementedException();
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return false;
    }
}