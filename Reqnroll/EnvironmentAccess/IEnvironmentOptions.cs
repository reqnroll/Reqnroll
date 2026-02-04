#nullable enable
using System.Collections.Generic;
using Reqnroll.Configuration;

namespace Reqnroll.EnvironmentAccess;

/// <summary>
/// Well-known settings that are provided by the environment through an <see cref="IEnvironmentWrapper"/> implementation.
/// </summary>
public interface IEnvironmentOptions
{
    bool IsDryRun { get; }

    string? BindingsOutputFilepath { get; }

    bool IsRunningInContainer { get; }

    bool FormattersDisabled { get; }

    string? FormattersJson { get; }

    IDictionary<string, string> FormatterSettings { get; }

    /// <summary>
    /// Override for the trace level, set via the REQNROLL_TRACE_LEVEL environment variable.
    /// Returns null if the environment variable is not set or has an invalid value.
    /// </summary>
    TraceLevel? TraceLevel { get; }
}
