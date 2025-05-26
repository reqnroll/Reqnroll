namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a step in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Step)]
public partial class StepSyntax : SyntaxNode
{
    [SyntaxSlot(
        [SyntaxKind.GivenKeyword, SyntaxKind.WhenKeyword, SyntaxKind.ThenKeyword, SyntaxKind.AndKeyword],
        "The token that represents the keyword of the step.")]
    public partial SyntaxToken StepKeyword { get; }

    [SyntaxSlot([SyntaxKind.LiteralStepText, SyntaxKind.InterpolatedStepText], "The text of the step following the keyword.")]
    public partial StepTextSyntax Text { get; }
}

/// <summary>
/// Represents the text (non-keyword) portion of a step in a Gherkin document. 
/// </summary>
[SyntaxNode]
public abstract partial class StepTextSyntax : SyntaxNode
{
}

/// <summary>
/// Represents the text (non-keyword) portion of a step in a Gherkin document which includes one or more parameters.
/// </summary>
[SyntaxNode(SyntaxKind.InterpolatedStepText)]
public partial class InterpolatedStepTextSyntax : StepTextSyntax
{
    [SyntaxSlot(SyntaxKind.List, "The text of the step following the keyword.")]
    public partial SyntaxList<InterpolatedStepTextContentSyntax> Text { get; }
}

[SyntaxNode(SyntaxKind.LiteralStepText)]
public partial class LiteralStepTextSyntax : StepTextSyntax
{
    [SyntaxSlot(SyntaxKind.StepTextLiteralToken, "The text of the step following the keyword.")]
    public partial SyntaxTokenList Text { get; }
}

/// <summary>
/// Represents a section of interpolated step text.
/// </summary>
[SyntaxNode]
public abstract partial class InterpolatedStepTextContentSyntax : SyntaxNode
{
}

/// <summary>
/// Represents a section of interpolated step text which is a literal string.
/// </summary>
[SyntaxNode(SyntaxKind.InterpolatedStepTextString)]
public partial class InterpolatedStepTextStringSyntax : InterpolatedStepTextContentSyntax
{
    [SyntaxSlot(SyntaxKind.StepTextLiteralToken, "The text token of the string.")]
    public SyntaxToken TextToken { get; }
}

/// <summary>
/// Represents a section of interpolated step text which is a parameter.
/// </summary>
[SyntaxNode(SyntaxKind.InterpolatedStepTextParameter)]
public partial class InterpolatedStepTextParameterSyntax : InterpolatedStepTextContentSyntax
{
    [SyntaxSlot(SyntaxKind.LessThanToken, "The less-than '<' token of the parameter.")]
    public SyntaxToken LessThanToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The identifier of the parameter.")]
    public SyntaxToken Identifier { get; }

    [SyntaxSlot(SyntaxKind.GreaterThanToken, "The greater-than '>' token of the parameter.")]
    public SyntaxToken GreaterThanToken { get; }
}
