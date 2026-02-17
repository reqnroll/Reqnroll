using System;
using System.IO;

namespace Reqnroll.Tools.MsBuild.Generation;

public class ExceptionTaskLogger(IReqnrollTaskLoggingHelper log) : IExceptionTaskLogger
{
    public void LogException(Exception exception)
    {
        if (exception.InnerException is FileLoadException fileLoadException)
        {
            log.LogTaskError($"FileLoadException Filename: {fileLoadException.FileName}");
            log.LogTaskError($"FileLoadException FusionLog: {fileLoadException.FusionLog}");
            log.LogTaskError($"FileLoadException Message: {fileLoadException.Message}");
        }

        if (exception.InnerException is { } innerException)
        {
            log.LogTaskError(innerException.ToString());
        }

        log.LogTaskError(exception.ToString());
    }
}