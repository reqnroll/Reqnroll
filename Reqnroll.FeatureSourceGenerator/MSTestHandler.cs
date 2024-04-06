using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.FeatureSourceGenerator;

public class MSTestHandler : ITestFrameworkHandler
{
    public string FrameworkName => throw new NotImplementedException();

    public bool CanGenerateLanguage(string language)
    {
        throw new NotImplementedException();
    }

    public SourceText GenerateTestFixture(FeatureInformation feature)
    {
        throw new NotImplementedException();
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        throw new NotImplementedException();
    }
}