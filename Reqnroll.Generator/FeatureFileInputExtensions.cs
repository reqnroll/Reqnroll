using System;
using System.Diagnostics;
using System.IO;
using Reqnroll.Generator.Interfaces;
using UtfUnknown;

namespace Reqnroll.Generator
{
    public static class FeatureFileInputExtensions
    {
        public static TextReader GetFeatureFileContentReader(this FeatureFileInput featureFileInput, ProjectSettings projectSettings)
        {
            if (featureFileInput == null) throw new ArgumentNullException("featureFileInput");

            if (featureFileInput.FeatureFileContent != null)
            {
                return new StringReader(featureFileInput.FeatureFileContent);
            }

            Debug.Assert(projectSettings != null);
            var filePath = Path.Combine(projectSettings.ProjectFolder, featureFileInput.ProjectRelativePath);
            DetectionResult charsetResult = CharsetDetector.DetectFromFile(filePath);
            if (charsetResult != null)
            {
                return new StreamReader(filePath, charsetResult.Detected.Encoding);
            }
            return new StreamReader(filePath);
        }

        public static string GetFullPath(this FeatureFileInput featureFileInput, ProjectSettings projectSettings)
        {
            if (featureFileInput == null) throw new ArgumentNullException("featureFileInput");

            if (projectSettings == null)
                return featureFileInput.ProjectRelativePath;

            return Path.GetFullPath(Path.Combine(projectSettings.ProjectFolder, featureFileInput.ProjectRelativePath));
        }

        public static string GetGeneratedTestFullPath(this FeatureFileInput featureFileInput, ProjectSettings projectSettings)
        {
            if (featureFileInput == null) throw new ArgumentNullException("featureFileInput");

            if (featureFileInput.GeneratedTestProjectRelativePath == null)
                return null;

            if (projectSettings == null)
                return featureFileInput.GeneratedTestProjectRelativePath;

            return Path.GetFullPath(Path.Combine(projectSettings.ProjectFolder, featureFileInput.GeneratedTestProjectRelativePath));
        }

        public static string GetGeneratedTestContent(this FeatureFileInput featureFileInput, string generatedTestFullPath)
        {
            var generatedTestFileContent = featureFileInput.GeneratedTestFileContent;
            if (generatedTestFileContent != null)
                return generatedTestFileContent;

            if (generatedTestFullPath == null)
                return null;

            try
            {
                if (!File.Exists(generatedTestFullPath))
                    return null;

                return File.ReadAllText(generatedTestFullPath);
            }
            catch(Exception exception)
            {
                Debug.WriteLine(exception, "FeatureFileInputExtensions.GetGeneratedTestContent");
                return null;
            }
        }
    }
}
