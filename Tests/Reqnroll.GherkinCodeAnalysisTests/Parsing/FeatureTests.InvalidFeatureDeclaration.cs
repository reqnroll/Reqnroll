using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class FeatureTests
{
    [Fact]
    public void InvalidFeatureSourceTextCreatesTreeWithSkippedTokensAndDiagnostic()
    {
        const string source = "invalid ";

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    MissingToken(SyntaxKind.FeatureKeyword),
                    MissingToken(SyntaxKind.ColonToken),
                    LiteralText(Literal(TriviaList(), "invalid", TriviaList([Space])))),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
