namespace Reqnroll.Analyzers.StepDefinitions;

internal static class AttributeHelper
{
    private static readonly HashSet<string> StepAttributeTypes =
    [
        "Reqnroll.GivenAttribute",
        "Reqnroll.WhenAttribute",
        "Reqnroll.ThenAttribute",
        "Reqnroll.StepDefinitionAttribute"
    ];

    private static bool IsStepAttributeType(string qualifiedTypeName) => StepAttributeTypes.Contains(qualifiedTypeName);

    public static bool IsStepAttribute(AttributeData attributeData)
    {
        if (attributeData.AttributeClass == null)
        {
            return false;
        }

        return IsStepAttributeType(attributeData.AttributeClass.ToDisplayString());
    }

    public static bool IsStepAttribute(ISymbol symbol)
    {
        return IsStepAttributeType(symbol.ContainingType.ToDisplayString());
    }
}
