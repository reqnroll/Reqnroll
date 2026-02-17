namespace Reqnroll.Tools.MsBuild.Generation;

public class ReqnrollFeatureFileInfo(string featureFilePath, string featureFileLink, string codeBehindFilePath, string messagesFilePath, string messagesResourceName)
{
    /// <summary>
    /// Path of the existing feature file. Absolute or relative to the project folder.
    /// </summary>
    public string FeatureFilePath { get; } = featureFilePath;

    /// <summary>
    /// Optional link information for feature files imported from outside the project folder.
    /// If the link is provided (not null), the feature file should be handled (e.g. for generated namespace) as a linked file in the project.
    /// </summary>
    public string FeatureFileLink { get; } = string.IsNullOrEmpty(featureFileLink) ? null : featureFileLink;

    /// <summary>
    /// Path of the code behind file to be generated. Absolute or relative to the project folder.
    /// </summary>
    public string CodeBehindFilePath { get; } = codeBehindFilePath;

    /// <summary>
    /// Path of the messages file to be generated (will be embedded as resource). Absolute or relative to the project folder.
    /// </summary>
    public string MessagesFilePath { get; } = messagesFilePath;

    /// <summary>
    /// The resource name to use for the embedded messages resource.
    /// </summary>
    public string MessagesResourceName { get; } = messagesResourceName;
}
