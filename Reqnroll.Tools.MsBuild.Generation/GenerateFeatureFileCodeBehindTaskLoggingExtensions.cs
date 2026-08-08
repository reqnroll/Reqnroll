#nullable enable

using Microsoft.Extensions.Logging;

namespace Reqnroll.Tools.MsBuild.Generation;

internal static partial class GenerateFeatureFileCodeBehindTaskLoggingExtensions
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Code generation completed")]
    public static partial void LogGenerateFeatureFilesCompleted(this ILogger<GenerateFeatureFileCodeBehindTask> logger);
}
