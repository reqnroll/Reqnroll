namespace Reqnroll.FeatureSourceGenerator;

public interface ITestFrameworkHandler
{
    string FrameworkName { get; }

    bool CanGenerateLanguage(LanguageInformation language);

    SourceText GenerateTestFixture(FeatureInformation feature);

    bool IsTestFrameworkReferenced(CompilationInformation compilationInformation);
}
