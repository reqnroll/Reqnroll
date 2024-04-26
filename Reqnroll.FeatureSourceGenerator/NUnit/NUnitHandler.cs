namespace Reqnroll.FeatureSourceGenerator.NUnit;

/// <summary>
/// The handler for NUnit.
/// </summary>
public class NUnitHandler : ITestFrameworkHandler
{
    public string FrameworkName => "NUnit";

    public bool CanGenerateLanguage(string language) => string.Equals(language, LanguageNames.CSharp, StringComparison.Ordinal);

    public SourceText GenerateTestFixture(FeatureInformation feature)
    {
        return feature.CompilationInformation.Language switch
        {
            LanguageNames.CSharp => new NUnitCSharpSyntaxGeneration(feature).GetSourceText(),
            _ => throw new NotSupportedException(),
        };
    }

    public bool IsTestFrameworkReferenced(CompilationInformation compilationInformation)
    {
        return compilationInformation.ReferencedAssemblies.Any(assembly => assembly.Name == "nunit.framework");
    }
}
