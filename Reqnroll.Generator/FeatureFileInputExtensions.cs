using System;
using System.IO;
using Reqnroll.Generator.Interfaces;

namespace Reqnroll.Generator;

public static class FeatureFileInputExtensions
{
    public static TextReader GetFeatureFileContentReader(this FeatureFileInput featureFileInput, ProjectSettings projectSettings)
    {
        if (featureFileInput == null) throw new ArgumentNullException(nameof(featureFileInput));

        if (featureFileInput.FeatureFileContent != null)
            return new StringReader(featureFileInput.FeatureFileContent);

        return new StreamReader(featureFileInput.GetFullPath(projectSettings));
    }

    public static string GetFullPath(this FeatureFileInput featureFileInput, ProjectSettings projectSettings) =>
        projectSettings.GetFullPath(featureFileInput.ProjectRelativePath);

    public static string GetFullPath(this ProjectSettings projectSettings, string projectRelativePath)
    {
        if (projectSettings == null) throw new ArgumentNullException(nameof(projectSettings));
        if (projectRelativePath == null) throw new ArgumentNullException(nameof(projectRelativePath));

        return Path.GetFullPath(Path.Combine(projectSettings.ProjectFolder, projectRelativePath));
    }
}