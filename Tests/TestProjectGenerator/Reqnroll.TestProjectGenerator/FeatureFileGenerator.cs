using System;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator;

public class FeatureFileGenerator
{
    public ProjectFile Generate(string featureFileContent, string featureFileName = null)
    {
        featureFileName ??= $"FeatureFile{Guid.NewGuid():N}.feature";

        string fileContent = featureFileContent.Replace("'''", "\"\"\"");
        return new ProjectFile(featureFileName, "None", fileContent);
    }
}