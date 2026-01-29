using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

namespace Reqnroll.Analyzers.StepDefinitions;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StepDefinitionExpressionCodeFixProvider)), Shared]
public class StepDefinitionExpressionCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        StepDefinitionExpressionAnalyzer.StepDefinitionExpressionCannotBeNullRule.Id,
        StepDefinitionExpressionAnalyzer.StepDefinitionExpressionCannotBeEmptyOrWhitespaceRule.Id,
        StepDefinitionExpressionAnalyzer.StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule.Id);

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

        if (root == null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();

        var argument = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!
            .AncestorsAndSelf().OfType<AttributeArgumentSyntax>().First();

        if (diagnostic.Descriptor == StepDefinitionExpressionAnalyzer.StepDefinitionExpressionCannotBeNullRule || 
            diagnostic.Descriptor == StepDefinitionExpressionAnalyzer.StepDefinitionExpressionCannotBeEmptyOrWhitespaceRule)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.RemoveStepTextArgumentTitle,
                    createChangedDocument: _ => RemoveStepTextArgumentAsync(context.Document, root, argument),
                    equivalenceKey: "RemoveStepTextArgument"),
                diagnostic);
        }

        if (diagnostic.Descriptor == StepDefinitionExpressionAnalyzer.StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.TrimStepTextArgumentTitle,
                    createChangedDocument: _ => TrimStepTextArgumentAsync(context.Document, root, argument),
                    equivalenceKey: "TrimStepTextArgument"),
                diagnostic);
        }
    }

    private async Task<Document> TrimStepTextArgumentAsync(
        Document document,
        SyntaxNode root,
        AttributeArgumentSyntax argument)
    {
        // Create a new argument with trimmed text.
        var literalExpression = (LiteralExpressionSyntax)argument.Expression;
        var originalText = (string)literalExpression.Token.Value!;
        var trimmedText = originalText.Trim();
        var newLiteralExpression = literalExpression.WithToken(SyntaxFactory.Literal(trimmedText));

        // Replace the argument expression.
        var newRoot = root.ReplaceNode(literalExpression, newLiteralExpression);

        return document.WithSyntaxRoot(newRoot);
    }

    private async Task<Document> RemoveStepTextArgumentAsync(
        Document document,
        SyntaxNode root,
        AttributeArgumentSyntax argument)
    {
        // Create a new argument list without the specified argument.
        var argumentList = (AttributeArgumentListSyntax)argument.Parent!;
        var newArgumentList = argumentList.RemoveNode(argument, SyntaxRemoveOptions.KeepNoTrivia);

        // Replace the argument list.
        SyntaxNode newRoot;
        if (newArgumentList == null || newArgumentList.Arguments.Count == 0)
        {
            newRoot = root.RemoveNode(argumentList, SyntaxRemoveOptions.KeepNoTrivia) ?? SyntaxFactory.CompilationUnit();
        }
        else
        {
            newRoot = root.ReplaceNode(argumentList, newArgumentList);
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
