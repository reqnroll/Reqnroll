using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

public class EmptySourceTests
{
    [Fact]
    public void EmptySourceTextCreatesEmptyFeatureFileSyntax()
    {
        var tree = GherkinSyntaxTree.ParseText("");

        var root = tree.GetRoot();
        var featureFileSyntax = root.Should().BeOfType<GherkinDocumentSyntax>().Subject;

        featureFileSyntax.FeatureDeclaration.Should().BeNull();

        featureFileSyntax.EndOfFileToken.GetLocation().Should().BeEquivalentTo(
            Location.Create("", new TextSpan(0, 0), new LinePositionSpan(LinePosition.Zero, LinePosition.Zero)));

        tree.GetDiagnostics().Should().BeEquivalentTo(featureFileSyntax.GetDiagnostics());

        tree.ToString().Should().Be("");
    }
}
