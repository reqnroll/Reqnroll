using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static InternalSyntaxFactory;

public class DiagnosticTests
{
    [Fact]
    public void CanAttachDiagnosticsToSyntaxToken()
    {
        const string text = "Descriptive, eh?";
        var token = Literal(null, SyntaxKind.DescriptionTextToken, text, text, null)
            .WithDiagnostics([InternalDiagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag) ]);

        // Create a sytax node to attach the token to a tree.
        var description = (DescriptionSyntax)Description(token).CreateSyntaxNode();

        var textToken = description.Lines[0];

        textToken.ContainsDiagnostics.Should().BeTrue();
        textToken.GetDiagnostics().Should().HaveCount(1);

        description.GetDiagnostics().Should().BeEquivalentTo(textToken.GetDiagnostics());
        description.SyntaxTree.GetDiagnostics().Should().BeEquivalentTo(textToken.GetDiagnostics());

        var diagnostic = textToken.GetDiagnostics().First();

        diagnostic.Should().BeEquivalentTo(
            Diagnostic.Create(
                DiagnosticDescriptors.ErrorExpectedFeatureOrTag,
                description.SyntaxTree.GetLocation(new TextSpan(0, text.Length))));
    }

    [Fact]
    public void CanAttachDiagnosticsToFloatingSyntaxToken()
    {
        const string text = "Descriptive, eh?";
        var token = Token(null, SyntaxKind.NameToken, text, null)
            .WithDiagnostics([InternalDiagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag)]);

        var textToken = new SyntaxToken(token);

        textToken.ContainsDiagnostics.Should().BeTrue();
        textToken.GetDiagnostics().Should().HaveCount(1);

        var diagnostic = textToken.GetDiagnostics().First();

        diagnostic.Should().BeEquivalentTo(Diagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag, null));
    }

    [Fact]
    public void CanAttachDiagnosticsToSyntaxTrivia()
    {
        var leading = Whitespace("    ")
            .WithDiagnostics([InternalDiagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag)]);
        var trailing = CarriageReturnLineFeed
            .WithDiagnostics([InternalDiagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag)]);

        // Create a token and syntax node to attach the trivia to a tree.
        const string text = "Descriptive, eh?";
        var token = Token(leading, SyntaxKind.DescriptionTextToken, text, trailing);
        var description = (DescriptionSyntax)Description(token).CreateSyntaxNode();

        var textToken = description.Lines.Single();

        var leadingTrivia = textToken.LeadingTrivia[0];
        var trailingTrivia = textToken.TrailingTrivia[0];

        leadingTrivia.ContainsDiagnostics.Should().BeTrue();
        leadingTrivia.GetDiagnostics().Should().HaveCount(1);

        trailingTrivia.ContainsDiagnostics.Should().BeTrue();
        trailingTrivia.GetDiagnostics().Should().HaveCount(1);

        var leadingDiagnostic = leadingTrivia.GetDiagnostics().Single();
        var trailingDiagnostic = trailingTrivia.GetDiagnostics().Single();

        description.GetDiagnostics().Should().BeEquivalentTo([leadingDiagnostic, trailingDiagnostic]);

        leadingDiagnostic.Should().BeEquivalentTo(
            Diagnostic.Create(
                DiagnosticDescriptors.ErrorExpectedFeatureOrTag,
                description.SyntaxTree.GetLocation(new TextSpan(0, 4))));

        trailingDiagnostic.Should().BeEquivalentTo(
            Diagnostic.Create(
                DiagnosticDescriptors.ErrorExpectedFeatureOrTag,
                description.SyntaxTree.GetLocation(new TextSpan(4 + text.Length, 2))));
    }

    [Fact]
    public void CanAttachDiagnosticsToFloatingSyntaxTrivia()
    {
        var trivia = (SyntaxTrivia)(InternalSyntaxTrivia)Whitespace("    ")
            .WithDiagnostics([InternalDiagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag)]);

        trivia.ContainsDiagnostics.Should().BeTrue();
        trivia.GetDiagnostics().Should().HaveCount(1);

        var diagnostic = trivia.GetDiagnostics().Single();

        diagnostic.Should().BeEquivalentTo(
            Diagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag, null));
    }

    [Fact]
    public void CanAttachDiagnosticsToSyntaxNode()
    {
        const string text = "Descriptive, eh?";

        var description = (DescriptionSyntax)Description(Token(null, SyntaxKind.DescriptionTextToken, text, null))
            .WithDiagnostics([InternalDiagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag)])
            .CreateSyntaxNode();

        description.HasDiagnostics.Should().BeTrue();
        description.GetDiagnostics().Should().HaveCount(1);

        var diagnostic = description.GetDiagnostics().Single();

        diagnostic.Should().BeEquivalentTo(
            Diagnostic.Create(
                DiagnosticDescriptors.ErrorExpectedFeatureOrTag,
                description.SyntaxTree.GetLocation(new TextSpan(0, text.Length))));
    }

    [Fact]
    public void AttachedDiagnosticsAreReturnedInDocumentOrder()
    {
        var descriptor1 = new DiagnosticDescriptor("GT1", "Diagnostic 1", "", "", DiagnosticSeverity.Error, true);
        var diagnostic1 = InternalDiagnostic.Create(descriptor1);

        var descriptor2 = new DiagnosticDescriptor("GT2", "Diagnostic 2", "", "", DiagnosticSeverity.Error, true);
        var diagnostic2 = InternalDiagnostic.Create(descriptor2);

        var feature = (FeatureSyntax)Feature(
            default,
            Token(SyntaxKind.FeatureKeyword, "Feature").WithDiagnostics([diagnostic2]),
            Token(null, SyntaxKind.ColonToken, Whitespace(" ").WithDiagnostics([diagnostic1])).WithDiagnostics([diagnostic2]),
            Literal(null, SyntaxKind.NameToken, "Guess the word", "Guess the word", CarriageReturnLineFeed),
            Description(
                Literal(
                    CarriageReturnLineFeed,
                    SyntaxKind.DescriptionTextToken,
                    "An example feature from the Gherkin reference.",
                    "An example feature from the Gherkin reference.",
                    CarriageReturnLineFeed)),
            default,
            default,
            default)
            .WithDiagnostics([diagnostic1, diagnostic2])
            .CreateSyntaxNode();

        var syntaxTree = feature.SyntaxTree;

        feature.GetDiagnostics().Should().BeEquivalentTo(
            [
                Diagnostic.Create(descriptor1, syntaxTree.GetLocation(new TextSpan(0, feature.Span.Length))),
                Diagnostic.Create(descriptor2, syntaxTree.GetLocation(new TextSpan(0, feature.Span.Length))),
                Diagnostic.Create(descriptor2, syntaxTree.GetLocation(new TextSpan(0, 7))),
                Diagnostic.Create(descriptor2, syntaxTree.GetLocation(new TextSpan(7, 1))),
                Diagnostic.Create(descriptor1, syntaxTree.GetLocation(new TextSpan(8, 1))),
            ],
            options => options.ComparingByMembers<Location>());
    }
}
