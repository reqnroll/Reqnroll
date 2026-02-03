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
    /// Deserializes JSON content and extracts the formatters configuration as a dictionary.
    /// </summary>
    /// <param name="jsonContent">The JSON content to parse.</param>
    /// <returns>A dictionary of formatter configurations, or an empty dictionary if parsing fails or no formatters are defined.</returns>
    public static IDictionary<string, IDictionary<string, object>> ExtractFormatters(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            return new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var jsonConfig = JsonSerializer.Deserialize(jsonContent, JsonConfigurationSourceGenerator.Default.JsonConfig);
            return ConvertFormattersElement(jsonConfig?.Formatters);
        }
        catch (JsonException)
        {
            return new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Converts a FormattersElement to the dictionary format used by the formatters configuration system.
    /// </summary>
    /// <param name="formatters">The FormattersElement to convert.</param>
    /// <returns>A dictionary of formatter configurations.</returns>
    public static IDictionary<string, IDictionary<string, object>> ConvertFormattersElement(FormattersElement formatters)
    {
        var result = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

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
                    var configValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var property in kvp.Value.EnumerateObject())
                    {
                        configValues[property.Name] = GetConfigValue(property.Value);
                    }
                    result[kvp.Key] = configValues;
                }
                else
                {
                    // Non-object values get an empty config dictionary
                    result[kvp.Key] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        return result;
    }

    private static IDictionary<string, object> ConvertFormatterOptions(FormatterOptionsElement options)
    {
        var configValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (options.OutputFilePath != null)
        {
            configValues["outputFilePath"] = options.OutputFilePath;
        }

        // Process additional options captured by JsonExtensionData
        if (options.AdditionalOptions != null)
        {
            foreach (var kvp in options.AdditionalOptions)
            {
                configValues[kvp.Key] = GetConfigValue(kvp.Value);
            }
        }

        return configValues;
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
