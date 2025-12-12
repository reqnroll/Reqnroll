using System;
using System.Collections.Generic;
using System.Text;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;

namespace Reqnroll.Analyzers.StepDefinitions;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StepTextCodeFixProvider)), Shared]
public class StepTextCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        StepTextAnalyzer.StepTextCannotBeNullOrEmptyRule.Id,
        StepTextAnalyzer.StepTextShouldNotHaveLeadingWhitespaceRule.Id,
        StepTextAnalyzer.StepTextShouldNotHaveTrailingWhitespaceRule.Id);

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

        if (diagnostic.Descriptor == StepTextAnalyzer.StepTextCannotBeNullOrEmptyRule)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove step text argument",
                    createChangedDocument: _ => RemoveStepTextArgumentAsync(context.Document, root, argument),
                    equivalenceKey: "RemoveStepTextArgument"),
                diagnostic);
        }
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
