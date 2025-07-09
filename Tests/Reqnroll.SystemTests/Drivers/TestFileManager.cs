using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Assembly = System.Reflection.Assembly;

namespace Reqnroll.SystemTests.Drivers;

//ported from Specs
public class TestFileManager
{
    private const string RootNamespace = "Reqnroll.SystemTests";
    private const string TestFileFolder = "Resources";

    private string GetPrefix(string? resourceGroup = null) =>
        resourceGroup == null ? $"{RootNamespace}.{TestFileFolder}" : $"{RootNamespace}.{resourceGroup}.{TestFileFolder}";

    public string GetTestFileContent(string testFileName, string? resourceGroup = null)
    {
        var testFileResourceName = testFileName.Replace('/', '.');
        var resourceName = $"{GetPrefix(resourceGroup)}.{testFileResourceName}";
        var projectTemplateStream = Assembly
                                    .GetExecutingAssembly()
                                    .GetManifestResourceStream(resourceName);
        projectTemplateStream.Should().NotBeNull($"Resource with name '{resourceName}' should be an embedded resource");
        Debug.Assert(projectTemplateStream != null);
        string fileContent = new StreamReader(projectTemplateStream).ReadToEnd();
        return fileContent;
    }

    public IEnumerable<string> GetTestFeatureFiles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string prefixToRemove = $"{GetPrefix()}.";
        return assembly.GetManifestResourceNames()
                       .Where(rn => rn.EndsWith(".feature") && rn.StartsWith(prefixToRemove))
                       .Select(rn => rn.Substring(prefixToRemove.Length));
    }
}