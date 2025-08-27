using System;
using System.Diagnostics;
using System.IO;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;

namespace Reqnroll.Generator
{
    public class TestUpToDateChecker : ITestUpToDateChecker
    {
        protected readonly GeneratorInfo generatorInfo;
        private readonly ProjectSettings projectSettings;

        public TestUpToDateChecker(GeneratorInfo generatorInfo, ProjectSettings projectSettings)
        {
            this.generatorInfo = generatorInfo;
            this.projectSettings = projectSettings;
        }

        private bool IsUpToDateByModificationTime(FeatureFileInput featureFileInput, string generatedTestFullPath)
        {
            if (generatedTestFullPath == null)
                return false;

            // check existence of the target file
            if (!File.Exists(generatedTestFullPath))
                return false;

            // check modification time of the target file
            try
            {
                var featureFileModificationTime = File.GetLastWriteTime(featureFileInput.GetFullPath(projectSettings));
                var codeFileModificationTime = File.GetLastWriteTime(generatedTestFullPath);

                return featureFileModificationTime <= codeFileModificationTime;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public bool? IsUpToDatePreliminary(FeatureFileInput featureFileInput, string generatedTestFullPath, UpToDateCheckingMethod upToDateCheckingMethod)
        {
            if (upToDateCheckingMethod == UpToDateCheckingMethod.ModificationTime)
            {
                return IsUpToDateByModificationTime(featureFileInput, generatedTestFullPath);
            }

            return null;
        }

        public bool IsUpToDate(FeatureFileInput featureFileInput, string generatedTestFullPath, string generatedTestContent, UpToDateCheckingMethod upToDateCheckingMethod)
        {
            string existingFileContent = featureFileInput.GetGeneratedTestContent(generatedTestFullPath);
            return existingFileContent != null && existingFileContent.Equals(generatedTestContent, StringComparison.InvariantCulture);
        }
    }
}