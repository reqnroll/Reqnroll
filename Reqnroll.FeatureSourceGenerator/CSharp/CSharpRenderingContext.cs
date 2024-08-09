using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.CSharp;
public class CSharpRenderingContext(
    FeatureInformation feature,
    string outputHintName,
    CSharpSourceTextWriter writer,
    CSharpRenderingOptions renderingOptions) : TestFixtureSourceRenderingContext(feature, outputHintName)
{
    public CSharpSourceTextWriter Writer { get; } = writer;

    public CSharpRenderingOptions RenderingOptions { get; } = renderingOptions;
}
