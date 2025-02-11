using System;
using System.Globalization;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection.Logging;

public sealed class ReqnrollLoggerOptions
{
    /// <summary>
    /// Includes scopes when <see langword="true" />.
    /// </summary>
    public bool IncludeScopes { get; set; }

    /// <summary>
    /// Includes category when <see langword="true" />.
    /// </summary>
    public bool IncludeCategory { get; set; }

    /// <summary>
    /// Includes log level when <see langword="true" />.
    /// </summary>
    public bool IncludeLogLevel { get; set; }

    /// <summary>
    /// Gets or sets format string used to format timestamp in logging messages. Defaults to <see langword="null" />.
    /// </summary>
    public string? TimestampFormat { get; set; }

    /// <summary>
    /// Gets or sets indication whether or not UTC timezone should be used to format timestamps in logging messages. Defaults to <see langword="false" />.
    /// </summary>
    public bool UseUtcTimestamp { get; set; }

    private CultureInfo _culture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Defines the culture to use when formatting timestamps in logging messages. Defaults to <see cref="CultureInfo.InvariantCulture" />.
    /// </summary>
    public CultureInfo Culture
    {
        get => _culture;
        set => _culture = value ?? throw new ArgumentNullException(nameof(value), "Culture cannot be set to null.");
    }
}