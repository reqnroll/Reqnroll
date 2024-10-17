using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Diagnostics;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Provides information about the rendering of a test-fixture's source.
/// </summary>
/// <param name="feature">The feature that is the primary source of the test-fixture.</param>
/// <param name="outputHintName">The hint name of the test-fixture source being rendered.</param>
[DebuggerDisplay("OutputHintName={OutputHintName}")]
public class TestFixtureSourceRenderingContext(FeatureInformation feature, string outputHintName)
{
    /// <summary>
    /// Gets the hint name of the test-fixture source being rendered.
    /// </summary>
    public string OutputHintName { get; } = outputHintName;

    /// <summary>
    /// Gets the feature that is the primary source of the test-fixture.
    /// </summary>
    public FeatureInformation Feature { get; } = feature;
}
