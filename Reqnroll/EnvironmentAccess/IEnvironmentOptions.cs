#nullable enable
using System.Collections.Generic;

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
}
