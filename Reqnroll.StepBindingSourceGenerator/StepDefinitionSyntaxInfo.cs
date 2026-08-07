using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reqnroll.StepBindingSourceGenerator;

/// <summary>
/// Represents the pipeline-relevant information about the declaration of a step definition.
/// </summary>
/// <param name="MethodDeclarationSyntax">The method declaration syntax which has been identified to be a step 
/// definition.</param>
internal record StepDefinitionSyntaxInfo(
    MethodDeclarationSyntax MethodDeclarationSyntax,
    MethodInfo Method,
    StepKeywordMatch MatchedKeywords,
    string? TextPattern);
