using System;
using System.Collections.Generic;
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
            foreach(JsonProperty formatterProperty in formatters.EnumerateObject())
            {
                var configValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                if (formatterProperty.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty configProperty in formatterProperty.Value.EnumerateObject())
                    {
                        configValues.Add(configProperty.Name, GetConfigValue(configProperty.Value));
                    }
                }

                result.Add(formatterProperty.Name, configValues);
            }
        }
    }

    private object GetConfigValue(JsonElement valueElement)
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
        }

        // if value is an embedded JSON object or array, we keep it as it is
        return valueElement;
    }
}