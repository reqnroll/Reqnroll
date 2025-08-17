using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class GherkinDocumentTests
{
    [Fact]
    public void CommentOnlySourceIsRepresentedInTree()
    {
        // Taken from good/incomplete_feature_3.feature
        const string source =
            """
            # Just a comment
            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument()
                .WithEndOfFileToken(
                    Token(
                        TriviaList([Comment("# Just a comment")]),
                        SyntaxKind.EndOfFileToken,
                        TriviaList()))
        );

        tree.ToString().Should().Be(source);
    }
}
