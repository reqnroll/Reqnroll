using Microsoft.CodeAnalysis.VisualBasic;

namespace Reqnroll.FeatureSourceGenerator.VisualBasic;

public class VisualBasicLanguageInformation(LanguageVersion version) : LanguageInformation(LanguageNames.VisualBasic)
{
    public LanguageVersion Version { get; } = version;
}
