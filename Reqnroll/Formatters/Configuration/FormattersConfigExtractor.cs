using Reqnroll.Configuration.JsonConfig;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

/// <summary>
/// Utility class for extracting formatters configuration from JSON content.
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
            var jsonConfig = JsonSerializer.Deserialize(jsonContent, FormattersConfigurationSourceGenerator.Default.FormattersElement);
            return ConvertFormattersElement(jsonConfig);
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

        if (formatters.Formatters != null)
            foreach (var kvp in formatters.Formatters)
            {
                result[kvp.Key] = ConvertFormatterOptions(kvp.Value);
            }
    
        return result;
    }

    internal static FormatterConfiguration ConvertFormatterOptions(FormatterOptionsElement options)
    {
        if (options == null)
            return new FormatterConfiguration();

        var config = new FormatterConfiguration
        {
            OutputFilePath = options.OutputFilePath
        };

        // Process additional options captured by JsonExtensionData
        if (options.AdditionalOptions != null)
            foreach (var kvp in options.AdditionalOptions)
            {
                var value = ConvertJsonElement(kvp.Value);
                if (value != null)
                {
                    config.AdditionalSettings[kvp.Key] = value;
                }
            }

        return config;
    }

    private static object ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                return element.TryGetInt64(out var l) ? (object)l : element.GetDouble();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object>();
                foreach (var prop in element.EnumerateObject())
                    dict[prop.Name] = ConvertJsonElement(prop.Value);
                return dict;
            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                    list.Add(ConvertJsonElement(item));
                return list;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                throw new ArgumentOutOfRangeException($"Unexpected JsonElement.ValueKind: {element.ValueKind}. Formatter configuration only supports strings, numbers, booleans, null, nested objects and arrays of the above.");
        }
    }
}
