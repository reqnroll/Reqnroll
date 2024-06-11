namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Represents a Reqnroll text fixture.
/// </summary>
/// <param name="feature">The feature the test fixture provides a representation of.</param>
public abstract class TestFixture(FeatureInformation feature)
{
    public FeatureInformation Feature { get; } = feature;

    public string HintName => Feature.FeatureHintName;

    /// <summary>
    /// Renders the configured test fixture to source text.
    /// </summary>
    /// <returns>A <see cref="SourceText"/> representing the rendered test fixture.</returns>
    public SourceText Render()
    {
        throw new NotImplementedException();
    }
}
