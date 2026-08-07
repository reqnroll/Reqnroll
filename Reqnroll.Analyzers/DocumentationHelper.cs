namespace Reqnroll.Analyzers;

internal static class DocumentationHelper
{
    public static string GetDocumentationUrl(string relativePath) => 
        $"https://docs.reqnroll.net/latest/" + relativePath;

    public static string GetRuleDocumentationUrl(string ruleId) => 
        GetDocumentationUrl($"automation/code-analysis/{ruleId.ToLowerInvariant()}.html");
}
