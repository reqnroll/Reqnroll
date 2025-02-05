namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface ITaskLoggingWrapper
    {
        void LogMessage(string message);

        void LogDiagnosticMessage(string message);

        void LogError(string message);

        bool HasLoggedErrors();
    }
}
