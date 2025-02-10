using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection.Logging;

public class ReqnrollLogger : ILogger
{
    private static readonly Dictionary<LogLevel, string> LogLevelStrings = new()
    {
        { LogLevel.Trace, nameof(LogLevel.Trace) },
        { LogLevel.Debug, nameof(LogLevel.Debug) },
        { LogLevel.Information, nameof(LogLevel.Information) },
        { LogLevel.Warning, nameof(LogLevel.Warning) },
        { LogLevel.Error, nameof(LogLevel.Error) },
        { LogLevel.Critical, nameof(LogLevel.Critical) }
    };

    private readonly IReqnrollOutputHelper _outputHelper;
    private readonly string? _categoryName;
    private readonly ReqnrollLoggerOptions _options;
    private readonly LoggerExternalScopeProvider _scopeProvider;

    public static ILogger CreateLogger(IReqnrollOutputHelper outputHelper) => new ReqnrollLogger(outputHelper, new LoggerExternalScopeProvider(), string.Empty);
    
    public static ILogger<T> CreateLogger<T>(IReqnrollOutputHelper outputHelper) => new ReqnrollLogger<T>(outputHelper, new LoggerExternalScopeProvider());

    public ReqnrollLogger(IReqnrollOutputHelper outputHelper, LoggerExternalScopeProvider scopeProvider, string? categoryName, bool appendScope)
        : this(outputHelper, scopeProvider, categoryName, options: new ReqnrollLoggerOptions { IncludeScopes = appendScope })
    {
    }

    public ReqnrollLogger(IReqnrollOutputHelper outputHelper, LoggerExternalScopeProvider scopeProvider, string? categoryName, ReqnrollLoggerOptions? options = null)
    {
        _outputHelper = outputHelper;
        _scopeProvider = scopeProvider;
        _categoryName = categoryName;
        _options = options ?? new();
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

#if NET8_0
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _scopeProvider.Push(state);
#else
    public IDisposable BeginScope<TState>(TState state) => _scopeProvider.Push(state);
#endif

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var sb = new StringBuilder();

        if (_options.TimestampFormat is not null)
        {
            var now = _options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
            var timestamp = now.ToString(_options.TimestampFormat, _options.Culture);
            sb.Append(timestamp).Append(' ');
        }

        if (_options.IncludeLogLevel)
        {
            sb.Append(GetLogLevelString(logLevel)).Append(' ');
        }

        if (_options.IncludeCategory)
        {
            sb.Append('[').Append(_categoryName).Append("] ");
        }

        sb.Append(formatter(state, exception));

        if (exception is not null)
        {
            sb.Append('\n').Append(exception);
        }

        // Append scopes
        if (_options.IncludeScopes)
        {
            _scopeProvider.ForEachScope((scope, st) =>
            {
                st.Append("\n => ");
                st.Append(scope);
            }, sb);
        }

        try
        {
            _outputHelper.WriteLine(sb.ToString());
        }
        catch
        {
            // This can happen when the test is not active
        }
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        if (LogLevelStrings.TryGetValue(logLevel, out string? logLevelString))
        {
            return logLevelString;
        }

        throw new ArgumentOutOfRangeException(nameof(logLevel));
    }
}