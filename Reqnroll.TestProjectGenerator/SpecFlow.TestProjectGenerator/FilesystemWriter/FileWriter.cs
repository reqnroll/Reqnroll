using System;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class FileWriter
    {
        public void Write(SolutionFile projectFile, string projectRootPath)
        {
            if (projectFile is null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            string absolutePath = Path.Combine(projectRootPath, projectFile.Path);
            string folderPath = Path.GetDirectoryName(absolutePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (projectFile.Content.IsNotNullOrWhiteSpace())
            {
                File.WriteAllText(absolutePath, projectFile.Content);
            }
        }
    }
}
