using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using Xunit.Abstractions;

namespace Reqnroll.CodeAnalysis.Gherkin;

public class Examples(ITestOutputHelper output)
{
    /// <summary>
    /// This trivial example illustrates how a syntax analyser could be constructed against a parsed syntax tree to
    /// examine the whitespace between a feature declaration's headline and any description.
    /// </summary>
    [Fact]
    public void CanAnalyseWhitespaceInFeatureDeclarations()
    {
        // You can fiddle with this source to see what kinds of different output is produced.
        const string source =
            """
                Feature: Guess the word
                  The word guess game is a turn-based game for two players.
                  The Maker makes a word for the Breaker to guess. The game
                  is over when the Breaker guesses the Maker's word.

            """;

        var syntax = GherkinSyntaxTree.ParseText(source);

        // At this point the ideal strategy would be to implement a visitor pattern to filter to just the nodes we care about.
        // This isn't strictly required at this stage, so this example walks the tree in a more manual/verbose fashion.
        // The Roslyn engine has a whole subsystem for analysers, which we could probably learn from.
        var featureFile = (GherkinDocumentSyntax)syntax.GetRoot();

        var featureDeclaration = featureFile.FeatureDeclaration;

        if (featureDeclaration == null || featureDeclaration.Description == null)
        {
            return;
        }

        var description = featureDeclaration.Description;

        // We examine the description to see if it has a blank line between it and the feature declaration.
        // We associate blank lines with the following syntax.
        if (description.HasLeadingTrivia)
        {
            var leadingTrivia = description.GetLeadingTrivia();

            // Counting the blank lines is about seeing if there are any non-whitespace trivia preceeding each end-of-line trivia.
            // We only care about there being at least one blank line, so we'll stop as soon as we find one.
            var onBlankLine = true;
            foreach (var trivia in leadingTrivia)
            {
                if (trivia.Kind == SyntaxKind.EndOfLineTrivia)
                {
                    if (onBlankLine)
                    {
                        return;
                    }
                    else
                    {
                        onBlankLine = true;
                    }
                }
                else if (trivia.Kind != SyntaxKind.WhitespaceTrivia)
                {
                    onBlankLine = false;
                }
            }
        }

        // I haven't implemented a system of diagnostics yet, but this would be the place to
        // indicate we would like there to be at least one line between the feature's headline and the description.
        output.WriteLine("Should have a blank line between feature headline and description.");
    }
}
