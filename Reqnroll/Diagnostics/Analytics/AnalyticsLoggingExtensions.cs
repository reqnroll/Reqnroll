#nullable enable

using Microsoft.Extensions.Logging;
using Reqnroll.Infrastructure;
using System.Collections.Generic;

namespace Reqnroll.Diagnostics.Analytics;

internal static partial class AnalyticsLoggingExtensions
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Test run completed")]
    public static partial void TestRunCompleted(this ILogger<TestExecutionEngine> logger);

    public static void TestRunCompleted(
        this ILogger<TestExecutionEngine> logger,
        IReadOnlyDictionary<string, object> additionalAttributes)
    {
        using var scope = logger.BeginScope(additionalAttributes);

        logger.TestRunCompleted();
    }
}
