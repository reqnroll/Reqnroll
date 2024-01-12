using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Specs.Support
{
    public class TestFileManager
    {
        public string GetTestFileContent(string testfileName)
        {
            var projectTemplateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Reqnroll.Specs.TestFiles." + testfileName);
            Debug.Assert(projectTemplateStream != null);
            string fileContent = new StreamReader(projectTemplateStream).ReadToEnd();
            return fileContent;
        }

        public IEnumerable<string> GetTestFeatureFiles()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string prefixToRemove = "Reqnroll.Specs.TestFiles.";
            return assembly.GetManifestResourceNames()
                .Where(rn => rn.EndsWith(".feature") && rn.StartsWith(prefixToRemove))
                .Select(rn => rn.Substring(prefixToRemove.Length));
        }
    }
}
