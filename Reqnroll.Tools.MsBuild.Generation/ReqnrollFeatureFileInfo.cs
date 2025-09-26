namespace Reqnroll.Tools.MsBuild.Generation;

public class ReqnrollFeatureFileInfo(string featureFilePath, string codeBehindFilePath, string messagesFilePath)
{
    /// <summary>
    /// Path of the existing feature file. Absolute or relative to the project folder.
    /// </summary>
    public string FeatureFilePath { get; } = featureFilePath;

    /// <summary>
    /// Path of the code behind file to be generated. Absolute or relative to the project folder.
    /// </summary>
    public string CodeBehindFilePath { get; } = codeBehindFilePath;

    /// <summary>
    /// Path of the messages file to be generated (will be embedded as resource). Absolute or relative to the project folder.
    /// </summary>
    public string MessagesFilePath { get; } = messagesFilePath;
}
