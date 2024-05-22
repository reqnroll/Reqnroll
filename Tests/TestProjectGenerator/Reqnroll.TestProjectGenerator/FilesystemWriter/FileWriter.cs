using System;
using System.IO;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
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

            projectFile.Freeze();
        }
    }
}
