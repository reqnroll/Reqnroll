using Microsoft.CodeAnalysis.CSharp;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

public class CSharpLanguageInformation(LanguageVersion version) : LanguageInformation(LanguageNames.CSharp)
{
    public LanguageVersion Version { get; } = version;
}
