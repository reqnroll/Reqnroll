using System;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class FeatureFileGenerator
    {
        public ProjectFile Generate(string featureFileContent, string featureFileName = null)
        {
            featureFileName = featureFileName ?? $"FeatureFile{Guid.NewGuid():N}.feature";


            string fileContent = featureFileContent.Replace("'''", "\"\"\"");
            return new ProjectFile(featureFileName, "None", fileContent);
        }
    }
}
