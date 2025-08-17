using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class GherkinDocumentTests
{
    [Fact]
    public void InvalidFeatureSourceTextCreatesTreeWithSkippedTokensAndDiagnostic()
    {
        const string source = "invalid ";

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                null,
                Token(
                    TriviaList([
                        Trivia(
                            SkippedTokensTrivia(
                                TokenList([
                                    Literal(TriviaList(), "invalid", TriviaList([Space]))
                                ])))
                        ]),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
