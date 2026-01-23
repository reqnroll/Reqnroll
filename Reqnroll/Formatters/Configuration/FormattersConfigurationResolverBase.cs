using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public abstract class FormattersConfigurationResolverBase : IFormattersConfigurationResolverBase
{
    protected const string FORMATTERS_KEY = "formatters";

    public IDictionary<string, IDictionary<string, object>> Resolve()
    {
        var result = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
        JsonDocument jsonDocument = GetJsonDocument();

        if (jsonDocument != null)
        {
            ProcessJsonDocument(jsonDocument, result);
        }

        return result;
    }

    protected abstract JsonDocument GetJsonDocument();

    protected virtual void ProcessJsonDocument(JsonDocument jsonDocument, Dictionary<string, IDictionary<string, object>> result)
    {
        if (jsonDocument.RootElement.TryGetProperty(FORMATTERS_KEY, out JsonElement formatters))
        {
            var formattersDict = GetConfigValue(formatters) as IDictionary<string, object>;
            if (formattersDict != null)
            {
                foreach (var kvp in formattersDict)
                {
                    if (kvp.Value is IDictionary<string, object> formatterConfig)
                    {
                        result.Add(kvp.Key, formatterConfig);
                    }
                }
            }
        }
    }

    private object GetConfigValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => GetConfigValue(prop.Value), StringComparer.OrdinalIgnoreCase),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(GetConfigValue).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element
        };
    }
}