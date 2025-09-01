namespace Reqnroll.Tools.MsBuild.Generation;

public class FeatureFileCodeBehindGeneratorResult(string codeBehindRelativePath, string embeddedMessagesStoragePath, string embeddedMessagesResourceName)
{
    public string CodeBehindRelativePath { get; } = codeBehindRelativePath;

    public string EmbeddedMessagesStoragePath { get; } = embeddedMessagesStoragePath;

    public string EmbeddedMessagesResourceName { get; } = embeddedMessagesResourceName;
}