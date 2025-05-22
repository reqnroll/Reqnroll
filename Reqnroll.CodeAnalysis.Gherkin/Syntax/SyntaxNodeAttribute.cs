namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[AttributeUsage(AttributeTargets.Class)]
public class SyntaxNodeAttribute(SyntaxKind syntaxKind) : Attribute
{
    public SyntaxKind SyntaxKind { get; } = syntaxKind;
}
