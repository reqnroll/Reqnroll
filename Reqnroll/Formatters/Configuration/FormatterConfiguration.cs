#nullable enable

using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

/// <summary>
/// Represents the configuration for a formatter with type-safe access to known properties
/// and extensibility for custom settings.
/// </summary>
public class FormatterConfiguration
{
    /// <summary>
    /// The output file path for the formatter. May be null if not configured.
    /// </summary>
    public string? OutputFilePath { get; set; }

    /// <summary>
    /// Additional settings for the formatter that are not explicitly defined as properties.
    /// </summary>
    public IDictionary<string, object> AdditionalSettings { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a FormatterConfiguration from a dictionary representation.
    /// </summary>
    /// <param name="dictionary">The dictionary containing formatter configuration values.</param>
    /// <returns>A new FormatterConfiguration instance, or null if the dictionary is null.</returns>
    public static FormatterConfiguration? FromDictionary(IDictionary<string, object>? dictionary)
    {
        if (dictionary == null)
            return null;

        var config = new FormatterConfiguration();
        var additionalSettings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in dictionary)
        {
            if (string.Equals(kvp.Key, "outputFilePath", StringComparison.OrdinalIgnoreCase))
            {
                config.OutputFilePath = kvp.Value?.ToString();
            }
            else if (kvp.Value != null)
            {
                additionalSettings[kvp.Key] = kvp.Value;
            }
        }

        config.AdditionalSettings = additionalSettings;
        return config;
    }

    /// <summary>
    /// Converts this FormatterConfiguration back to a dictionary representation.
    /// Used for backward compatibility with legacy APIs.
    /// </summary>
    /// <returns>A dictionary containing all configuration values.</returns>
    public IDictionary<string, object> ToDictionary()
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (OutputFilePath != null)
        {
            result["outputFilePath"] = OutputFilePath;
        }

        foreach (var kvp in AdditionalSettings)
        {
            result[kvp.Key] = kvp.Value;
        }

        return result;
    }

    /// <summary>
    /// Gets a configuration value by key, checking both known properties and additional settings.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value if the key is not found.</param>
    /// <returns>The configuration value or the default value.</returns>
    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        if (string.Equals(key, "outputFilePath", StringComparison.OrdinalIgnoreCase))
        {
            if (OutputFilePath is T typedValue)
                return typedValue;
            return defaultValue;
        }

        if (AdditionalSettings.TryGetValue(key, out var value))
        {
            if (value is T typedValue)
                return typedValue;

            // Try conversion for common types
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Merges settings from another FormatterConfiguration into this one.
    /// Only non-null values from the other configuration will override values in this configuration.
    /// This allows partial overrides where only specified settings are changed.
    /// </summary>
    /// <param name="other">The configuration to merge from. Null values are ignored.</param>
    public void MergeFrom(FormatterConfiguration? other)
    {
        if (other == null)
            return;

        // Only override OutputFilePath if the other configuration has it set
        if (other.OutputFilePath != null)
        {
            OutputFilePath = other.OutputFilePath;
        }

        // Merge additional settings - other's values override this's values
        foreach (var kvp in other.AdditionalSettings)
        {
            AdditionalSettings[kvp.Key] = kvp.Value;
        }
    }
}
