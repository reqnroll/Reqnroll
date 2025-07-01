namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[AttributeUsage(AttributeTargets.Class)]
public class SyntaxNodeAttribute(SyntaxKind syntaxKind) : Attribute
{
    public SyntaxNodeAttribute() : this(SyntaxKind.None)
    {
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
}
