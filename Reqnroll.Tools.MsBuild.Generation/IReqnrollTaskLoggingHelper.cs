using Microsoft.Build.Utilities;

namespace Reqnroll.Tools.MsBuild.Generation;

/// <summary>
/// The logging helper interface used by MSBuild tasks. Do not use <see cref="TaskLoggingHelper"/> directly!
/// </summary>
public interface IReqnrollTaskLoggingHelper
{
    /// <summary>
    /// Logs a message from the task (prefixed), verbosity: normal
    /// </summary>
    void LogTaskMessage(string message);
    /// <summary>
    /// Logs a message from the task (prefixed), verbosity: diagnostic
    /// </summary>
    void LogTaskDiagnosticMessage(string message);
    /// <summary>
    /// Logs an error from the task (prefixed), verbosity: minimal
    /// </summary>
    void LogTaskError(string message);
    /// <summary>
    /// Has the task logged any errors through this logging helper object?
    /// </summary>
    bool HasLoggedErrors { get; }

    /// <summary>
    /// Logs a user error from the task (not prefixed), verbosity: minimal
    /// </summary>
    void LogUserError(string errorMessage, string featureFile, int errorLine, int errorLinePosition);

    /// <summary>
    /// Logs a user warning from the task (not prefixed), verbosity: minimal
    /// </summary>
    void LogUserWarning(string warningMessage);
}