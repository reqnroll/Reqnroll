#nullable enable

using Microsoft.Extensions.Logging;
using System;

namespace Reqnroll.Diagnostics.Analytics;

public interface ITelemetryHost : IDisposable
{
    ILogger<T> CreateLogger<T>();
}