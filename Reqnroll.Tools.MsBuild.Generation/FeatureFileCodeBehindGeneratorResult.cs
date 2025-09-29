namespace Reqnroll.Tools.MsBuild.Generation;

public class FeatureFileCodeBehindGeneratorResult(
    string codeBehindFileFullPath,
    string messagesResourceName,
    string messagesFileFullPath)
{
    public string CodeBehindFileFullPath { get; } = codeBehindFileFullPath;

    public string MessagesResourceName { get; } = messagesResourceName;

    public string MessagesFileFullPath { get; } = messagesFileFullPath;
}