using Microsoft.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static InternalSyntaxFactory;

public class InternalSyntaxListTests
{
    [Fact]
    public void CreateWithSpanOfNodesCreatesList()
    {
        var first = LiteralText(Literal(null, "Alpha", "Alpha", null));
        var second = LiteralText(Literal(null, "Beta", "Beta", null));
        var third = LiteralText(Literal(null, "Gamma", "Gamma", null));

        var list = InternalSyntaxList.Create([first, second, third]);

        list.SlotCount.Should().Be(3);

        list[0].Should().BeEquivalentTo(first);
        list[1].Should().BeEquivalentTo(second);
        list[2].Should().BeEquivalentTo(third);
    }

    [Fact]
    public void EnumeratorReturnsNodesInOrder()
    {
        var first = LiteralText(Literal(null, "Alpha", "Alpha", null));
        var second = LiteralText(Literal(null, "Beta", "Beta", null));
        var third = LiteralText(Literal(null, "Gamma", "Gamma", null));

        var list = InternalSyntaxList.Create([first, second, third]);

        list.ToList().Should().BeEquivalentTo([first, second, third]);
    }

    [Fact]
    public void EnumeratorReturnsTheCorrectNumberOfNodes()
    {
        var first = LiteralText(Literal(null, "Alpha", "Alpha", null));
        var second = LiteralText(Literal(null, "Beta", "Beta", null));
        var third = LiteralText(Literal(null, "Gamma", "Gamma", null));

        var list = InternalSyntaxList.Create([first, second, third]);

        list.Should().HaveCount(3);
    }

    [Fact]
    public void WithAnnotationsReturnsNewListWithAnnotations()
    {
        var first = LiteralText(Literal(null, "Alpha", "Alpha", null));
        var second = LiteralText(Literal(null, "Beta", "Beta", null));
        var third = LiteralText(Literal(null, "Gamma", "Gamma", null));

        var list = InternalSyntaxList.Create([first, second, third]);

        var annotation = new SyntaxAnnotation("Test");

        var annotatedList = list.WithAnnotations(annotation);

        annotatedList.GetAnnotations().Should().BeEquivalentTo([ annotation ]);
    }

    [Fact]
    public void WithDiagnosticsReturnsNewListWithDiagnostics()
    {
        var first = LiteralText(Literal(null, "Alpha", "Alpha", null));
        var second = LiteralText(Literal(null, "Beta", "Beta", null));
        var third = LiteralText(Literal(null, "Gamma", "Gamma", null));

        var list = InternalSyntaxList.Create([first, second, third]);

        var diagnostic = InternalDiagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag);

        var annotatedList = list.WithDiagnostics([ diagnostic ]);

        annotatedList.GetAttachedDiagnostics().Should().BeEquivalentTo([ diagnostic ]);
    }
}
