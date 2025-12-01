using System;

namespace Reqnroll.Generator.Interfaces;

/// <summary>
/// Represents the information related to a feature file as an input of the generation
/// </summary>
public class FeatureFileInput(string projectRelativePath)
{
    /// <summary>
    /// The project relative path of the feature file. Must be specified.
    /// </summary>
    public string ProjectRelativePath { get; private set; } = projectRelativePath ?? throw new ArgumentNullException(nameof(projectRelativePath));

    /// <summary>
    /// The content of the feature file. Optional. If not specified, the content is read from <see cref="ProjectRelativePath"/>.
    /// </summary>
    public string FeatureFileContent { get; set; }
    /// <summary>
    /// A custom namespace for the generated test class. Optional.
    /// </summary>
    public string CustomNamespace { get; set; }

    /// <summary>
    /// The resource name to use for the embedded messages resource.
    /// </summary>
    public string MessagesResourceName { get; set; }

    /// <summary>
    /// Absolute path of the code behind file to be generated.
    /// </summary>
    public string CodeBehindFilePath { get; set; }

    /// <summary>
    /// Optional link information for feature files imported from outside the project folder. Relative "virtual" path in the project.
    /// If the link is provided (not null), the feature file should be handled (e.g. for generated namespace) as a linked file in the project.
    /// </summary>
    public string FeatureFileLink { get; set; }
}