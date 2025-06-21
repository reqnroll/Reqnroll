using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public abstract class FormattersConfigurationResolverBase : IFormattersConfigurationResolverBase
{
    protected const string FORMATTERS_KEY = "formatters";

    public IDictionary<string, IDictionary<string, string>> Resolve()
    {
        var result = new Dictionary<string, IDictionary<string, string>>();
        JsonDocument jsonDocument = GetJsonDocument();
        
        if (jsonDocument != null)
        {
            ProcessJsonDocument(jsonDocument, result);
        }
        
        return result;
    }
    
    protected abstract JsonDocument GetJsonDocument();
    
    protected virtual void ProcessJsonDocument(JsonDocument jsonDocument, Dictionary<string, IDictionary<string, string>> result)
    {
        if (jsonDocument.RootElement.TryGetProperty(FORMATTERS_KEY, out JsonElement formatters))
        {
            foreach(JsonProperty formatterProperty in formatters.EnumerateObject())
            {
                var configValues = new Dictionary<string, string>();
                
                if (formatterProperty.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty configProperty in formatterProperty.Value.EnumerateObject())
                    {
                        // If the configuration value is a simple type, store its string representation
                        if (configProperty.Value.ValueKind == JsonValueKind.String ||
                            configProperty.Value.ValueKind == JsonValueKind.Number ||
                            configProperty.Value.ValueKind == JsonValueKind.True ||
                            configProperty.Value.ValueKind == JsonValueKind.False)
                        {
                            configValues.Add(configProperty.Name, configProperty.Value.ToString());
                        }
                        else
                        {
                            // For complex types, store the raw JSON text
                            configValues.Add(configProperty.Name, configProperty.Value.GetRawText());
                        }
                    }
                }
                
                result.Add(formatterProperty.Name, configValues);
            }
        }
    }
}