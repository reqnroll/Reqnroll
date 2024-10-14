using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;

namespace Reqnroll.SystemTests.Drivers;

//ported from Specs
public class TestFileManager
{
    private const string RootNamespace = "Reqnroll.SystemTests";
    private const string TestFileFolder = "Resources";
    private readonly string _prefix = $"{RootNamespace}.{TestFileFolder}";

    public string GetTestFileContent(string testFileName, string? prefixOverride = null, Assembly? assemblyToLoadFrom = null)
    {

        var prefix = prefixOverride ?? _prefix;
        var assembly = assemblyToLoadFrom ?? Assembly.GetExecutingAssembly();
        var testFileResourceName = testFileName.Replace('/', '.');
        var resourceName = $"{prefix}.{testFileResourceName}";
        var projectTemplateStream = assembly
                                    .GetManifestResourceStream(resourceName);
        projectTemplateStream.Should().NotBeNull($"Resource with name '{resourceName}' should be an embedded resource");
        Debug.Assert(projectTemplateStream != null);
        string fileContent = new StreamReader(projectTemplateStream).ReadToEnd();
        return fileContent;
    }

    public IEnumerable<string> GetTestFeatureFiles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string prefixToRemove = $"{_prefix}.";
        return assembly.GetManifestResourceNames()
                       .Where(rn => rn.EndsWith(".feature") && rn.StartsWith(prefixToRemove))
                       .Select(rn => rn.Substring(prefixToRemove.Length));
    }
}