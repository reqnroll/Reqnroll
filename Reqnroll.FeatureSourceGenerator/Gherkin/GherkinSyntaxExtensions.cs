namespace Reqnroll.FeatureSourceGenerator.Gherkin;
internal static class GherkinSyntaxExtensions
{
    public static LinePosition ToLinePosition(this global::Gherkin.Ast.Location location)
    {
        // Roslyn uses 0-based indexes for line and character-offset numbers.
        // The Gherkin parser uses 1-based indexes for line and column numbers.
        return new LinePosition(location.Line - 1, location.Column - 1);
    }
}
