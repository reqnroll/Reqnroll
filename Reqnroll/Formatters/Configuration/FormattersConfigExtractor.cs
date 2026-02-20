using Reqnroll.Configuration.JsonConfig;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

/// <summary>
/// Utility class for extracting formatters configuration from JSON content
/// using the centralized JsonConfig deserialization.
/// </summary>
public static class FormattersConfigExtractor
{
    /// <summary>
    /// Deserializes JSON content and extracts the formatters configuration as typed FormatterConfiguration objects.
    /// </summary>
    /// <param name="jsonContent">The JSON content to parse.</param>
    /// <returns>A dictionary of formatter configurations, or an empty dictionary if parsing fails or no formatters are defined.</returns>
    public static IDictionary<string, FormatterConfiguration> ExtractFormatters(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            return new Dictionary<string, FormatterConfiguration>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var jsonConfig = JsonSerializer.Deserialize(jsonContent, JsonConfigurationSourceGenerator.Default.JsonConfig);
            return ConvertFormattersElement(jsonConfig?.Formatters);
        }
        catch (JsonException)
        {
            return new Dictionary<string, FormatterConfiguration>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Converts a FormattersElement to typed FormatterConfiguration objects.
    /// </summary>
    /// <param name="formatters">The FormattersElement to convert.</param>
    /// <returns>A dictionary of formatter configurations.</returns>
    public static IDictionary<string, FormatterConfiguration> ConvertFormattersElement(FormattersElement formatters)
    {
        var result = new Dictionary<string, FormatterConfiguration>(StringComparer.OrdinalIgnoreCase);

        if (formatters == null)
            return result;

        // Process known formatters
        if (formatters.Html != null)
        {
            result["html"] = ConvertFormatterOptions(formatters.Html);
        }

        if (formatters.Message != null)
        {
            result["message"] = ConvertFormatterOptions(formatters.Message);
        }

        // Process additional/custom formatters captured by JsonExtensionData
        if (formatters.AdditionalFormatters != null)
        {
            foreach (var kvp in formatters.AdditionalFormatters)
            {
                if (kvp.Value.ValueKind == JsonValueKind.Object)
                {
                    result[kvp.Key] = ConvertJsonElementToFormatterConfiguration(kvp.Value);
                }
                else
                {
                    // Non-object values get an empty config
                    result[kvp.Key] = new FormatterConfiguration();
                }
            }
        }

        return result;
    }

    private static FormatterConfiguration ConvertFormatterOptions(FormatterOptionsElement options)
    {
        var config = new FormatterConfiguration
        {
            OutputFilePath = options.OutputFilePath
        };

        // Process additional options captured by JsonExtensionData
        if (options.AdditionalOptions != null)
        {
            foreach (var kvp in options.AdditionalOptions)
            {
                var value = GetConfigValue(kvp.Value);
                if (value != null)
                {
                    config.AdditionalSettings[kvp.Key] = value;
                }
            }
        }

        return config;
    }

    private static FormatterConfiguration ConvertJsonElementToFormatterConfiguration(JsonElement element)
    {
        var config = new FormatterConfiguration();

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, "outputFilePath", StringComparison.OrdinalIgnoreCase))
            {
                config.OutputFilePath = property.Value.GetString();
            }
            else
            {
                var value = GetConfigValue(property.Value);
                if (value != null)
                {
                    config.AdditionalSettings[property.Name] = value;
                }
            }
        }

        return config;
    }

    private static object GetConfigValue(JsonElement valueElement)
    {
        switch (valueElement.ValueKind)
        {
            case JsonValueKind.String:
                return valueElement.GetString();
            case JsonValueKind.False:
            case JsonValueKind.True:
                return valueElement.GetBoolean();
            case JsonValueKind.Number:
                return valueElement.GetDouble();
            case JsonValueKind.Null:
                return null;
            default:
                // For embedded JSON objects or arrays, keep as JsonElement
                return valueElement;
        }
    }
}
