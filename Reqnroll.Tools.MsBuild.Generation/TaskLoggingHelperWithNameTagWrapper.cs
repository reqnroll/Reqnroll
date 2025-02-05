using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class TaskLoggingHelperWithNameTagWrapper : ITaskLoggingWrapper
    {
        private readonly TaskLoggingHelper _taskLoggingHelper;

        public TaskLoggingHelperWithNameTagWrapper(TaskLoggingHelper taskLoggingHelper)
        {
            _taskLoggingHelper = taskLoggingHelper;
        }

        public void LogMessage(string message)
        {
            string messageWithNameTag = GetMessageWithNameTag(message);
            _taskLoggingHelper.LogMessage(messageWithNameTag);
        }

        public void LogDiagnosticMessage(string message)
        {
            string messageWithNameTag = GetMessageWithNameTag(message);
            _taskLoggingHelper.LogMessage(MessageImportance.Low, messageWithNameTag);
        }

        public void LogError(string message)
        {
            string messageWithNameTag = GetMessageWithNameTag(message);
            _taskLoggingHelper.LogError(messageWithNameTag);
        }

        public bool HasLoggedErrors() => _taskLoggingHelper.HasLoggedErrors;

        public string GetMessageWithNameTag(string message)
        {
            string fullMessage = $"[Reqnroll] {message}";
            return fullMessage;
        }
    }
}
