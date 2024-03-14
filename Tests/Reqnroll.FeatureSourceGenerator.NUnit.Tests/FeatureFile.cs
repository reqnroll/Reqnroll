using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

internal class FeatureFile(string path, string content) : AdditionalText
{
    private readonly SourceText _sourceText = SourceText.From(content);

    public override string Path { get; } = path;

    public override SourceText? GetText(CancellationToken cancellationToken = default)
    {
        return _sourceText;
    }
}
