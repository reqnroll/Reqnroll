namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a step in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Step)]
public partial class StepSyntax : SyntaxNode
{
    [SyntaxSlot([
        SyntaxKind.GivenKeyword,
        SyntaxKind.WhenKeyword,
        SyntaxKind.ThenKeyword,
        SyntaxKind.AndKeyword,
        SyntaxKind.ButKeyword,
        SyntaxKind.AsterixToken],
        "The token that represents the keyword of the step.")]
    public partial SyntaxToken StepKeyword { get; }

    [SyntaxSlot([SyntaxKind.LiteralText, SyntaxKind.InterpolatedText], "The text of the step following the keyword.")]
    public partial PlainTextSyntax Text { get; }

    [SyntaxSlot([SyntaxKind.StepTable, SyntaxKind.StepDocString], "The optional data associated with the step.")]
    public partial StepDataSyntax? Data { get; }
}

/// <summary>
/// Represents the data associated with a step in a Gherkin document, such as a table or a doc string.
/// </summary>
[SyntaxNode]
public abstract partial class StepDataSyntax : SyntaxNode
{
}

/// <summary>
/// Represents a table argument of a step.
/// </summary>
[SyntaxNode(SyntaxKind.StepTable)]
public partial class StepTableSyntax : StepDataSyntax
{
    [SyntaxSlot(SyntaxKind.Table, "The table argument of the step data.")]
    public partial TableSyntax Table { get; }
}

/// <summary>
/// Represents a Doc String argument of a step.
/// </summary>
[SyntaxNode(SyntaxKind.StepDocString)]
public partial class StepDocStringSyntax : StepDataSyntax
{
    [SyntaxSlot(SyntaxKind.DocString, "The Doc String argument of the step data.")]
    public partial DocStringSyntax DocString { get; }
}

/// <summary>
/// Represents non-structural text in a Gherkin document, such as the name of a scenario, 
/// a block of descriptive text, or the text of a step. 
/// </summary>
[SyntaxNode]
public abstract partial class PlainTextSyntax : SyntaxNode
{
}

/// <summary>
/// Represents plain, non-structural text which includes one or more parameters.
/// </summary>
[SyntaxNode(SyntaxKind.InterpolatedText)]
public partial class InterpolatedTextSyntax : PlainTextSyntax
{
    [SyntaxSlot([SyntaxKind.InterpolatedLiteralToken, SyntaxKind.InterpolatedTableLiteralToken, SyntaxKind.Interpolation], "The interpolated text elements.")]
    public partial SyntaxList<InterpolatedTextContentSyntax> Text { get; }
}

/// <summary>
/// Represents plain, non-structural text.
/// </summary>
[SyntaxNode(SyntaxKind.LiteralText)]
public partial class LiteralTextSyntax : PlainTextSyntax
{
    [SyntaxSlot([SyntaxKind.LiteralToken, SyntaxKind.TableLiteralToken], "The text of the step following the keyword.")]
    public partial SyntaxTokenList Text { get; }
}

/// <summary>
/// Represents a section of interpolated text.
/// </summary>
[SyntaxNode]
public abstract partial class InterpolatedTextContentSyntax : SyntaxNode
{
}

/// <summary>
/// Represents a section of interpolated step text which is a literal string.
/// </summary>
[SyntaxNode(SyntaxKind.InterpolatedTextLiteral)]
public partial class InterpolatedTextLiteralSyntax : InterpolatedTextContentSyntax
{
    [SyntaxSlot(
        [SyntaxKind.InterpolatedLiteralToken, SyntaxKind.InterpolatedTableLiteralToken],
        "The literal token of the text.")]
    public partial SyntaxToken TextToken { get; }
}

/// <summary>
/// Represents the interpolation of a parameter within interpolated text.
/// </summary>
[SyntaxNode(SyntaxKind.Interpolation)]
public partial class InterpolationSyntax : InterpolatedTextContentSyntax
{
    [SyntaxSlot(SyntaxKind.LessThanToken, "The less-than '<' token of the parameter.")]
    public partial SyntaxToken LessThanToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The identifier of the parameter.")]
    public partial SyntaxToken Identifier { get; }

    [SyntaxSlot(SyntaxKind.GreaterThanToken, "The greater-than '>' token of the parameter.")]
    public partial SyntaxToken GreaterThanToken { get; }
}
