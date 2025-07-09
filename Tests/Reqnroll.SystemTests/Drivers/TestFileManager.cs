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

    private string GetPrefix(string? assemblyName = null, string? resourceGroup = null)
    {
        var rootNamespace = assemblyName ?? RootNamespace;
        if (resourceGroup == null)
        {
            return $"{rootNamespace}.{TestFileFolder}";
        }
        else
        {
            return $"{rootNamespace}.{resourceGroup}.{TestFileFolder}";
        }
    }

    public string GetTestFileContent(string testFileName, string? resourceGroup = null, Assembly? assemblyToLoadFrom = null)
    {
        var testFileResourceName = testFileName.Replace('/', '.');
        var assemblyToLoad = assemblyToLoadFrom ?? Assembly.GetExecutingAssembly();
        var assemblyName = assemblyToLoad.GetName().Name;
        var resourceName = $"{GetPrefix(assemblyName, resourceGroup)}.{testFileResourceName}";
        var projectTemplateStream = assemblyToLoad
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