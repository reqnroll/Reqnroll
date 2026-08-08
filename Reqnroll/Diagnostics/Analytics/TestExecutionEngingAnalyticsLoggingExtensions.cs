#nullable enable

using Microsoft.Extensions.Logging;
using Reqnroll.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Diagnostics.Analytics;

internal static partial class TestExecutionEngingAnalyticsLoggingExtensions
{
    public static void LogReqnrollExecutionCompleted(this ILogger<TestExecutionEngine> logger, AttributeBag additionalAttributes)
    {
        using var _ = logger.BeginScope(additionalAttributes.AsDictionary());

        logger.LogReqnrollExecutionCompleted();
    }

    [LoggerMessage("Test run completed", EventId = 1001, Level = LogLevel.Information)]
    public static partial void LogReqnrollExecutionCompleted(this ILogger<TestExecutionEngine> logger);
}
