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
}