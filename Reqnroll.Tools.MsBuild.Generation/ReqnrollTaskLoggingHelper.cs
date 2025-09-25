using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Reqnroll.Tools.MsBuild.Generation;

public class ReqnrollTaskLoggingHelper(TaskLoggingHelper taskLoggingHelper) : IReqnrollTaskLoggingHelper
{
    public bool HasLoggedErrors => taskLoggingHelper.HasLoggedErrors;

    private string GetMessageWithNameTag(string message)
    {
        return $"[Reqnroll] {message}";
    }

    public void LogTaskMessage(string message)
    {
        string messageWithNameTag = GetMessageWithNameTag(message);
        taskLoggingHelper.LogMessage(messageWithNameTag);
    }

    public void LogTaskDiagnosticMessage(string message)
    {
        string messageWithNameTag = GetMessageWithNameTag(message);
        taskLoggingHelper.LogMessage(MessageImportance.Low, messageWithNameTag);
    }

    public void LogTaskError(string message)
    {
        string messageWithNameTag = GetMessageWithNameTag(message);
        taskLoggingHelper.LogError(messageWithNameTag);
    }

    public void LogUserError(string errorMessage, string featureFile, int errorLine, int errorLinePosition)
    {
        taskLoggingHelper.LogError(null, null, null, featureFile, errorLine, errorLinePosition, 0, 0, errorMessage);
    }

    public void LogUserWarning(string warningMessage)
    {
        taskLoggingHelper.LogWarning(warningMessage);
    }
}