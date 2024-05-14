namespace Reqnroll.FeatureSourceGenerator;

public interface ITestFrameworkHandler
{
    string FrameworkName { get; }

    bool CanGenerateForCompilation(CompilationInformation compilationInformation);

    SourceText GenerateTestFixture(FeatureInformation feature);

    bool IsTestFrameworkReferenced(CompilationInformation compilationInformation);
}
