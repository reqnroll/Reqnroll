using Microsoft.Extensions.Logging;
using System;

namespace Reqnroll.RuntimeTests.Diagnostics;

public class TestLogger<T> : ILogger<T>
{
    public LogLevel MinLevel { get; set; } = LogLevel.Debug;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= MinLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        throw new NotImplementedException();
    }
}
